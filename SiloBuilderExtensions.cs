using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NATS.Client.Core;
using NATS.Client.ObjectStore;
using Orleans.Hosting;
using Orleans.Nats.Implementations;
using Orleans.Nats.Interfaces;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Nats;

public static class SiloBuilderExtensions
{
    public static ISiloBuilder UseNatsClient(this ISiloBuilder builder, NatsOpts opts) =>
        builder.ConfigureServices(services => services.AddSingleton(opts))
               .ConfigureServices(services => services.AddSingleton<INatsClientFactory, NatsClientFactory>())
               .ConfigureServices(services => services.AddSingleton<INatsObjContext>(sp => sp.GetRequiredService<INatsClientFactory>().CreateContext()));

    public static ISiloBuilder UseNatsClustering(this ISiloBuilder builder) =>
        builder.ConfigureServices(services => services.AddNatsMembershipTable());

    public static IServiceCollection AddNatsMembershipTable(this IServiceCollection services) =>
        services.AddSingleton<INatsMembershipService, NatsMembershipService>()
                .AddSingleton<IMembershipTable, NatsMembershipTable>();


    #region Storage

    public static ISiloBuilder AddNatsGrainStorageAsDefault(this ISiloBuilder builder) =>
        builder.ConfigureServices(services => services.AddNatsGrainStorage<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));

    public static ISiloBuilder AddNatsGrainStorage(this ISiloBuilder builder, string name) =>
        builder.ConfigureServices(services => services.AddNatsGrainStorage<IGrainStorage>(name));

    public static IServiceCollection AddNatsGrainStorage<T>(this IServiceCollection services, string name) where T : IGrainStorage
    {
        services.AddKeyedSingleton<IGrainStorage, NatsGrainStorage>(name);

        // Check if it is the default implementation
        if (string.Equals(name, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.Ordinal))
        {
            services.TryAddSingleton(sp => sp.GetKeyedService<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
        }

        // Check if the grain storage implements ILifecycleParticipant<ISiloLifecycle>
        if (typeof(ILifecycleParticipant<ISiloLifecycle>).IsAssignableFrom(typeof(T)))
        {
            services.AddSingleton(s => (ILifecycleParticipant<ISiloLifecycle>) s.GetRequiredKeyedService<IGrainStorage>(name));
        }

        return services;
    }

    #endregion
}