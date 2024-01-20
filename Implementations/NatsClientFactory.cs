using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.ObjectStore;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;

namespace Orleans.Nats.Implementations;

sealed class NatsClientFactory(NatsOrleansOptions options) : INatsClientFactory
{
    public NatsContextWrapper CreateContext()
    {
        var client = new NatsConnection(options.Options);
        var js     = new NatsJSContext(client);
        return new NatsContextWrapper(new NatsObjContext(js));
    }
}