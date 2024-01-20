using Orleans.Nats.Implementations.Reminders;
using Orleans.Runtime;

namespace Orleans.Nats.Interfaces;

public interface INatsReminderService
{
    Task Init();

    Task<ReminderEntry[]> Find(FindReminderPredicate filter);

    Task<ReminderEntry> Get(GrainId grainId, string reminderName);

    Task<string> Put(ReminderEntry entry);

    Task<bool> Remove(ReminderEntry entry);
}

public delegate bool FindReminderPredicate(NatsReminderMetadata parms);