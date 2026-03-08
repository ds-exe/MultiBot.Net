using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Multi_Bot.Net;

public class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
        var config = ConfigHelper.GetJsonObject<Config>("config");

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddDiscordGateway(gatewayOptions => SetGatewayClientOptions(gatewayOptions, config))
            .AddApplicationCommands(option =>
            {
                // Commands are manually registered below to support Test Server
                option.AutoRegisterCommands = false;
            });

        var host = builder.Build();

        host.AddModules(typeof(Program).Assembly);

        #region register commands
        var client = host.Services.GetRequiredService<RestClient>();

        // make sure the minimal API style commands are added
        foreach (var commandsBuilder in host.Services.GetServices<IApplicationCommandsBuilder>())
        {
            commandsBuilder.Build();
        }

        ApplicationCommandServiceManager manager = new();

        // add registered services to the manager
        foreach (var service in host.Services.GetServices<IApplicationCommandService>())
        {
            manager.AddService(service);
        }

        var applicationId = ((IEntityToken)client.Token!).Id;

        if (config.TestServer == null)
        {
            // register the global commands
            await manager.RegisterCommandsAsync(client, applicationId);
        }
        else
        {
            foreach (var guildId in config.TestServer)
            {
                // register the guild commands
                await manager.RegisterCommandsAsync(client, applicationId, guildId);
            }
        }
        #endregion

        await host.RunAsync();
    }

    private static void SetGatewayClientOptions(GatewayClientOptions gatewayOptions, Config config)
    {
        gatewayOptions.Token = config.Token;
        gatewayOptions.Presence = new PresenceProperties(UserStatusType.Online).AddActivities([new UserActivityProperties("/help", UserActivityType.Listening)]);
    }
}
