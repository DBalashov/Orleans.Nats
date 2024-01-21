namespace Orleans.Nats.Interfaces;

interface INatsMembershipService
{
    Task Init();

    Task Cleanup();

    Task<MembershipTableData> Read();

    Task<bool> ReadModifyWrite(Func<MembershipTableData, ReadModifyWriteResult> func);
}

public sealed record ReadModifyWriteResult(MembershipTableData NewTable, bool Modified);