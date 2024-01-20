using NATS.Client.ObjectStore;

namespace Orleans.Nats.Models;

public sealed record NatsContextWrapper(INatsObjContext Context);