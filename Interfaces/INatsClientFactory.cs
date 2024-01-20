using NATS.Client.ObjectStore;

namespace Orleans.Nats.Interfaces;

interface INatsClientFactory
{
    INatsObjContext CreateContext();
}