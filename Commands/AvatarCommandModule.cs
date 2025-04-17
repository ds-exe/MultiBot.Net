namespace MultiBot.Net.Commands;

//public class AvatarCommandModule : ApplicationCommandsModule
//{
//    [SlashCommand("avatar", "Gets the given users avatar, default self")]
//    public async Task Avatar(InteractionContext ctx, [Option("user", "Selected User")] DiscordUser? user = null)
//    {
//        if (user == null)
//        {
//            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
//            {
//                Content = ctx.User.AvatarUrl,
//                IsEphemeral = true,
//            });
//            return;
//        }
//        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
//        {
//            Content = user.AvatarUrl,
//            IsEphemeral = true,
//        });
//    }
//}
