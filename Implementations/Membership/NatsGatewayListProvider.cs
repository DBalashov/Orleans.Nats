using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Nats.Interfaces;
using Orleans.Runtime;

namespace Orleans.Nats.Implementations.Membership;

sealed class NatsGatewayListProvider(INatsMembershipService   membershipService,
                                     IOptions<GatewayOptions> gatewayOptions) : IGatewayListProvider
{
    public TimeSpan MaxStaleness => gatewayOptions.Value.GatewayListRefreshPeriod;
    public bool     IsUpdatable  => true;

    public async Task InitializeGatewayListProvider() =>
        await membershipService.Init();

    public async Task<IList<Uri>> GetGateways()
    {
        var r = await membershipService.Read();
        return r.Members
                .Where(x => x.Item1.Status == SiloStatus.Active && x.Item1.ProxyPort > 0)
                .Select(x => x.Item1.ToGatewayUri()).ToList();
    }
}