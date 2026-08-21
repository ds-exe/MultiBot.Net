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
        InitialiseTable("ResinData(UserId INTEGER, Game TEXT, MaxResinTimestamp INTEGER, PRIMARY KEY(UserId, Game))");
        InitialiseTable("ResinNotification(UserId INTEGER, Game TEXT, NotificationTimestamp INTEGER, " +
            "MaxResinTimestamp INTEGER, PRIMARY KEY(UserId, Game, NotificationTimestamp))");
        InitialiseTable("CustomResinData(UserId INTEGER, Game TEXT, Resin INTEGER, PRIMARY KEY(UserId, Game))");
        InitialiseTable("UntilReminder(Id INTEGER PRIMARY KEY AUTOINCREMENT, ChannelId INTEGER, MessageId INTEGER UNIQUE, DueAtUnix INTEGER, MessageText TEXT)");
        InitialiseTable("UntilReminderSubscriber(ReminderId INTEGER, UserId INTEGER, PRIMARY KEY(ReminderId, UserId))");
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

            const string query = "INSERT INTO UntilReminder (ChannelId, MessageId, DueAtUnix, MessageText) VALUES (@ChannelId, @MessageId, @DueAtUnix, @MessageText); SELECT last_insert_rowid();";
            reminder.Id = _connection.ExecuteScalar<long>(query, reminder);
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

    public long? GetUntilReminderIdByMessageId(ulong messageId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return null;
            }

            const string query = "SELECT Id FROM UntilReminder WHERE MessageId = @messageId";
            return _connection.QuerySingleOrDefault<long?>(query, new { messageId });
        }
        catch
        {
            return null;
        }
    }

    public void InsertUntilReminderSubscriber(long reminderId, ulong userId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            const string query = "REPLACE INTO UntilReminderSubscriber (ReminderId, UserId) VALUES (@reminderId, @userId)";
            _connection.Execute(query, new { reminderId, userId });
        }
        catch
        {
            // ignored
        }
    }

    public void DeleteUntilReminderSubscriber(long reminderId, ulong userId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            const string query = "DELETE FROM UntilReminderSubscriber WHERE ReminderId = @reminderId AND UserId = @userId";
            _connection.Execute(query, new { reminderId, userId });
        }
        catch
        {
            // ignored
        }
    }

    public IReadOnlyList<ulong> GetUntilReminderSubscriberIds(long reminderId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return [];
            }

            const string query = "SELECT UserId FROM UntilReminderSubscriber WHERE ReminderId = @reminderId";
            return _connection.Query<ulong>(query, new { reminderId }).ToList();
        }
        catch
        {
            return [];
        }
    }

    public void DeleteUntilReminder(long reminderId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            const string deleteSubscribers = "DELETE FROM UntilReminderSubscriber WHERE ReminderId = @reminderId";
            const string deleteReminder = "DELETE FROM UntilReminder WHERE Id = @reminderId";
            _connection.Execute(deleteSubscribers, new { reminderId });
            _connection.Execute(deleteReminder, new { reminderId });
        }
        catch
        {
            // ignored
        }
    }
}
