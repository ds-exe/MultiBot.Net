using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking.Trackers.Users;
using Lavalink4NET.NetCord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MultiBot.Net;

public class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
        var config = ConfigHelper.GetJsonObject<Config>("config");
        if (config.Token == null || config.LavalinkPassword == null)
        {
            return;
        };

        var builder = Host.CreateApplicationBuilder(args);

        var hostname = "lavalink";
        #if DEBUG
        hostname = "127.0.0.1";
        #endif

        builder.Services
            .AddDiscordGateway(SetGatewayClientOptions)
            .AddApplicationCommands()
            .AddLavalink()
            .AddInactivityTracking()
            .ConfigureLavalink(cfg =>
            {
                cfg.BaseAddress = new Uri($"http://{hostname}:2333");
                cfg.Passphrase = config.LavalinkPassword;
                cfg.ReadyTimeout = TimeSpan.FromSeconds(10);
            })
            .ConfigureInactivityTracking(config =>
            {
                config.DefaultTimeout = TimeSpan.FromMinutes(15);
            })
            .Configure<UsersInactivityTrackerOptions>(config =>
            {
                config.Threshold = 0;
            });

        var host = builder.Build();

        host.AddModules(typeof(Program).Assembly);

        host.UseGatewayEventHandlers();

        await host.RunAsync();
    }

    static void SetGatewayClientOptions(GatewayClientOptions gatewayOptions)
    {
        var config = ConfigHelper.GetJsonObject<Config>("config");

        gatewayOptions.Token = config.Token;
        gatewayOptions.Presence = new PresenceProperties(UserStatusType.Online).AddActivities([new UserActivityProperties("/help", UserActivityType.Listening)]);
    }
}
