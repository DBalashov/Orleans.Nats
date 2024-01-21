using Microsoft.Extensions.Logging;
using Orleans.Nats.Models;

namespace Orleans.Nats.Implementations;

sealed class NatsContextClusteringWrapper(NatsClusteringOptions options, ILogger<NatsContextClusteringWrapper> logger) : NatsContextWrapper(options, logger);