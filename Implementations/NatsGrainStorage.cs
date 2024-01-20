using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NATS.Client.ObjectStore;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Nats.Implementations;

sealed class NatsGrainStorage(INatsObjContext           context,
                              IOptions<ClusterOptions>  clusterOptions,
                              ILogger<NatsGrainStorage> logger) : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    readonly string bucketId = "Orleans-" + clusterOptions.Value.ClusterId + '-' + clusterOptions.Value.ServiceId + "-Grains";

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var store      = await getStore();
        var objectName = grainId.GetObjectNormalizedName<T>(stateName);
        logger.LogDebug($"{nameof(ReadStateAsync)}: Object {grainState} ({grainId}) => {objectName}");
        try
        {
            var bytes = await store.GetBytesAsync(objectName, CancellationToken.None);
            var r     = JsonSerializer.Deserialize<T>(bytes);
            if (r == null)
            {
                logger.LogWarning($"{nameof(ReadStateAsync)}: Object {grainState} ({grainId}): can't deserialize, ignore");
                grainState.RecordExists = false;
            }
            else
            {
                grainState.State        = r;
                grainState.RecordExists = true;
            }
        }
        catch (NatsObjNotFoundException)
        {
            logger.LogDebug($"{nameof(ReadStateAsync)}: Object {grainState} ({grainId}) not found, ignore");
            grainState.RecordExists = false;
        }
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var store      = await getStore();
        var objectName = grainId.GetObjectNormalizedName<T>(stateName);
        logger.LogDebug($"{nameof(WriteStateAsync)}: Object {grainState} ({grainId}) => {objectName}");

        var bytes = JsonSerializer.SerializeToUtf8Bytes(grainState.State);
        await store.PutAsync(objectName, bytes, CancellationToken.None);
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var store      = await getStore();
        var objectName = grainId.GetObjectNormalizedName<T>(stateName);
        logger.LogDebug($"{nameof(ClearStateAsync)}: Object {grainState} ({grainId}) => {objectName}");
        try
        {
            await store.DeleteAsync(objectName);
        }
        catch (NatsObjNotFoundException)
        {
        }
    }

    async Task<INatsObjStore> getStore()
    {
        try
        {
            return await context.GetObjectStoreAsync(bucketId);
        }
        catch (NatsJSApiException e) when (e.Error.Code == 404)
        {
            await context.CreateObjectStoreAsync(bucketId);
            return await context.GetObjectStoreAsync(bucketId);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(getStore)}: {e.Message}");
            throw;
        }
    }

    async Task Init(CancellationToken ct)
    {
        // fake call to create bucket
        await getStore();
    }

    public void Participate(ISiloLifecycle lifecycle) =>
        lifecycle.Subscribe<NatsGrainStorage>(ServiceLifecycleStage.ApplicationServices, Init);
}