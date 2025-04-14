using Microsoft.Extensions.Hosting;
using NetCord.Gateway;

namespace MultiBot.Net;

public class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddDiscordGateway(SetGatewayClientOptions);

        var host = builder.Build();

        await host.RunAsync();
    }

    static void SetGatewayClientOptions(GatewayClientOptions gatewayOptions)
    {
        var config = ConfigHelper.GetJsonObject<Config>("config");
        if (config.Token == null || config.LavalinkPassword == null)
        {
            return;
        };

        gatewayOptions.Token = config.Token;
        gatewayOptions.Presence = new PresenceProperties(UserStatusType.Online).AddActivities([new UserActivityProperties("/help", UserActivityType.Listening)]);
    }
}
