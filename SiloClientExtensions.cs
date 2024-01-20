using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;
using NATS.Client.ObjectStore;
using Orleans.Hosting;
using Orleans.Messaging;
using Orleans.Nats.Implementations;
using Orleans.Nats.Interfaces;

namespace Orleans.Nats;

public static class SiloClientExtensions
{
    public static IClientBuilder UseNatsClient(this IClientBuilder builder, NatsOpts opts) =>
        builder.ConfigureServices(services => services.AddSingleton(opts))
               .ConfigureServices(services => services.AddSingleton<INatsObjContext>(sp => sp.GetRequiredService<INatsClientFactory>().CreateContext()))
               .ConfigureServices(services => services.AddSingleton<INatsClientFactory, NatsClientFactory>());

    public static IClientBuilder UseNatsClustering(this IClientBuilder builder) =>
        builder.ConfigureServices(services =>
                                  {
                                      services.AddNatsMembershipTable();
                                      services.AddSingleton<IGatewayListProvider, NatsGatewayListProvider>();
                                  });
}