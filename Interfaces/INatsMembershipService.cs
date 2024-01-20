namespace Orleans.Nats.Interfaces;

interface INatsMembershipService
{
    Task Init();

    Task Cleanup();

    Task<MembershipTableData> Read();

    Task<bool> ReadModifyWrite(Func<MembershipTableData, (MembershipTableData newTable, bool modified)> func);
}