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
        if (config.LavalinkPassword == null)
        {
            return;
        };

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddDiscordGateway(SetGatewayClientOptions)
            .AddApplicationCommands(option =>
            {
                option.AutoRegisterCommands = false;
            });

        var host = builder.Build();

        host.AddModules(typeof(Program).Assembly);
        
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

        ulong applicationId = ((IEntityToken)client.Token!).Id;

        //Also you will probably want to set AutoRegisterCommands to false in the configuration.
        if (config.TestServer == null)
        {
            await manager.RegisterCommandsAsync(client, applicationId);
        }
        else
        {
            foreach (var guildId in config.TestServer)
            {
                // register the commands
                await manager.RegisterCommandsAsync(client, applicationId, guildId);
            }
        }

        await host.RunAsync();
    }

    private static void SetGatewayClientOptions(GatewayClientOptions gatewayOptions)
    {
        var config = ConfigHelper.GetJsonObject<Config>("config");

        gatewayOptions.Token = config.Token;
        gatewayOptions.Presence = new PresenceProperties(UserStatusType.Online).AddActivities([new UserActivityProperties("/help", UserActivityType.Listening)]);
    }
}
