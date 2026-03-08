namespace Multi_Bot.Net.Modules;

public class AvatarCommandModule() : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("avatar", "Gets the given users avatar, default self")]
    public async Task Avatar([SlashCommandParameter(Description = "Selected User")] User? user = null)
    {
        var avatar = user == null ? Context.User.GetAvatarUrl() : user.GetAvatarUrl();

        if (avatar == null)
        {
            await InteractionHelper.SendReponse(Context.Interaction, "Invalid user avatar");
            return;
        }

        await InteractionHelper.SendReponse(Context.Interaction, avatar.ToString(), true);
    }
}
