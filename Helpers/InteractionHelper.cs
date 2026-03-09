namespace Multi_Bot.Net.Helpers;

public static class InteractionHelper
{
    public static async Task SendResponse(ApplicationCommandInteraction interaction, string? text = null, EmbedProperties? embed = null, bool isEphemeral = false)
    {
        if (text == null && embed == null)
        {
            return;
        }

        var message = new InteractionMessageProperties()
        {
            Flags = isEphemeral ? MessageFlags.Ephemeral : 0,
            Content = text,
            Embeds =  embed != null ? [embed] : null
        };
        await interaction.SendResponseAsync(InteractionCallback.Message(message));
    }

    public static async Task SendDeferredResponse(ApplicationCommandInteraction interaction)
    {
        await interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
    }

    public static async Task SendFollowupResponse(ApplicationCommandInteraction interaction, string text, EmbedProperties? embed = null, bool isEphemeral = false)
    {
        var message = new InteractionMessageProperties()
        {
            Flags = isEphemeral ? MessageFlags.Ephemeral : 0,
            Content = text,
            Embeds =  embed != null ? [embed] : null
        };
        await interaction.SendFollowupMessageAsync(message);
    }
}