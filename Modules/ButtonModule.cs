namespace Multi_Bot.Net.Modules;

public class ButtonModule(DatabaseService databaseService) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("until-reminder")]
    public async Task UntilReminder()
    {
        var state = databaseService.ToggleUntilReminderUser(Context.Message.Id, Context.Interaction.User.Id);
        switch (state)
        {
            case ReminderState.Inserted:
                await InteractionHelper.SendResponse(Context.Interaction, "Notification added", isEphemeral: true);
                break;
            case ReminderState.Deleted:
                await InteractionHelper.SendResponse(Context.Interaction, "Notification removed", isEphemeral: true);
                break;
            case ReminderState.Errored:
                await InteractionHelper.SendResponse(Context.Interaction, "Error creating notification", isEphemeral: true);
                break;
        }
    }
}