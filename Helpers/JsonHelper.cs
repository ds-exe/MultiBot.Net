using System.Text.Json;

namespace Multi_Bot.Net.Helpers;

public static class JsonHelper
{
    public static T GetJsonObject<T>(string file)
    {
        var cwd = Directory.GetCurrentDirectory();
        // ReSharper disable once RedundantAssignment
        var path = cwd + $"/{file}.json";

        #if DEBUG
        path = cwd + $"/../../../{file}.json";
        #endif

        var txt = File.ReadAllText(path);
        var result = JsonSerializer.Deserialize<T>(txt);
        return result ?? throw new Exception($"Error loading file {file}.json");
    }
}
