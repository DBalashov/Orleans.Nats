using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.Nats.Implementations;
using Orleans.Nats.Implementations.GrainStorage;
using Orleans.Nats.Implementations.Membership;
using Orleans.Nats.Implementations.Reminders;
using Orleans.Nats.Interfaces;
using Orleans.Nats.Models;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.

namespace Orleans.Nats;

public static class SiloBuilderExtensions
{
    #region Clustering

    public static ISiloBuilder UseNatsClustering(this ISiloBuilder builder, NatsClusteringOptions? options = null) =>
        builder
           .ConfigureServices(services =>
                              {
                                  services.AddSingleton<NatsClusteringOptions>(options ?? NatsClusteringOptions.Default);
                                  services.AddSingleton<NatsContextClusteringWrapper>();
                                  services.AddNatsMembershipTable();
                              });

    public static IServiceCollection AddNatsMembershipTable(this IServiceCollection services) =>
        services.AddSingleton<INatsMembershipService, NatsMembershipService>()
                .AddSingleton<IMembershipTable, NatsMembershipTable>();

    #endregion

    #region Storage

    public static ISiloBuilder AddNatsGrainStorageAsDefault(this ISiloBuilder builder, NatsGrainStorageOptions? options = null) =>
        builder.ConfigureServices(services => services.AddNatsGrainStorage<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, options));

    public static ISiloBuilder AddNatsGrainStorage(this ISiloBuilder builder, string name, NatsGrainStorageOptions? options) =>
        builder.ConfigureServices(services => services.AddNatsGrainStorage<IGrainStorage>(name, options));

    public static IServiceCollection AddNatsGrainStorage<T>(this IServiceCollection services, string name, NatsGrainStorageOptions? options) where T : IGrainStorage
    {
        services.AddSingleton<NatsGrainStorageOptions>(options ?? NatsGrainStorageOptions.Default);
        services.AddSingleton<NatsContextGrainStorageWrapper>();

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

    #region Reminders

    public static ISiloBuilder UseNatsReminders(this ISiloBuilder builder, NatsRemindersOptions? options = null) =>
        builder.ConfigureServices(services =>
                                  {
                                      services.AddReminders();
                                      services.AddSingleton<NatsRemindersOptions>(options ?? NatsRemindersOptions.Default);
                                      services.AddSingleton<NatsContextRemindersWrapper>();
                                      services.AddSingleton<INatsReminderService, NatsReminderService>();
                                      services.AddSingleton<IReminderTable, NatsRemindersTable>();
                                  });

    #endregion
}