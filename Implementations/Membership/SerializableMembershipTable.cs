using Orleans.Runtime;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Orleans.Nats.Implementations.Membership;

public sealed class SerializableMembershipTable
{
    public Dictionary<string, SerializableMembershipEntry> Members { get; set; }

    public string Etag    { get; set; }
    public int    Version { get; set; }

    public override string ToString() => $"Version={Version}, ETag={Etag}, {Members.Count} entries";
}

public sealed class SerializableMembershipEntry
{
    public string                       SiloAddress  { get; set; }
    public SiloStatus                   Status       { get; set; }
    public Dictionary<string, DateTime> SuspectTimes { get; set; }
    public int                          ProxyPort    { get; set; }
    public string                       HostName     { get; set; }
    public string                       SiloName     { get; set; }

    public string RoleName   { get; set; }
    public int    UpdateZone { get; set; }
    public int    FaultZone  { get; set; }

    public DateTime StartTime    { get; set; }
    public DateTime IAmAliveTime { get; set; }
    
    public override string ToString() => $"SiloAddress={SiloAddress}, Status={Status}, StartTime={StartTime}, IAmAliveTime={IAmAliveTime}";
}