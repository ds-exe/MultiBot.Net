namespace Multi_Bot.Net.Models;

public class Config
{
    public required string Token { get; set; }

    public string? EmbedThumbnail { get; set; }

    public ulong Owner { get; set; }

    public ulong[]? TestServer { get; set; }
}
