namespace MultiBot.Net.Models;

public class Queue
{
    //protected const int timeoutMinutes = 15;
    //private Random _rand = new Random();
    //private List<QueueEntry> QueueEntries = new();
    //private QueueEntry? PreviousQueueEntry = null;
    //private QueueService _queueService;
    //private LavalinkGuildPlayer _player;

    //public Queue(QueueService queueService, LavalinkGuildPlayer player)
    //{
    //    _queueService = queueService;
    //    _player = player;
    //    player.TrackEnded += Player_TrackEnded;
    //}

    //public void AddTrack(DiscordChannel channel, DiscordUser user, LavalinkTrack track)
    //{
    //    QueueEntries.Add(new QueueEntry(channel, user, track));
    //}

    //public void ClearQueue()
    //{
    //    QueueEntries = new();
    //}

    //protected QueueEntry? GetNextQueueEntry()
    //{
    //    PreviousQueueEntry = QueueEntries.FirstOrDefault();
    //    if (PreviousQueueEntry == null)
    //    {
    //        return PreviousQueueEntry;
    //    }
    //    QueueEntries.Remove(PreviousQueueEntry);
    //    return PreviousQueueEntry;
    //}

    //public DiscordUser? GetCurrentTrackUser()
    //{
    //    return PreviousQueueEntry?.User;
    //}

    //public void Shuffle()
    //{
    //    QueueEntries = QueueEntries.OrderBy(_ => _rand.Next()).ToList();
    //}

    //protected async Task Player_TrackEnded(LavalinkGuildPlayer sender, DisCatSharp.Lavalink.EventArgs.LavalinkTrackEndedEventArgs e)
    //{
    //    await PlayQueueAsync();
    //}

    //public async Task PlayQueueAsync()
    //{
    //    if (_player.CurrentTrack != null)
    //    {
    //        return;
    //    }

    //    try
    //    {
    //        if (PreviousQueueEntry?.PlayingMessage?.Id != null)
    //        {
    //            await PreviousQueueEntry.PlayingMessage.DeleteAsync();
    //        }
    //    }
    //    catch { }

    //    await Task.Delay(1000);
    //    var next = GetNextQueueEntry();
    //    if (next == null)
    //    {
    //        _queueService.SetLastPlayed(_player.GuildId);
    //        await Task.Delay(1000);
    //        StartTimeout();
    //        return;
    //    }
    //    await _player.PlayAsync(next.Track);
    //    if (PreviousQueueEntry != null)
    //    {
    //        PreviousQueueEntry.PlayingMessage = await next.Channel.SendMessageAsync(EmbedHelper.GetTrackPlayingEmbed(next.Track.Info));
    //    }
    //}

    //protected async void StartTimeout()
    //{
    //    await Task.Delay(timeoutMinutes * 60 * 1000);
    //    if (_queueService.GetLastPlayed(_player.GuildId) <= DateTime.UtcNow.AddMinutes(-timeoutMinutes))
    //    {
    //        if (_player.CurrentTrack == null)
    //        {
    //            _player.TrackEnded -= Player_TrackEnded;
    //            _queueService.RemoveLastPlayed(_player.GuildId);
    //            _queueService.RemoveQueue(_player.GuildId);
    //            await _player.DisconnectAsync();
    //        }
    //    }
    //}
}
