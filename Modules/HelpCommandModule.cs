using System.Text;

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
        var owner = Environment.GetEnvironmentVariable(nameof(EnvVar.OWNER));
        var ownerExists = ulong.TryParse(owner, out var ownerId);
        var botOwner = ownerExists ? ownerId : CreatorId;
        _user = _client.GetUserAsync(botOwner).Result;
        _embedThumbnail = Environment.GetEnvironmentVariable(nameof(EnvVar.EMBED_THUMBNAIL)) ?? "";
    }

    [SlashCommand("help", "Displays command help")]
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
        var embed = new EmbedProperties
        {
            Title = "Help",
            Description =
                $"{command.ToString()}: {command.Description}",
            Fields =
            [
                new EmbedFieldProperties()
                {
                    Name = "Command is NSFW",
                    Value = command.Nsfw.ToString()
                }
            ],
            Color = new Color(39423), // 0099ff
            Thumbnail = _embedThumbnail,
            Footer = new EmbedFooterProperties
            {
                Text = $"BOT owner @{_user.Username}",
                IconUrl = _user.GetAvatarUrl()?.ToString()
            }
        };

        if (command.Options.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var option in command.Options)
            {
                sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description).Append('\n');
            }
            sb.Append('\n');
            embed.AddFields(new EmbedFieldProperties
            {
                Name = "Arguments",
                Value = sb.ToString().Trim()
            });
        }

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

        matchedCommands.RemoveAll(rec => rec.Name == "restart");
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
