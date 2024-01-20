namespace Orleans.Nats.Implementations.Reminders;

public sealed record NatsReminderMetadata(string GrainId, string ReminderName, uint GrainHash)
{
    public static bool TryParse(Dictionary<string, string> metadata, out NatsReminderMetadata parsed)
    {
        parsed = default!;

        if (!metadata.TryGetValue("X-GrainId", out var grainId)) return false;
        if (!metadata.TryGetValue("X-Name",    out var reminderName)) return false;
        if (!metadata.TryGetValue("X-Hash",    out var grainHashStr)) return false;
        if (!uint.TryParse(grainHashStr, out var grainHash)) return false;

        parsed = new NatsReminderMetadata(grainId, reminderName, grainHash);
        return true;
    }

    public Dictionary<string, string> ToMetadata() =>
        new()
        {
            ["X-GrainId"] = GrainId,
            ["X-Name"]    = ReminderName,
            ["X-Hash"]    = GrainHash.ToString()
        };
}