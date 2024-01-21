using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.ObjectStore;
using NATS.Client.ObjectStore.Models;
using Orleans.Configuration;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;
using Orleans.Runtime;

namespace Orleans.Nats.Implementations.Reminders;

sealed class NatsReminderService(NatsContextRemindersWrapper  wrapper,
                                 NatsRemindersOptions         options,
                                 ILogger<NatsReminderService> logger,
                                 IOptions<ClusterOptions>     clusterOptions) : INatsReminderService
{
    readonly string bucketId = options.GetBucketName(clusterOptions.Value.ClusterId, clusterOptions.Value.ServiceId);

    public async Task Init()
    {
        // fake call to create bucket
        await wrapper.GetStore(bucketId);
    }

    public async Task<ReminderEntry[]> Find(FindReminderPredicate filter)
    {
        var store = await wrapper.GetStore(bucketId);
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
        var store      = await wrapper.GetStore(bucketId);
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
        var store = await wrapper.GetStore(bucketId);

        using var stm = new MemoryStream(entry.ToBytes());
        await store.PutAsync(new ObjectMetadata()
                             {
                                 Metadata = new NatsReminderMetadata(entry.GrainId.ToString(), entry.ReminderName, entry.GrainId.GetUniformHashCode()).ToMetadata(),
                                 Name     = getReminderNormalizedName(entry.GrainId, entry.ReminderName)
                             },
                             stm);

        return entry.ETag;
    }
    
    public async Task<bool> Remove(ReminderEntry entry)
    {
        var store = await wrapper.GetStore(bucketId);
        try
        {
            await store.DeleteAsync(getReminderNormalizedName(entry.GrainId, entry.ReminderName));
            return true;
        }
        catch (NatsObjNotFoundException)
        {
            return false;
        }
    }
    
    string getReminderNormalizedName(GrainId grainId, string reminderName) =>
        string.Join('-',
                    grainId.ToString().Replace('/', '-'),
                    reminderName.Replace('/', '-').Replace('.', '-').Replace('\\', '-'));
}