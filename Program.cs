using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Multi_Bot.Net;

public class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
        var token = Environment.GetEnvironmentVariable(nameof(EnvVar.TOKEN));
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("Please set environment variable TOKEN.");
            return;
        }

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddSingleton<DatabaseService>()
            .AddDiscordGateway(gatewayOptions => SetGatewayClientOptions(gatewayOptions, token))
            .AddApplicationCommands(option =>
            {
                // Commands are manually registered below to support Test Server
                option.AutoRegisterCommands = false;
            });

        var logLevel = Environment.GetEnvironmentVariable(nameof(EnvVar.LOGGING_LEVEL))?.ToUpper();
        if (string.IsNullOrWhiteSpace(logLevel))
        {
            builder.Logging.SetMinimumLevel(LogLevel.None);
        }
        else
        {
            foreach (var level in Enum.GetNames<LogLevel>())
            {
                if (level.ToUpper() == logLevel || level[0..4].ToUpper() == logLevel)
                {
                    builder.Logging.SetMinimumLevel(Enum.Parse<LogLevel>(level));
                }
            }
        }

        var host = builder.Build();

        #region register commands
        var removeCommandsEnvVar = Environment.GetEnvironmentVariable(nameof(EnvVar.REMOVE_COMMANDS));
        bool.TryParse(removeCommandsEnvVar, out var removeCommands);
        if (!removeCommands)
        {
            host.AddModules(typeof(Program).Assembly);
        }

        // make sure the minimal API style commands are added
        foreach (var commandsBuilder in host.Services.GetServices<IApplicationCommandsBuilder>())
        {
            commandsBuilder.Build();
        }

        // add registered services to the manager
        ApplicationCommandServiceManager manager = new();
        foreach (var service in host.Services.GetServices<IApplicationCommandService>())
        {
            manager.AddService(service);
        }

        var testServersEnvVar = Environment.GetEnvironmentVariable(nameof(EnvVar.TEST_SERVERS));
        var testServerList = testServersEnvVar?.Split(',').Select(ulong.Parse).ToList() ?? [];

        var client = host.Services.GetRequiredService<RestClient>();
        var applicationId = ((IEntityToken)client.Token!).Id;
        if (testServerList.Count == 0)
        {
            if (!removeCommands)
            {
                await manager.RegisterCommandsAsync(client, applicationId);
            }
            else
            {
                await client.BulkOverwriteGlobalApplicationCommandsAsync(applicationId, []);
            }
        }
        else
        {
            foreach (var guildId in testServerList)
            {
                if (!removeCommands)
                {
                    await manager.RegisterCommandsAsync(client, applicationId, guildId);
                }
                else
                {
                    await client.BulkOverwriteGuildApplicationCommandsAsync(applicationId, guildId, []);
                }
            }
        }
        #endregion

        await host.RunAsync();
    }

    private static void SetGatewayClientOptions(GatewayClientOptions gatewayOptions, string token)
    {
        gatewayOptions.Token = token;
        gatewayOptions.Presence = new PresenceProperties(UserStatusType.Online).AddActivities([new UserActivityProperties("/help", UserActivityType.Listening)]);
    }
}
