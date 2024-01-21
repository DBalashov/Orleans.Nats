using Microsoft.Extensions.Logging;
using Orleans.Nats.Models;

namespace Orleans.Nats.Implementations;

sealed class NatsContextRemindersWrapper(NatsRemindersOptions options, ILogger<NatsContextRemindersWrapper> logger) : NatsContextWrapper(options, logger);