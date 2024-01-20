using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.ObjectStore;
using Orleans.Nats.Helpers;
using Orleans.Configuration;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats.Implementations;

sealed class NatsMembershipService(INatsObjContext              context,
                                   ILogger<NatsMembershipTable> logger,
                                   IOptions<ClusterOptions>     clusterOptions) : INatsMembershipService
{
    const string objectName = "MembershipTableData";

    readonly string bucketId = "Orleans-" + clusterOptions.Value.ClusterId + '-' + clusterOptions.Value.ServiceId + "-Membership";

    public async Task Init() =>
        await context.CreateObjectStoreAsync(bucketId);

    public async Task Cleanup() =>
        await context.DeleteObjectStore(bucketId, CancellationToken.None);

    public async Task<MembershipTableData> Read()
    {
        try
        {
            logger.LogDebug($"{nameof(Read)} called.");

            var store = await context.GetObjectStoreAsync(bucketId);
            var bytes = await store.GetBytesAsync(objectName);
            return bytes.ToMembershipTableData();
        }
        catch (NatsObjNotFoundException)
        {
            return new MembershipTableData(new TableVersion(1, Extenders.CreateEtag()));
        }
        catch (Exception ex)
        {
            logger.LogError($"{nameof(Read)} failed. Exception={ex.Message}");
            throw;
        }
    }

    public async Task<bool> ReadModifyWrite(Func<MembershipTableData, (MembershipTableData newTable, bool modified)> func)
    {
        try
        {
            logger.LogDebug($"{nameof(ReadModifyWrite)} called.");

            var original = await read();
            var result   = func(original);

            if (result.modified)
                await write(result.newTable);

            return result.modified;
        }
        catch (Exception ex)
        {
            logger.LogError($"{nameof(ReadModifyWrite)} failed. Exception={ex.Message}");
            throw;
        }
    }

    #region read

    async Task<MembershipTableData> read()
    {
        var store = await context.GetObjectStoreAsync(bucketId);
        try
        {
            var bytes     = await store.GetBytesAsync(objectName);
            var tableData = bytes.ToMembershipTableData();
            return tableData;
        }
        catch (NatsObjNotFoundException)
        {
            return new MembershipTableData(new TableVersion(1, Guid.NewGuid().ToString()));
        }
    }

    async Task write(MembershipTableData tableData)
    {
        var store      = await context.GetObjectStoreAsync(bucketId);
        var serialized = tableData.ToBytes();
        await store.PutAsync(objectName, serialized);
    }

    #endregion
}