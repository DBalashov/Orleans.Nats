using Microsoft.Extensions.DependencyInjection;
using Orleans.Messaging;
using Orleans.Nats.Implementations;
using Orleans.Nats.Implementations.Membership;
using Orleans.Nats.Models;

namespace Orleans.Nats;

public static class SiloClientExtensions
{
    public static IClientBuilder UseNatsClustering(this IClientBuilder builder, NatsClusteringOptions? options = null) =>
        builder.ConfigureServices(services =>
                                  {
                                      services.AddSingleton<NatsClusteringOptions>(options ?? NatsClusteringOptions.Default);
                                      services.AddSingleton<NatsContextClusteringWrapper>();
                                      services.AddNatsMembershipTable();
                                      services.AddSingleton<IGatewayListProvider, NatsGatewayListProvider>();
                                  });
}