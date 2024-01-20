using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.ObjectStore;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats.Implementations;

sealed class NatsClientFactory(NatsOpts options) : INatsClientFactory
{
    public INatsObjContext CreateContext()
    {
        var client = new NatsConnection(options);
        var js     = new NatsJSContext(client);
        return new NatsObjContext(js);
    }
}