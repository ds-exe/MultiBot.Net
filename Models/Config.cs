namespace MultiBot.Net.Models;

public class Config
{
    public required string Token { get; set; }

    public string? LavalinkPassword { get; set; }

    public string? EmbedThumbnail { get; set; }

    public bool ReconnectIndefinitely { get; set; } = true;

    public bool EnableResinModule { get; set; }

    public ulong Owner { get; set; }

    public ulong[]? TestServer { get; set; }
}
