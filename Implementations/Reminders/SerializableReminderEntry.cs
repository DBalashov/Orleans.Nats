using System.Text.Json;
using Orleans.Runtime;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Orleans.Nats.Implementations.Reminders;

static class SerializableReminderExtenders
{
    public static ReminderEntry ToEntry(this byte[] bytes)
    {
        var r = JsonSerializer.Deserialize<SerializableReminderEntry>(bytes)!;
        return r.ToReminderEntry();
    }

    public static byte[] ToBytes(this ReminderEntry entry)
    {
        using var stm = new MemoryStream();
        JsonSerializer.Serialize(stm, new SerializableReminderEntry(entry));
        return stm.ToArray();
    }
}

public class SerializableReminderEntry
{
    public string   GrainId      { get; set; }
    public string   ReminderName { get; set; }
    public DateTime StartAt      { get; set; }
    public TimeSpan Period       { get; set; }
    public string   ETag         { get; set; }

    [Obsolete("For serialization/deserialization only", true)]
    public SerializableReminderEntry()
    {
    }

    public SerializableReminderEntry(ReminderEntry entry)
    {
        GrainId      = entry.GrainId.ToString();
        ReminderName = entry.ReminderName;
        StartAt      = entry.StartAt;
        Period       = entry.Period;
        ETag         = entry.ETag;
    }

    public ReminderEntry ToReminderEntry() =>
        new()
        {
            GrainId      = Orleans.Runtime.GrainId.Parse(GrainId),
            ReminderName = ReminderName,
            StartAt      = StartAt,
            Period       = Period,
            ETag         = ETag
        };
}