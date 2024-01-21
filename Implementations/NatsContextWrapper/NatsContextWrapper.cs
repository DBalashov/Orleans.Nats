using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.ObjectStore;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats.Implementations;

abstract class NatsContextWrapper : INatsContextWrapper
{
    readonly ILogger         logger;
    readonly INatsObjContext context;
    public   INatsObjContext Context => context;

    protected NatsContextWrapper(INatsOrleansOptions options, ILogger logger)
    {
        this.logger = logger;
        var client = new NatsConnection(options.Options);
        var js     = new NatsJSContext(client);
        context = new NatsObjContext(js);
    }

    public async Task<INatsObjStore> GetStore(string bucketId)
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
            logger.LogError($"{nameof(GetStore)}: {e.Message}");
            throw;
        }
    }

    public async Task DeleteStore(string bucketId)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await Context.DeleteObjectStore(bucketId, cts.Token);
    }
}