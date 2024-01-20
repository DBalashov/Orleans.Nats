using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.ObjectStore;
using Orleans.Configuration;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;

namespace Orleans.Nats.Implementations.Membership;

sealed class NatsMembershipService(NatsContextWrapper             wrapper,
                                   NatsOrleansOptions             options,
                                   ILogger<NatsMembershipService> logger,
                                   IOptions<ClusterOptions>       clusterOptions) : INatsMembershipService
{
    readonly string bucketId = options.MembershipBucketName(clusterOptions.Value.ClusterId, clusterOptions.Value.ServiceId);

    public async Task Init() =>
        await wrapper.Context.CreateObjectStoreAsync(bucketId);

    public async Task Cleanup() =>
        await wrapper.Context.DeleteObjectStore(bucketId, CancellationToken.None);

    public async Task<MembershipTableData> Read()
    {
        try
        {
            logger.LogDebug($"{nameof(Read)} called.");

            var store = await wrapper.Context.GetObjectStoreAsync(bucketId);
            var bytes = await store.GetBytesAsync(options.MembershipObjectName);
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
        var store = await wrapper.Context.GetObjectStoreAsync(bucketId);
        try
        {
            var bytes     = await store.GetBytesAsync(options.MembershipObjectName);
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
        var store      = await wrapper.Context.GetObjectStoreAsync(bucketId);
        var serialized = tableData.ToBytes();
        await store.PutAsync(options.MembershipObjectName, serialized);
    }

    #endregion
}