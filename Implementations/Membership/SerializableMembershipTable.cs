using System.Text.Json;
using Orleans.Runtime;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Orleans.Nats.Implementations.Membership;

public sealed class SerializableMembershipTable
{
    public Dictionary<string, SerializableMembershipEntry> Members { get; set; }

    public string Etag    { get; set; }
    public int    Version { get; set; }

    [Obsolete("For serialization/deserialization only", true)]
    public SerializableMembershipTable()
    {
    }

    public SerializableMembershipTable(MembershipTableData tableData)
    {
        Members = tableData.Members.ToDictionary(p => p.Item2, p => new SerializableMembershipEntry(p.Item1));
        Etag    = tableData.Version.VersionEtag;
        Version = tableData.Version.Version;
    }
    
    public byte[] ToBytes()
    {
        using var stm = new MemoryStream();
        JsonSerializer.Serialize(stm, this);
        return stm.ToArray();
    }

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

    [Obsolete("For serialization/deserialization only", true)]
    public SerializableMembershipEntry()
    {
    }

    public SerializableMembershipEntry(MembershipEntry entry)
    {
        SiloAddress  = entry.SiloAddress.ToParsableString();
        Status       = entry.Status;
        SuspectTimes = entry.SuspectTimes.ToDictionary(p => p.Item1.ToParsableString(), p => p.Item2);
        ProxyPort    = entry.ProxyPort;
        HostName     = entry.HostName;
        SiloName     = entry.SiloName;
        RoleName     = entry.RoleName;
        UpdateZone   = entry.UpdateZone;
        FaultZone    = entry.FaultZone;
        StartTime    = entry.StartTime;
        IAmAliveTime = entry.IAmAliveTime;
    }

    public MembershipEntry ToEntry() =>
        new()
        {
            SiloAddress  = Orleans.Runtime.SiloAddress.FromParsableString(SiloAddress),
            Status       = Status,
            SuspectTimes = SuspectTimes.Select(p => new Tuple<SiloAddress, DateTime>(Orleans.Runtime.SiloAddress.FromParsableString(p.Key), p.Value)).ToList(),
            ProxyPort    = ProxyPort,
            HostName     = HostName,
            SiloName     = SiloName,
            RoleName     = RoleName,
            UpdateZone   = UpdateZone,
            FaultZone    = FaultZone,
            StartTime    = StartTime,
            IAmAliveTime = IAmAliveTime
        };

    public override string ToString() => $"SiloAddress={SiloAddress}, Status={Status}, StartTime={StartTime}, IAmAliveTime={IAmAliveTime}";
}