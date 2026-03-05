namespace Multi_Bot.Net.Modules;

public class AvatarCommandModule() : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("avatar", "Gets the given users avatar, default self")]
    public async Task Avatar([SlashCommandParameter(Description = "Selected User")] User? user = null)
    {
        var avatar = user == null ? Context.User.GetAvatarUrl() : user.GetAvatarUrl();
        
        if (avatar == null)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.Message("Invalid user avatar"));
            return;
        }

        var message = new InteractionMessageProperties()
        {
            Content = avatar.ToString(),
            Flags = MessageFlags.Ephemeral
        };
        await Context.Interaction.SendResponseAsync(InteractionCallback.Message(message));
    }
}
