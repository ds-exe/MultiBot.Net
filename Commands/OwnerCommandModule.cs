namespace MultiBot.Net.Commands;

//public class OwnerCommandModule
//{
//    private const ulong creatorID = 74968333413257216;
//    private static readonly ulong owner;

//    static OwnerCommandModule()
//    {
//        var config = ConfigHelper.GetJsonObject<Config>("config");
//        owner = config.Owner == 0 ? creatorID : config.Owner;
//    }

//    public static async Task Restart(DiscordMessage message)
//    {
//        if (message.Author.Id == owner)
//        {
//            try
//            {
//                await message.RespondAsync("Restarting.");
//            }
//            catch { }
//            Environment.Exit(0);
//        }
//    }
//}
