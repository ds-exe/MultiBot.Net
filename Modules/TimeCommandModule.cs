using System.Globalization;
using TimeZoneConverter;

namespace Multi_Bot.Net.Modules;

public class TimeCommandModule(DatabaseService databaseService) : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly Dictionary<string, string> TimeZones = new()
    {
        { "utc", "UTC" },
        { "gmt", "UTC" },
        { "bst", "Etc/GMT-1" },
        { "cet", "Etc/GMT-1" },
        { "cest", "Etc/GMT-2" },
        { "cst", "Etc/GMT+6" },
        { "cdt", "Etc/GMT+5" },
        { "ct", "Canada/Central" },
        { "est", "Etc/GMT+5" },
        { "edt", "Etc/GMT+4" },
        { "et", "Canada/Eastern" },
        { "jst", "Etc/GMT-9" },
        { "pst", "Etc/GMT+8" },
        { "pdt", "Etc/GMT+7" },
        { "pt", "Canada/Pacific" }
    };

    private const string SlashDateRegex = @"^(\d{1,2})/(\d{1,2})/?(\d{4})?$";
    private const string DotDateRegex = @"^(\d{1,2})\.(\d{1,2})\.?(\d{4})?$";
    private const string IsoDateRegex = @"^(\d{4})-(\d{1,2})-(\d{1,2})$";
    private const string TimezoneRegex = @"^utc(\+|-)([0-9]{1,2})$";

    [SlashCommand("time", "Gets the given time embed.")]
    public async Task Time([SlashCommandParameter(Name = "time", Description = "Selected Time", MinLength = 5, MaxLength = 5)] string time,
        [SlashCommandParameter(Name = "date", Description = "Selected Date", MinLength = 3, MaxLength = 10)] string? date = null,
        [SlashCommandParameter(Name = "timezone", Description = "Selected Timezone")] string? timezone = null)
    {
        await SendTimeEmbed(Context.Interaction, time, date, timezone, TimestampStyle.LongDateTime);
    }

    [SlashCommand("until", "Gets the given time embed.")]
    public async Task Until([SlashCommandParameter(Name = "time", Description = "Selected Time", MinLength = 5, MaxLength = 5)] string time,
        [SlashCommandParameter(Name = "date", Description = "Selected Date", MinLength = 3, MaxLength = 10)] string? date = null,
        [SlashCommandParameter(Name = "timezone", Description = "Selected Timezone")] string? timezone = null)
    {
        await SendTimeEmbed(Context.Interaction, time, date, timezone, TimestampStyle.RelativeTime);
    }

    [SlashCommand("now", "Gets the given time.")]
    public async Task Now([SlashCommandParameter(Name = "timezone", Description = "Selected Timezone")] string? timezone = null)
    {
        var zone = GetTimeZone(timezone, Context.User.Id);
        if (zone == null)
        {
            await InteractionHelper.SendResponse(Context.Interaction, text: "Invalid timezone", isEphemeral: true);
            return;
        }

        await InteractionHelper.SendResponse(Context.Interaction, text: $"`{TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone):dd MMM yyyy, HH:mm}`");
    }

    [SlashCommand("timezone", "Gets / Sets timezone.")]
    public async Task Timezone([SlashCommandParameter(Name = "timezone", Description = "Selected Timezone")] string? timezone = null)
    {
        if (timezone == null)
        {
            var timeZoneInfo = databaseService.GetTimeZone(Context.User.Id);
            if (timeZoneInfo != null)
            {
                await InteractionHelper.SendResponse(Context.Interaction, text: timeZoneInfo.DisplayName, isEphemeral: true);
            }
            else
            {
                await InteractionHelper.SendResponse(Context.Interaction, text: "No Time Zone data set", isEphemeral: true);
            }
            return;
        }

        var zone = GetTimeZone(timezone, Context.User.Id);
        if (zone != null)
        {
            databaseService.InsertTimeZone(new TimeZoneData { UserId = Context.User.Id, TimeZoneId = zone.Id });
            await InteractionHelper.SendResponse(Context.Interaction, text: "Time Zone set", isEphemeral: true);
            return;
        }
        await InteractionHelper.SendResponse(Context.Interaction, text: "Invalid Time Zone", isEphemeral: true);
    }

    private async Task SendTimeEmbed(ApplicationCommandInteraction interaction, string time, string? date, string? timezone, TimestampStyle format)
    {
        var zone = GetTimeZone(timezone, interaction.User.Id);
        if (zone == null)
        {
            await InteractionHelper.SendResponse(interaction, text: "Invalid timezone", isEphemeral: true);
            return;
        }

        date = ParseDate(date, zone);
        if (date == null)
        {
            await InteractionHelper.SendResponse(interaction, text: "Invalid date", isEphemeral: true);
            return;
        }

        var success = DateTime.TryParseExact($"{time} {date}", "HH:mm dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var datetime);
        if (!success)
        {
            await InteractionHelper.SendResponse(interaction, text: "Invalid time", isEphemeral: true);
            return;
        }

        var timestamp = new Timestamp(TimeZoneInfo.ConvertTimeToUtc(datetime, zone), format).ToString();
        await InteractionHelper.SendResponse(interaction, embed: GetTimestampEmbed(timestamp));
    }

    private static EmbedProperties GetTimestampEmbed(string time)
    {
        return new EmbedProperties()
        {
            Title = "Local time:",
            Description = time,
            Color = new Color(65535), //00ffff
            Fields =
            [
                new EmbedFieldProperties()
                {
                    Name = "Copy Link:",
                    Value = $"\\{time}"
                }
            ],
        };
    }

    private static string? ParseDate(string? date, TimeZoneInfo zone)
    {
        var baseDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
        if (date == null)
        {
            return baseDate.ToString("dd/MM/yyyy");
        }

        var slashMatches = Regex.Match(date, SlashDateRegex);
        if (slashMatches.Success)
        {
            var year = slashMatches.Groups[3].Value;
            return $"{slashMatches.Groups[1].Value.PadLeft(2, '0')}/{slashMatches.Groups[2].Value.PadLeft(2, '0')}/{(year != string.Empty ? year : baseDate.Year)}";
        }

        var dotMatches = Regex.Match(date, DotDateRegex);
        if (dotMatches.Success)
        {
            var year = dotMatches.Groups[3].Value;
            return $"{dotMatches.Groups[1].Value.PadLeft(2, '0')}/{dotMatches.Groups[2].Value.PadLeft(2, '0')}/{(year != string.Empty ? year : baseDate.Year)}";
        }

        var isoMatches = Regex.Match(date, IsoDateRegex);
        if (isoMatches.Success)
        {
            return $"{isoMatches.Groups[3].Value.PadLeft(2, '0')}/{isoMatches.Groups[2].Value.PadLeft(2, '0')}/{isoMatches.Groups[1].Value}";
        }

        return null;
    }

    private TimeZoneInfo? GetTimeZone(string? timezone, ulong uid)
    {
        try
        {
            if (timezone == null)
            {
                var userTimeZone = databaseService.GetTimeZone(uid);
                return userTimeZone ?? TZConvert.GetTimeZoneInfo("utc");
            }

            var success = TimeZones.TryGetValue(timezone.ToLower(), out var result);
            if (success && result != null)
            {
                return TZConvert.GetTimeZoneInfo(result);
            }

            var matches = Regex.Match(timezone.ToLower(), TimezoneRegex);
            if (!matches.Success)
            {
                return TZConvert.GetTimeZoneInfo(timezone);
            }

            var sign = matches.Groups[1].Value == "+" ? "-" : "+"; // Inverted due to Etc/GMT using reversed offsets
            var value = Math.Abs(int.Parse(matches.Groups[2].Value));
            return TZConvert.GetTimeZoneInfo($"Etc/GMT{sign}{value}");
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
    }
}
