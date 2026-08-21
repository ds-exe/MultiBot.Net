using Microsoft.Extensions.Hosting;

namespace Multi_Bot.Net.Services;

public class UntilReminderService(
    DatabaseService databaseService,
    GatewayClient gatewayClient,
    RestClient restClient) : BackgroundService
{
    public const string ReminderEmoji = "\u23F0";
    private readonly ulong _botId = gatewayClient.Token.Id;
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: swap to button interaction
        gatewayClient.MessageReactionAdd += args => new ValueTask(HandleReactionAdded(args, stoppingToken));
        gatewayClient.MessageReactionRemove += args => new ValueTask(HandleReactionRemoved(args, stoppingToken));

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
            foreach (var reminderUser in reminderUsers)
            {
                // TODO: use applicationId and token to send interaction response
                //restClient.SendInteractionFollowupMessageAsync()
                await restClient.SendMessageAsync(reminder.ChannelId, new MessageProperties()
                {
                    Content = $"<@{reminderUser.UserId}> {reminder.MessageText}",
                    MessageReference = MessageReferenceProperties.Reply(reminder.MessageId, false),
                    AllowedMentions = new AllowedMentionsProperties()
                    {
                        ReplyMention = false
                    },
                    Flags = MessageFlags.Ephemeral
                }, cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            // ignored
        }
        finally
        {
            databaseService.DeleteUntilReminder(reminder.MessageId);
        }
    }
    
    private Task HandleReactionAdded(MessageReactionAddEventArgs args, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || args.UserId == _botId || args.Emoji.Name != ReminderEmoji)
        {
            return Task.CompletedTask;
        }

        //databaseService.InsertUntilReminderUser(args.MessageId, args.UserId);

        return Task.CompletedTask;
    }

    private Task HandleReactionRemoved(MessageReactionRemoveEventArgs args, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || args.UserId == _botId || args.Emoji.Name != ReminderEmoji)
        {
            return Task.CompletedTask;
        }

        databaseService.DeleteUntilReminderUser(args.MessageId, args.UserId);

        return Task.CompletedTask;
    }
}