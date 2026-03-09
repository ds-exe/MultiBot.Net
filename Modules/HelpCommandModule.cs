namespace Multi_Bot.Net.Modules;

public class HelpCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private const ulong CreatorId = 74968333413257216;
    
    private readonly RestClient _client;
    private readonly User _user;
    private readonly string _embedThumbnail;

    public HelpCommandModule(RestClient discordClient)
    {
        _client = discordClient;
        var config = ConfigHelper.GetJsonObject<Config>("config");
        var botOwner = config.Owner == 0 ? CreatorId : config.Owner;
        _user = _client.GetUserAsync(botOwner).Result;
        _embedThumbnail = config.EmbedThumbnail ?? "";
    }

    // Initial code based on DisCatSharp default help command
    [SlashCommand("help", "Displays command help")]
    //public async Task Avatar([SlashCommandParameter(Description = "Selected User")] User? user = null)
    public async Task Help([SlashCommandParameter(Name = "option_one", 
                                                  Description = "top level command to provide help for", 
                                                  AutocompleteProviderType = typeof(DefaultHelpAutoCompleteProvider))] string commandName)
    {
        var applicationCommands = await GetApplicationCommands(_client, Context.Guild?.Id, commandName);
        
        if (applicationCommands.Count < 1)
        {
            await InteractionHelper.SendResponse(Context.Interaction, "Slash command not found", isEphemeral: true);
            return;
        }
        
        var command = applicationCommands[0];
        var embed = new EmbedProperties()
        {
            Title = "Help",
            Description =
                $"/{command.Name}: {command.Description}" // replace '/{command.Name}' with equivalent of 'command.Mention'
        };
        embed.Fields = [
            new EmbedFieldProperties()
            {
                Name = "Command is NSFW",
                Value = command.Nsfw.ToString()
            }
        ];
        embed.Color = new Color(39423);
        embed.Thumbnail = _embedThumbnail;
        embed.Footer = new EmbedFooterProperties()
        {
            Text = $"BOT owner @{_user.Username}",
            IconUrl = _user.GetAvatarUrl()?.ToString()
        };
        
        // if (command.Options is not null)
        // {
        //     var commandOptions = command.Options.ToList();
        //     var sb = new StringBuilder();
        //
        //     foreach (var option in commandOptions)
        //         sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');
        //
        //     sb.Append('\n');
        //     discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
        // }
        //
        // await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        //         new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
        
        await InteractionHelper.SendResponse(Context.Interaction, embed: embed, isEphemeral: true);
    }

    private static async Task<List<ApplicationCommand>> GetApplicationCommands(RestClient client, ulong? guildId, string commandName)
    {
        var applicationId = ((IEntityToken)client.Token!).Id;
        var commands = await client.GetGlobalApplicationCommandsAsync(applicationId);
        IReadOnlyList<GuildApplicationCommand> guildCommands = [];

        if (guildId != null)
        {
            guildCommands = await client.GetGuildApplicationCommandsAsync(applicationId, guildId.Value);
        }

        List<ApplicationCommand> matchedCommands = [];
        if (commands.Count > 0)
        {
            var foundCommands = commands.Where(rec => rec.Name.Contains(commandName, StringComparison.OrdinalIgnoreCase)).ToList();
            matchedCommands.AddRange(foundCommands);
        }

        if (guildCommands.Count > 0)
        {
            var foundCommands = guildCommands.Where(rec => rec.Name.Contains(commandName, StringComparison.OrdinalIgnoreCase)).ToList();
            matchedCommands.AddRange(foundCommands);
        }
        return matchedCommands.DistinctBy(rec => rec.Name).ToList();
    }

    private class DefaultHelpAutoCompleteProvider(RestClient client) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
            ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            var userInput = option.Value ?? string.Empty;
            var guildId = context.Guild?.Id;
            var commands = await GetApplicationCommands(client, guildId, userInput);
            return commands.Select(rec => new ApplicationCommandOptionChoiceProperties(rec.Name, rec.Name)); 
        }
    }
}
