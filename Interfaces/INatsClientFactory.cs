using Orleans.Nats.Models;

namespace Orleans.Nats.Interfaces;

interface INatsClientFactory
{
    NatsContextWrapper CreateContext();
}