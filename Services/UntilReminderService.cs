using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Multi_Bot.Net.Services;

public class UntilReminderService(
    DatabaseService databaseService,
    GatewayClient gatewayClient,
    RestClient restClient,
    ILogger<UntilReminderService> logger) : BackgroundService
{
    public const string ReminderEmoji = "\u23F0";
    private readonly ulong _currentUserId = ((IEntityToken)restClient.Token!).Id;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        gatewayClient.MessageReactionAdd += args => new ValueTask(HandleReactionAdded(args, stoppingToken));
        gatewayClient.MessageReactionRemove += args => new ValueTask(HandleReactionRemoved(args, stoppingToken));

        _ = RunPendingReminders(stoppingToken);
        return Task.CompletedTask;
    }

    public async Task TrackReminderAsync(ulong channelId, ulong messageId, DateTimeOffset dueAt, string? message, CancellationToken cancellationToken = default)
    {
        var reminder = databaseService.InsertUntilReminder(new UntilReminderData
        {
            ChannelId = channelId,
            MessageId = messageId,
            DueAtUnix = dueAt.ToUnixTimeSeconds(),
            MessageText = string.IsNullOrWhiteSpace(message) ? null : message.Trim()
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
            var delay = DateTimeOffset.FromUnixTimeSeconds(reminder.DueAtUnix) - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            var subscriberIds = databaseService.GetUntilReminderSubscriberIds(reminder.Id);
            var mentions = string.Join(' ', subscriberIds.Select(id => $"<@{id}>"));
            var content = string.Join(' ', new[] { mentions, reminder.MessageText }.Where(value => !string.IsNullOrWhiteSpace(value)));

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "Time's up.";
            }

            await restClient.SendMessageAsync(reminder.ChannelId, new MessageProperties()
            {
                Content = content,
                MessageReference = MessageReferenceProperties.Reply(reminder.MessageId, false),
                AllowedMentions = new AllowedMentionsProperties()
                {
                    ReplyMention = false
                }
            }, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send /until reminder for message {MessageId}", reminder.MessageId);
        }
        finally
        {
            databaseService.DeleteUntilReminder(reminder.Id);
        }
    }

    private Task HandleReactionAdded(MessageReactionAddEventArgs args, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || args.UserId == _currentUserId || !IsReminderReaction(args.Emoji.Name))
        {
            return Task.CompletedTask;
        }

        var reminderId = databaseService.GetUntilReminderIdByMessageId(args.MessageId);
        if (reminderId != null)
        {
            databaseService.InsertUntilReminderSubscriber(reminderId.Value, args.UserId);
        }

        return Task.CompletedTask;
    }

    private Task HandleReactionRemoved(MessageReactionRemoveEventArgs args, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || args.UserId == _currentUserId || !IsReminderReaction(args.Emoji.Name))
        {
            return Task.CompletedTask;
        }

        var reminderId = databaseService.GetUntilReminderIdByMessageId(args.MessageId);
        if (reminderId != null)
        {
            databaseService.DeleteUntilReminderSubscriber(reminderId.Value, args.UserId);
        }

        return Task.CompletedTask;
    }

    private static bool IsReminderReaction(string? emojiName)
    {
        return emojiName == ReminderEmoji;
    }
}
