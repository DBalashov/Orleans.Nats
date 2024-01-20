using Orleans.Nats.Interfaces;
using Orleans.Runtime;

namespace Orleans.Nats.Implementations;

sealed class NatsMembershipTable(INatsMembershipService membershipService) : IMembershipTable
{
    /// <inheritdoc />
    public async Task InitializeMembershipTable(bool tryInitTableVersion) =>
        await membershipService.Init();

    /// <inheritdoc />
    public async Task DeleteMembershipTableEntries(string deploymentId) =>
        await membershipService.Cleanup();

    /// <inheritdoc />
    public async Task<MembershipTableData> ReadRow(SiloAddress key) => // ?
        await membershipService.Read();

    /// <inheritdoc />
    public async Task<MembershipTableData> ReadAll() =>
        await membershipService.Read();

    /// <inheritdoc />
    public async Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion) =>
        await membershipService.ReadModifyWrite(orig =>
                                                       {
                                                           var memberList = orig.Members.ToList();
                                                           memberList.Add(new Tuple<MembershipEntry, string>(entry, Extenders.CreateEtag()));
                                                           return (new MembershipTableData(memberList, tableVersion), true);
                                                       });

    /// <inheritdoc />
    public async Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion) =>
        await membershipService.ReadModifyWrite(orig =>
                                                       {
                                                           var memberList = orig.Members.ToList();
                                                           var idx        = memberList.FindIndex(p => p.Item1.SiloAddress.CompareTo(entry.SiloAddress) == 0);
                                                           if (idx >= 0)
                                                           {
                                                               // ? if (memberList[idx].Item2 != etag) return false;
                                                               memberList[idx] = new Tuple<MembershipEntry, string>(entry, etag);
                                                           }
                                                           else
                                                           {
                                                               memberList.Add(new Tuple<MembershipEntry, string>(entry, etag));
                                                           }

                                                           return (new MembershipTableData(memberList, tableVersion), true);
                                                       });

    /// <inheritdoc />
    public async Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
    {
        var beforeUtc = beforeDate.UtcDateTime;
        await membershipService.ReadModifyWrite(orig =>
                                                       {
                                                           var newMembers = orig.Members.Where(p => p.Item1.Status != SiloStatus.Active && p.Item1.IAmAliveTime < beforeUtc).ToList();
                                                           var newVersion = new TableVersion(orig.Version.Version + 1, orig.Version.VersionEtag);
                                                           return (new MembershipTableData(newMembers, newVersion),
                                                                   orig.Members.Count != newMembers.Count);
                                                       });
    }

    /// <inheritdoc />
    public async Task UpdateIAmAlive(MembershipEntry entry) =>
        await membershipService.ReadModifyWrite(orig =>
                                                       {
                                                           var foundedEntry = orig.Members.FirstOrDefault(p => p.Item1.SiloAddress.CompareTo(entry.SiloAddress) == 0);
                                                           if (foundedEntry != null)
                                                           {
                                                               foundedEntry.Item1.IAmAliveTime = entry.IAmAliveTime;
                                                               var newVersion = new TableVersion(orig.Version.Version + 1, orig.Version.VersionEtag);
                                                               return (new MembershipTableData(orig.Members.ToList(), newVersion), true);
                                                           }

                                                           return (orig, false);
                                                       });
}