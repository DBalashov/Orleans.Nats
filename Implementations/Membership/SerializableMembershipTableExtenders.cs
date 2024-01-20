using System.Text.Json;
using Orleans.Runtime;

namespace Orleans.Nats.Implementations.Membership;

static class SerializableMembershipTableExtenders
{
    public static byte[] ToBytes(this MembershipTableData tableData)
    {
        var t = new SerializableMembershipTable()
                {
                    Members = tableData.Members.ToDictionary(p => p.Item2, p => p.Item1.ToSerializable()),
                    Etag    = tableData.Version.VersionEtag,
                    Version = tableData.Version.Version
                };

        using var stm = new MemoryStream();
        JsonSerializer.Serialize(stm, t);
        return stm.ToArray();
    }

    public static MembershipTableData ToMembershipTableData(this byte[] bytes)
    {
        var r = JsonSerializer.Deserialize<SerializableMembershipTable>(bytes)!;
        return new MembershipTableData(r.Members.Select(p => new Tuple<MembershipEntry, string>(p.Value.ToEntry(), p.Key)).ToList(),
                                       new TableVersion(r.Version, r.Etag))
           .WithoutDuplicateDeads();
    }

    static SerializableMembershipEntry ToSerializable(this MembershipEntry me) =>
        new()
        {
            SiloAddress  = me.SiloAddress.ToParsableString(),
            Status       = me.Status,
            SuspectTimes = me.SuspectTimes.ToDictionary(p => p.Item1.ToParsableString(), p => p.Item2),
            ProxyPort    = me.ProxyPort,
            HostName     = me.HostName,
            SiloName     = me.SiloName,
            RoleName     = me.RoleName,
            UpdateZone   = me.UpdateZone,
            FaultZone    = me.FaultZone,
            StartTime    = me.StartTime,
            IAmAliveTime = me.IAmAliveTime
        };

    static MembershipEntry ToEntry(this SerializableMembershipEntry me) =>
        new()
        {
            SiloAddress  = SiloAddress.FromParsableString(me.SiloAddress),
            Status       = me.Status,
            SuspectTimes = me.SuspectTimes.Select(p => new Tuple<SiloAddress, DateTime>(SiloAddress.FromParsableString(p.Key), p.Value)).ToList(),
            ProxyPort    = me.ProxyPort,
            HostName     = me.HostName,
            SiloName     = me.SiloName,
            RoleName     = me.RoleName,
            UpdateZone   = me.UpdateZone,
            FaultZone    = me.FaultZone,
            StartTime    = me.StartTime,
            IAmAliveTime = me.IAmAliveTime
        };
}