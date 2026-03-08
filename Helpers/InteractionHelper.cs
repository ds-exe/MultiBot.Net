namespace Multi_Bot.Net.Helpers;

public class InteractionHelper
{
    public static async Task SendReponse(ApplicationCommandInteraction interaction, string text, bool isEphemeral = false)
    {
        var message = new InteractionMessageProperties()
        {
            Content = text,
            Flags = isEphemeral ? MessageFlags.Ephemeral : 0
        };
        await interaction.SendResponseAsync(InteractionCallback.Message(message));
    }
    
    // await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
    // await Context.Interaction.SendFollowupMessageAsync("Updated Message");
}