namespace Multi_Bot.Net.Models;

public class UntilReminderData
{
    public long Id { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
    public long DueAtUnix { get; set; }
    public string? MessageText { get; set; }
}
