using System.Globalization;
using NetCord.Services.Commands;
using TimeZoneConverter;

namespace Multi_Bot.Net.Modules;

public class TimeCommandModule(DatabaseService databaseService) : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly Dictionary<string, string> TimeZones =
        ConfigHelper.GetJsonObject<Dictionary<string, string>>("timezones");

    private const string DateRegex = @"^(\d{2})/(\d{2})/?(\d{4})?$";

    [SlashCommand("time", "Gets the given time embed.")]
    public async Task Time([SlashCommandParameter(Name = "time", Description = "Selected Time", MinLength = 5, MaxLength = 5)] string time,
        [SlashCommandParameter(Name = "date", Description = "Selected Date", MinLength = 5, MaxLength = 10)] string? date = null,
        [SlashCommandParameter(Name = "timezone", Description = "Selected Timezone")] string? timezone = null)
    {
        await SendTimeEmbed(Context.Interaction, time, date, timezone, TimestampStyle.LongDateTime);
    }

    [SlashCommand("until", "Gets the given time embed.")]
    public async Task Until([SlashCommandParameter(Name = "time", Description = "Selected Time", MinLength = 5, MaxLength = 5)] string time,
        [SlashCommandParameter(Name = "date", Description = "Selected Date", MinLength = 5, MaxLength = 10)] string? date = null,
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

        var matches = Regex.Match(date, DateRegex);
        if (!matches.Success)
        {
            return null;
        }

        var year = matches.Groups[3].Value;
        return $"{matches.Groups[1].Value}/{matches.Groups[2].Value}/{(year != string.Empty ? year : baseDate.Year)}";
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
            
            var matches = Regex.Match(timezone.ToLower(), @"^utc(\+|-)([0-9]{1,2})$");
            if (!matches.Success)
            {
                return TZConvert.GetTimeZoneInfo(timezone);
            }
            
            var sign = int.Parse(matches.Groups[2].Value) > 0
                ? "-"
                : "+"; // Inverted due to Etc/GMT using reversed offsets
            var value = Math.Abs(int.Parse(matches.Groups[2].Value));
            return TZConvert.GetTimeZoneInfo($"Etc/GMT{sign}{value}");
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
    }
}
