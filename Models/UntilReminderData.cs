namespace Multi_Bot.Net.Models;

public class UntilReminderData
{
    public ulong MessageId { get; set; }
    
    public ulong ChannelId { get; set; }
    
    public long NotifyTimestamp { get; set; }
    
    public required string MessageText { get; set; }
}