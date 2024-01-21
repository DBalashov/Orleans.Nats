using NATS.Client.Core;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats.Models;

public sealed class NatsGrainStorageOptions(NatsOpts? Options = null) : INatsOrleansOptions
{
    public NatsOpts Options { get; } = Options ?? new NatsOpts();

    /// <summary> Default name will generated as "Orleans-{clusterId}-{serviceId}-Grains" </summary>
    public GetBucketNameDelegate GetBucketName { get; private set; } = (clusterId, serviceId) => $"Orleans-{clusterId}-{serviceId}-Grains";

    public NatsGrainStorageOptions WithBucketName(GetBucketNameDelegate bucketNameDelegate)
    {
        ArgumentNullException.ThrowIfNull(bucketNameDelegate);
        GetBucketName = bucketNameDelegate;
        return this;
    }

    public static NatsGrainStorageOptions Default => new();
}