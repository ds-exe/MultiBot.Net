using System.Text.Json;

namespace Multi_Bot.Net.Helpers;

public class ConfigHelper
{
    public static T GetJsonObject<T>(string file)
    {
        string cwd = Directory.GetCurrentDirectory();
        string path = cwd + $"/{file}.json";

        #if DEBUG
        path = cwd + $"/../../../{file}.json";
        #endif

        string txt = File.ReadAllText(path);
        T? result = JsonSerializer.Deserialize<T>(txt);
        if (result == null)
        {
            throw new Exception($"Error loading file {file}.json");
        }
        return result;
    }

    //public async static Task ConnectLavalink(LavalinkExtension lavalink)
    //{
    //    var config = GetJsonObject<Config>("config");
    //    if (config.LavalinkPassword == null)
    //    {
    //        return;
    //    };

    //    var endpoint = new ConnectionEndpoint
    //    {
    //        Hostname = "lavalink", // From your server configuration.
    //        Port = 2333 // From your server configuration
    //    };

    //    #if DEBUG
    //    endpoint = new ConnectionEndpoint
    //    {
    //        Hostname = "127.0.0.1", // From your server configuration.
    //        Port = 2333 // From your server configuration
    //    };
    //    #endif

    //    var lavalinkConfig = new LavalinkConfiguration
    //    {
    //        Password = config.LavalinkPassword, // From your server configuration.
    //        RestEndpoint = endpoint,
    //        SocketEndpoint = endpoint
    //    };

    //    try
    //    {
    //        await lavalink.ConnectAsync(lavalinkConfig);
    //    }
    //    catch { }
    //}
}
