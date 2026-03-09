namespace Multi_Bot.Net.Modules;

public class HelpCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private const ulong creatorID = 74968333413257216;
    private User User;
    private string EmbedThumbnail;

    public HelpCommandModule(RestClient discordClient)
    {
        var config = ConfigHelper.GetJsonObject<Config>("config");
        var botOwner = config.Owner == 0 ? creatorID : config.Owner;
        User = discordClient.GetUserAsync(botOwner).Result;
        EmbedThumbnail = config.EmbedThumbnail ?? "";
    }

    // Initial code based on DisCatSharp default help command
    [SlashCommand("help", "Displays command help")]
    //public async Task Avatar([SlashCommandParameter(Description = "Selected User")] User? user = null)
    public async Task DefaultHelpAsync([SlashCommandParameter(Name = "option_one", 
                                                              Description = "top level command to provide help for", 
                                                              AutocompleteProviderType = typeof(DefaultHelpAutoCompleteProvider))] string commandName)
    {
        // List<DiscordApplicationCommand> applicationCommands = null;
        // var globalCommandsTask = ctx.Client.GetGlobalApplicationCommandsAsync();
        // if (ctx.Guild != null)
        // {
        //     var guildCommandsTask = ctx.Client.GetGuildApplicationCommandsAsync(ctx.Guild.Id);
        //     await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
        //     applicationCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
        //         .Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
        //         .GroupBy(ac => ac.Name).Select(x => x.First())
        //         .ToList();
        // }
        // else
        // {
        //     await Task.WhenAll(globalCommandsTask).ConfigureAwait(false);
        //     applicationCommands = globalCommandsTask.Result
        //         .Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
        //         .GroupBy(ac => ac.Name).Select(x => x.First())
        //         .ToList();
        // }
        //
        // if (applicationCommands.Count < 1)
        // {
        //     await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
        //             .WithContent("There are no slash commands").AsEphemeral()).ConfigureAwait(false);
        //     return;
        // }
        //
        // var command = applicationCommands.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        // if (command is null)
        // {
        //     await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
        //             .WithContent($"No command called {commandName} in guild {ctx.Guild.Name}").AsEphemeral()).ConfigureAwait(false);
        //     return;
        // }
        //
        // var discordEmbed = new DiscordEmbedBuilder
        // {
        //     Title = "Help",
        //     Description = $"{command.Mention}: {command.Description ?? "No description provided."}"
        // }.AddField(new("Command is NSFW", command.IsNsfw.ToString()));
        // // Custom help start
        // discordEmbed.Color = new DiscordColor("0099ff");
        // discordEmbed.WithThumbnail(EmbedThumbnail);
        // discordEmbed.WithFooter($"BOT owner @{User.Username}", User.AvatarUrl);
        // // Custom help end
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
    }

    private class DefaultHelpAutoCompleteProvider(RestClient client) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
            ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            var applicationId = ((IEntityToken)client.Token!).Id;
            var commands = await client.GetGlobalApplicationCommandsAsync(applicationId);
            IReadOnlyList<GuildApplicationCommand> guildCommands = [];
            var guildId = context.Guild?.Id;
            if (guildId != null)
            {
                guildCommands = await client.GetGuildApplicationCommandsAsync(applicationId, guildId.Value);
            }

            string userInput = option.Value ?? string.Empty;
            List<string> commandNames = [];
            if (commands.Count > 0)
            {
                var foundCommands = commands.Where(rec => rec.Name.Contains(userInput, StringComparison.OrdinalIgnoreCase)).ToList();
                commandNames.AddRange(foundCommands.Select(rec => rec.Name));
            }

            if (guildCommands.Count > 0)
            {
                var foundCommands = guildCommands.Where(rec => rec.Name.Contains(userInput, StringComparison.OrdinalIgnoreCase)).ToList();
                commandNames.AddRange(foundCommands.Select(rec => rec.Name));
            }

            return commandNames.Distinct().Select(rec => new ApplicationCommandOptionChoiceProperties(rec, rec)); 
        }

        // public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
        // {
        //     IEnumerable<DiscordApplicationCommand> slashCommands = null;
        //     var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
        //     if (context.Guild != null)
        //     {
        //         var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
        //         await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
        //         slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
        //             .Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
        //             .GroupBy(ac => ac.Name).Select(x => x.First())
        //             .Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase))
        //             .ToList();
        //     }
        //     else
        //     {
        //         await Task.WhenAll(globalCommandsTask).ConfigureAwait(false);
        //         slashCommands = globalCommandsTask.Result
        //             .Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
        //             .GroupBy(ac => ac.Name).Select(x => x.First())
        //             .Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase))
        //             .ToList();
        //     }
        //
        //     var options = slashCommands.Take(25).Select(sc => new DiscordApplicationCommandAutocompleteChoice(sc.Name, sc.Name.Trim())).ToList();
        //     return options.AsEnumerable();
        // }
    }
}
