

namespace Multi_Bot.Net.Modules;

public class GachaCodeToLinkCommandModule() : ApplicationCommandModule<ApplicationCommandContext>
{
    private const string CodeRegex = @"^\w+$";

    [SlashCommand("code", "Gets the given code as a link")]
    public async Task CodeToLinkCommand([SlashCommandParameter(Name = "game", Description = "Selected Game")] GachaOptions gachaGame, 
                                        [SlashCommandParameter(Name = "code", Description = "Selected Code")] string code)
    {
        if (!Regex.IsMatch(code, CodeRegex))
        {
            await InteractionHelper.SendReponse(Context.Interaction, "Invalide code entered.", true);
            return;
        }

        string url = gachaGame switch
        {
            GachaOptions.Zzz => $"https://zenless.hoyoverse.com/redemption?code={code}",
            GachaOptions.Hsr => $"https://hsr.hoyoverse.com/gift?code={code}",
            GachaOptions.Genshin => $"https://genshin.hoyoverse.com/en/gift?code={code}",
            _ => string.Empty
        };

        await InteractionHelper.SendReponse(Context.Interaction, $"[{code}]({url})");
    }

    public enum GachaOptions
    {
        [SlashCommandChoice(Name = "ZZZ")]
        Zzz,
        [SlashCommandChoice(Name = "HSR")]
        Hsr,
        Genshin
    }
}
