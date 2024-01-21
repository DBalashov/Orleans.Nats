using Microsoft.Extensions.Logging;
using Orleans.Nats.Models;

namespace Orleans.Nats.Implementations;

sealed class NatsContextGrainStorageWrapper(NatsGrainStorageOptions options, ILogger<NatsContextGrainStorageWrapper> logger) : NatsContextWrapper(options, logger);