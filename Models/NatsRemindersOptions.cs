using NATS.Client.Core;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats.Models;

public delegate string GetBucketNameDelegate(string clusterId, string serviceId);

public sealed class NatsRemindersOptions(NatsOpts? Options = null) : INatsOrleansOptions
{
    public NatsOpts Options { get; } = Options ?? new NatsOpts();

    /// <summary> Default name will generated as "Orleans-{clusterId}-{serviceId}-Reminders" </summary>
    public GetBucketNameDelegate GetBucketName { get; private set; } = (clusterId, serviceId) => $"Orleans-{clusterId}-{serviceId}-Reminders";

    public NatsRemindersOptions WithBucketName(GetBucketNameDelegate bucketNameDelegate)
    {
        ArgumentNullException.ThrowIfNull(bucketNameDelegate);
        GetBucketName = bucketNameDelegate;
        return this;
    }

    public static NatsRemindersOptions Default => new();
}