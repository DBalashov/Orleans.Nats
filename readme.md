# Usage (Silo)


```csharp
Host.CreateDefaultBuilder(args)
    ...
    .UseOrleans(c =>
                 {
                     ...
                     c.UseNatsClustering()
                      .AddNatsGrainStorageAsDefault()
                      .UseNatsReminders();
                 })

```
or

```csharp
Host.CreateDefaultBuilder(args)
    ...
    .UseOrleans(c =>
                 {
                     ...
                     var natsOptions = new NatsOpts() {Url = "nats://some-nats-host:4222"};
                     c.UseNatsClustering(new NatsClusteringOptions(natsOptions))
                      .AddNatsGrainStorageAsDefault(new NatsGrainStorageOptions(natsOptions))
                      .UseNatsReminders(new NatsRemindersOptions(natsOptions));
                 })

```

# Usage (Client)

```csharp
new HostBuilder()
    ...
    .UseOrleansClient(c =>
                      {
                          ...
                          c.UseNatsClustering();
                      })
    .Build();
```
or
```csharp
new HostBuilder()
    ...
    .UseOrleansClient(c =>
                      {
                          ...
                          var natsOptions = new NatsOpts() {Url = "nats://some-nats-host:4222"};
                          c.UseNatsClustering(new NatsClusteringOptions(natsOptions));
                      })
    .Build();

```