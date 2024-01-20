using NATS.Client.Core;

namespace Orleans.Nats.Models;

public delegate string GetMembershipBucketName(string clusterId, string serviceId);

public delegate string GetGrainStorageBucketName(string clusterId, string serviceId);

public delegate string GetReminderBucketName(string clusterId, string serviceId);

public sealed class NatsOrleansOptions(NatsOpts? Options = null)
{
    public NatsOpts Options { get; } = Options ?? new NatsOpts();

    public string MembershipObjectName { get; private set; } = "MembershipTableData";

    /// <summary> Default name will generated as "Orleans-{clusterId}-{serviceId}-Membership" </summary>
    public GetMembershipBucketName MembershipBucketName { get; private set; } = (clusterId, serviceId) => $"Orleans-{clusterId}-{serviceId}-Membership";

    /// <summary> Default name will generated as "Orleans-{clusterId}-{serviceId}-Grains" </summary>
    public GetGrainStorageBucketName GrainStorageBucketName { get; private set; } = (clusterId, serviceId) => $"Orleans-{clusterId}-{serviceId}-Grains";
    
    /// <summary> Default name will generated as "Orleans-{clusterId}-{serviceId}-Reminders" </summary>
    public GetReminderBucketName ReminderBucketName { get; private set; } = (clusterId, serviceId) => $"Orleans-{clusterId}-{serviceId}-Reminders";

    public NatsOrleansOptions WithMembershipBucketName(GetMembershipBucketName bucketNameDelegate)
    {
        ArgumentNullException.ThrowIfNull(bucketNameDelegate);
        MembershipBucketName = bucketNameDelegate;
        return this;
    }

    public NatsOrleansOptions WithMembershipObjectName(string membershipObjectName)
    {
        ArgumentException.ThrowIfNullOrEmpty(membershipObjectName);
        MembershipObjectName = membershipObjectName;
        return this;
    }

    public NatsOrleansOptions WithGrainStorageBucketName(GetGrainStorageBucketName bucketNameDelegate)
    {
        ArgumentNullException.ThrowIfNull(bucketNameDelegate);
        GrainStorageBucketName = bucketNameDelegate;
        return this;
    }
    
    public NatsOrleansOptions WithReminderBucketName(GetReminderBucketName bucketNameDelegate)
    {
        ArgumentNullException.ThrowIfNull(bucketNameDelegate);
        ReminderBucketName = bucketNameDelegate;
        return this;
    }

    public static NatsOrleansOptions Default => new();
}