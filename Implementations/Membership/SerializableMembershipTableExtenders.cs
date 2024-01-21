using System.Text.Json;

namespace Orleans.Nats.Implementations.Membership;

static class SerializableMembershipTableExtenders
{
    public static MembershipTableData ToMembershipTableData(this byte[] bytes)
    {
        var r = JsonSerializer.Deserialize<SerializableMembershipTable>(bytes)!;
        return new MembershipTableData(r.Members.Select(p => new Tuple<MembershipEntry, string>(p.Value.ToEntry(), p.Key)).ToList(),
                                       new TableVersion(r.Version, r.Etag))
           .WithoutDuplicateDeads();
    }
}