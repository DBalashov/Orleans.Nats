using NATS.Client.ObjectStore;

namespace Orleans.Nats.Interfaces;

interface INatsContextWrapper
{
    INatsObjContext Context { get; }

    Task<INatsObjStore> GetStore(string bucketId);

    Task DeleteStore(string bucketId);
}