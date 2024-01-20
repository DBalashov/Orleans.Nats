using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NATS.Client.ObjectStore;
using NATS.Client.ObjectStore.Models;
using Orleans.Configuration;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;
using Orleans.Runtime;

namespace Orleans.Nats.Implementations.Reminders;

sealed class NatsReminderService(NatsContextWrapper           wrapper,
                                 NatsOrleansOptions           options,
                                 ILogger<NatsReminderService> logger,
                                 IOptions<ClusterOptions>     clusterOptions) : INatsReminderService
{
    readonly string bucketId = options.ReminderBucketName(clusterOptions.Value.ClusterId, clusterOptions.Value.ServiceId);

    public async Task Init()
    {
        // fake call to create bucket
        await getStore();
    }

    public async Task<ReminderEntry[]> Find(FindReminderPredicate filter)
    {
        var store = await getStore();
        var items = new List<ReminderEntry>();
        await foreach (var item in store.ListAsync())
        {
            if (!NatsReminderMetadata.TryParse(item.Metadata, out var meta) || !filter(meta)) continue;

            var bytes = await store.GetBytesAsync(item.Name);
            var entry = bytes.ToEntry();
            items.Add(entry);
        }

        return items.ToArray();
    }

    public async Task<ReminderEntry> Get(GrainId grainId, string reminderName)
    {
        var store      = await getStore();
        var grainIdStr = grainId.ToString();
        await foreach (var item in store.ListAsync())
        {
            if (!NatsReminderMetadata.TryParse(item.Metadata, out var meta)) continue;
            if (meta.GrainId != grainIdStr || meta.ReminderName != reminderName) continue;

            var bytes = await store.GetBytesAsync(item.Name);
            var entry = bytes.ToEntry();
            return entry;
        }

        return new ReminderEntry();
    }

    public async Task<string> Put(ReminderEntry entry)
    {
        var store = await getStore();

        using var stm = new MemoryStream(entry.ToBytes());
        await store.PutAsync(new ObjectMetadata()
                             {
                                 Metadata = new NatsReminderMetadata(entry.GrainId.ToString(), entry.ReminderName, entry.GrainId.GetUniformHashCode()).ToMetadata(),
                                 Name     = entry.GrainId.GetReminderNormalizedName(entry.ReminderName)
                             },
                             stm);

        return entry.ETag;
    }
    
    public async Task<bool> Remove(ReminderEntry entry)
    {
        var store = await getStore();

        try
        {
            await store.DeleteAsync(entry.GrainId.GetReminderNormalizedName(entry.ReminderName));
            return true;
        }
        catch (NatsObjNotFoundException)
        {
            return false;
        }
    }

    async Task<INatsObjStore> getStore()
    {
        try
        {
            return await wrapper.Context.GetObjectStoreAsync(bucketId);
        }
        catch (NatsJSApiException e) when (e.Error.Code == 404)
        {
            await wrapper.Context.CreateObjectStoreAsync(bucketId);
            return await wrapper.Context.GetObjectStoreAsync(bucketId);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(getStore)}: {e.Message}");
            throw;
        }
    }
}