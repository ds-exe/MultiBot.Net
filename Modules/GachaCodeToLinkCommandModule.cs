namespace Multi_Bot.Net.Modules;

//public class GachaCodeToLinkCommandModule : ApplicationCommandsModule
//{
//    private static readonly string _codeRegex = @"^\w+$";

//    [SlashCommand("code", "Gets the given code as a link")]
//    public async Task CodeToLinkCommand(InteractionContext ctx, [Option("game", "Selected Game")] GachaOptions gachaGame, [Option("code", "Selected Code")] string code)
//    {
//        if (!Regex.Match(code, _codeRegex).Success)
//        {
//            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
//            {
//                Content = "Invalid code entered.",
//                IsEphemeral = true,
//            });
//            return;
//        }

//        string url = string.Empty;
//        switch(gachaGame)
//        {
//            case GachaOptions.ZZZ:
//                url = $"https://zenless.hoyoverse.com/redemption?code={code}";
//                break;
//            case GachaOptions.HSR:
//                url = $"https://hsr.hoyoverse.com/gift?code={code}";
//                break;
//            case GachaOptions.Genshin:
//                url = $"https://genshin.hoyoverse.com/en/gift?code={code}";
//                break;
//        }

//        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
//        {
//            Content = $"[{code}]({url})",
//        });
//        return;
//    }

//    public enum GachaOptions
//    {
//        ZZZ,
//        HSR,
//        Genshin
//    }
//}
