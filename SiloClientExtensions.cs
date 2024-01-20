using Microsoft.Extensions.DependencyInjection;
using Orleans.Messaging;
using Orleans.Nats.Implementations;
using Orleans.Nats.Implementations.Membership;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;

namespace Orleans.Nats;

public static class SiloClientExtensions
{
    public static IClientBuilder UseNatsClient(this IClientBuilder builder, NatsOrleansOptions opts) =>
        builder.ConfigureServices(services => services.AddSingleton(opts))
               .ConfigureServices(services => services.AddSingleton<NatsContextWrapper>(sp => sp.GetRequiredService<INatsClientFactory>().CreateContext()))
               .ConfigureServices(services => services.AddSingleton<INatsClientFactory, NatsClientFactory>());

    public static IClientBuilder UseNatsClustering(this IClientBuilder builder) =>
        builder.ConfigureServices(services =>
                                  {
                                      services.AddNatsMembershipTable();
                                      services.AddSingleton<IGatewayListProvider, NatsGatewayListProvider>();
                                  });
}