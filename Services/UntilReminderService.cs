using Microsoft.Extensions.Hosting;

namespace Multi_Bot.Net.Services;

public class UntilReminderService(
    DatabaseService databaseService,
    RestClient restClient) : BackgroundService
{
    public const string ReminderEmoji = "\u23F0";
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = RunPendingReminders(stoppingToken);
        return Task.CompletedTask;
    }
    
    public async Task TrackReminderAsync(ulong channelId, ulong messageId, DateTimeOffset notifyTimestamp, string message, CancellationToken cancellationToken = default)
    {
        var reminder = databaseService.InsertUntilReminder(new UntilReminderData
        {
            MessageId = messageId,
            ChannelId = channelId,
            NotifyTimestamp = notifyTimestamp.ToUnixTimeSeconds(),
            MessageText = message.Trim()
        });

        _ = RunReminderAsync(reminder, cancellationToken);
        await Task.CompletedTask;
    }

    private async Task RunPendingReminders(CancellationToken cancellationToken)
    {
        foreach (var reminder in databaseService.GetPendingUntilReminders())
        {
            _ = RunReminderAsync(reminder, cancellationToken);
        }

        await Task.CompletedTask;
    }

    private async Task RunReminderAsync(UntilReminderData reminder, CancellationToken cancellationToken)
    {
        try
        {
            var delay = DateTimeOffset.FromUnixTimeSeconds(reminder.NotifyTimestamp) - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            var reminderUsers = databaseService.GetUntilReminderUsers(reminder.MessageId);
            if (reminderUsers.Count > 0)
            {
                var content = string.Join(" ", reminderUsers.Select(rec => $"<@{rec.UserId}>"));
                await restClient.SendMessageAsync(reminder.ChannelId, new MessageProperties()
                {
                    Content = content,
                    MessageReference = MessageReferenceProperties.Reply(reminder.MessageId, false),
                    AllowedMentions = new AllowedMentionsProperties()
                    {
                        ReplyMention = false
                    },
                }, cancellationToken: cancellationToken);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            databaseService.DeleteUntilReminder(reminder.MessageId);
        }
    }
}
