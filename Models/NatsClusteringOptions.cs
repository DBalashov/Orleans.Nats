using NATS.Client.Core;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats.Models;

public sealed class NatsClusteringOptions(NatsOpts? Options = null) : INatsOrleansOptions
{
    public NatsOpts Options { get; } = Options ?? new NatsOpts();

    public string ObjectName { get; private set; } = "MembershipTableData";

    /// <summary> Default name will generated as "Orleans-{clusterId}-{serviceId}-Membership" </summary>
    public GetBucketNameDelegate GetBucketName { get; private set; } = (clusterId, serviceId) => $"Orleans-{clusterId}-{serviceId}-Membership";

    public NatsClusteringOptions WithBucketName(GetBucketNameDelegate bucketNameDelegate)
    {
        ArgumentNullException.ThrowIfNull(bucketNameDelegate);
        GetBucketName = bucketNameDelegate;
        return this;
    }

    /// <summary> by default "MembershipTableData" </summary>
    public NatsClusteringOptions WithObjectName(string membershipObjectName)
    {
        ArgumentException.ThrowIfNullOrEmpty(membershipObjectName);
        ObjectName = membershipObjectName;
        return this;
    }

    public static NatsClusteringOptions Default => new();
}