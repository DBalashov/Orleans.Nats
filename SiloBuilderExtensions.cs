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
    public static ISiloBuilder UseNatsClient(this ISiloBuilder builder, NatsOrleansOptions opts) =>
        builder.ConfigureServices(services => services.AddSingleton(opts))
               .ConfigureServices(services => services.AddSingleton<INatsClientFactory, NatsClientFactory>())
               .ConfigureServices(services => services.AddSingleton<NatsContextWrapper>(sp => sp.GetRequiredService<INatsClientFactory>().CreateContext()));

    #region Clustering

    public static ISiloBuilder UseNatsClustering(this ISiloBuilder builder) =>
        builder.ConfigureServices(services => services.AddNatsMembershipTable());

    public static IServiceCollection AddNatsMembershipTable(this IServiceCollection services) =>
        services.AddSingleton<INatsMembershipService, NatsMembershipService>()
                .AddSingleton<IMembershipTable, NatsMembershipTable>();

    #endregion

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

    #region Reminders

    public static ISiloBuilder UseNatsReminders(this ISiloBuilder builder) =>
        builder.ConfigureServices(services =>
                                  {
                                      services.AddReminders();
                                      services.AddSingleton<INatsReminderService, NatsReminderService>();
                                      services.AddSingleton<IReminderTable, NatsRemindersTable>();
                                  });

    #endregion
}