using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.ObjectStore;
using Orleans.Configuration;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;

namespace Orleans.Nats.Implementations.Membership;

sealed class NatsMembershipService(NatsContextClusteringWrapper   wrapper,
                                   NatsClusteringOptions          options,
                                   ILogger<NatsMembershipService> logger,
                                   IOptions<ClusterOptions>       clusterOptions) : INatsMembershipService
{
    readonly string bucketId = options.GetBucketName(clusterOptions.Value.ClusterId, clusterOptions.Value.ServiceId);

    public async Task Init() =>
        await wrapper.GetStore(bucketId);

    public async Task Cleanup() =>
        await wrapper.DeleteStore(bucketId);

    public async Task<MembershipTableData> Read()
    {
        try
        {
            logger.LogDebug($"{nameof(Read)} called.");

            var store = await wrapper.GetStore(bucketId);
            var bytes = await store.GetBytesAsync(options.ObjectName);
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

    public async Task<bool> ReadModifyWrite(Func<MembershipTableData, ReadModifyWriteResult> func)
    {
        try
        {
            logger.LogDebug($"{nameof(ReadModifyWrite)} called.");

            var original = await read();
            var result   = func(original);

            if (result.Modified)
                await write(result.NewTable);

            return result.Modified;
        }
        catch (Exception ex)
        {
            logger.LogError($"{nameof(ReadModifyWrite)} failed. Exception={ex.Message}");
            throw;
        }
    }

    #region read / write

    async Task<MembershipTableData> read()
    {
        var store = await wrapper.GetStore(bucketId);
        try
        {
            var bytes     = await store.GetBytesAsync(options.ObjectName);
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
        var store      = await wrapper.GetStore(bucketId);
        var serialized = new SerializableMembershipTable(tableData).ToBytes();
        await store.PutAsync(options.ObjectName, serialized);
    }

    #endregion
}