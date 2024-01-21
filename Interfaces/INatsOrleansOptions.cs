using NATS.Client.Core;
using Orleans.Nats.Models;

namespace Orleans.Nats.Interfaces;

public interface INatsOrleansOptions
{
    NatsOpts Options { get; }
    
    GetBucketNameDelegate GetBucketName { get; }
}