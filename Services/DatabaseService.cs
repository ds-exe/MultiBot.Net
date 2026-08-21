using Dapper;
using Microsoft.Data.Sqlite;
using TimeZoneConverter;

namespace Multi_Bot.Net.Services;

public class DatabaseService
{
    private readonly SqliteConnection _connection;

    public DatabaseService()
    {
        var dbPath = Environment.GetEnvironmentVariable(nameof(EnvVar.DATABASE_PATH)) ?? "/app/data/Multi_Bot.db";
        _connection = new SqliteConnection($"Data Source={dbPath}");
        try
        {
            _connection.Open();
        }
        catch (SqliteException)
        { 
            Console.WriteLine("DB connection error");
            _connection.Close();
            return;
        }
        InitialiseTables();
    }

    private void InitialiseTables()
    {
        InitialiseTable("TimeZoneData(UserId INTEGER PRIMARY KEY, TimeZoneId TEXT)");
        InitialiseTable("UntilReminder(MessageId INTEGER PRIMARY KEY, ChannelId INTEGER, NotifyTimestamp INTEGER, MessageText TEXT)");
        InitialiseTable("UntilReminderUser(MessageId INTEGER, UserId INTEGER, applicationId INTEGER, token string, PRIMARY KEY(MessageId, UserID))");
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
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }
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
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return TZConvert.GetTimeZoneInfo("utc");
            }
            const string query = $"SELECT * FROM TimeZoneData WHERE UserId = @userId";
            var data = _connection.Query<TimeZoneData>(query, new { userId }).FirstOrDefault();
            return data == null ? null : TZConvert.GetTimeZoneInfo(data.TimeZoneId);
        }
        catch
        {
            return TZConvert.GetTimeZoneInfo("utc");
        }
    }
    
    
    public UntilReminderData InsertUntilReminder(UntilReminderData reminder)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return reminder;
            }

            const string query = "REPLACE INTO UntilReminder (MessageId, ChannelId, NotifyTimestamp, MessageText) VALUES (@MessageId, @ChannelId, @NotifyTimestamp, @MessageText)";
            _connection.Execute(query, reminder);
        }
        catch
        {
            // ignored
        }

        return reminder;
    }

    public IEnumerable<UntilReminderData> GetPendingUntilReminders()
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return [];
            }

            const string query = "SELECT * FROM UntilReminder";
            return _connection.Query<UntilReminderData>(query).ToList();
        }
        catch
        {
            return [];
        }
    }

    public void InsertUntilReminderUser(UntilReminderUserData reminderUser)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            const string query = "REPLACE INTO UntilReminderUser (MessageId, UserId) VALUES (@MessageId, @UserId, @ApplicationId, @Token)";
            _connection.Execute(query, reminderUser);
        }
        catch
        {
            // ignored
        }
    }

    public void DeleteUntilReminderUser(ulong messageId, ulong userId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            const string query = "DELETE FROM UntilReminderUser WHERE MessageId = @messageId AND UserId = @userId";
            _connection.Execute(query, new { messageId, userId });
        }
        catch
        {
            // ignored
        }
    }

    public IReadOnlyList<UntilReminderUserData> GetUntilReminderUsers(ulong messageId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return [];
            }

            const string query = "SELECT * FROM UntilReminderUser WHERE MessageId = @messageId";
            return _connection.Query<UntilReminderUserData>(query, new { messageId }).ToList();
        }
        catch
        {
            return [];
        }
    }

    public void DeleteUntilReminder(ulong messageId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            const string deleteUsers = "DELETE FROM UntilReminderUser WHERE MessageId = @messageId";
            const string deleteReminder = "DELETE FROM UntilReminder WHERE MessageId = @messageId";
            _connection.Execute(deleteUsers, new { messageId });
            _connection.Execute(deleteReminder, new { messageId });
        }
        catch
        {
            // ignored
        }
    }
}
