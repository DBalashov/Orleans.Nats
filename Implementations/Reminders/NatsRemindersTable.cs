using Orleans.Nats.Interfaces;
using Orleans.Runtime;

namespace Orleans.Nats.Implementations.Reminders;

sealed class NatsRemindersTable(INatsReminderService reminderService) : IReminderTable
{
    public async Task Init() =>
        await reminderService.Init();

    public async Task<ReminderTableData> ReadRows(GrainId grainId)
    {
        var grainIdStr = grainId.ToString();
        var items      = await reminderService.Find(p => p.GrainId == grainIdStr);
        return new ReminderTableData(items);
    }

    public async Task<ReminderTableData> ReadRows(uint begin, uint end)
    {
        FindReminderPredicate filter = begin < end
                                           ? p => p.GrainHash > begin && p.GrainHash <= end
                                           : p => p.GrainHash > begin || p.GrainHash <= end;

        var items = await reminderService.Find(filter);
        return new ReminderTableData(items);
    }

    public async Task<ReminderEntry> ReadRow(GrainId grainId, string reminderName) =>
        await reminderService.Get(grainId, reminderName);

    public async Task<string> UpsertRow(ReminderEntry entry) =>
        await reminderService.Put(entry);

    public async Task<bool> RemoveRow(GrainId grainId, string reminderName, string eTag) =>
        await reminderService.Remove(new ReminderEntry() {GrainId = grainId, ReminderName = reminderName, ETag = eTag});

    public async Task TestOnlyClearTable()
    {
    }
}