using Dapper;
using Microsoft.Data.Sqlite;
using TimeZoneConverter;

namespace Multi_Bot.Net.Services;

public class DatabaseService
{
    private readonly SqliteConnection _connection;

    public DatabaseService()
    {
        var dbName = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "Multi_Bot.db";

        _connection = new SqliteConnection($"Data Source=Database/{dbName}");
        try
        {
            _connection.Open();
            InitialiseTables();
        }
        catch
        {
            Console.WriteLine("DB connection error, folder or name invalid");
        }
    }

    private void InitialiseTables()
    {
        InitialiseTable("TimeZoneData(UserId INTEGER PRIMARY KEY, TimeZoneId TEXT)");
        InitialiseTable("ResinData(UserId INTEGER, Game TEXT, MaxResinTimestamp INTEGER, PRIMARY KEY(UserId, Game))");
        InitialiseTable("ResinNotification(UserId INTEGER, Game TEXT, NotificationTimestamp INTEGER, " +
            "MaxResinTimestamp INTEGER, PRIMARY KEY(UserId, Game, NotificationTimestamp))");
        InitialiseTable("CustomResinData(UserId INTEGER, Game TEXT, Resin INTEGER, PRIMARY KEY(UserId, Game))");
    }

    private void InitialiseTable(string table)
    {
        var query = $"CREATE TABLE IF NOT EXISTS {table}";
        _connection.Execute(query);
    }

    public void InsertTimeZone(TimeZoneData tz)
    {
        try
        {
            const string query = $"REPLACE INTO TimeZoneData (UserId, TimeZoneId) VALUES (@UserId, @TimeZoneId)";
            _connection.Execute(query, tz);
        }
        catch
        {
            // ignored
        }
    }

    public TimeZoneInfo? GetTimeZone(ulong userId)
    {
        try
        {
            const string query = $"SELECT * FROM TimeZoneData WHERE UserId = @userId";
            var data = _connection.Query<TimeZoneData>(query, new { userId }).FirstOrDefault();
            return data == null ? null : TZConvert.GetTimeZoneInfo(data.TimeZoneId);
        }
        catch
        {
            return TZConvert.GetTimeZoneInfo("utc");
        }
    }
}