using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace CappyCleaner.Core;

public sealed class SnapshotStore : IDisposable
{
    private readonly SqliteConnection connection;
    private SqliteTransaction? transaction;
    private int pending;

    public SnapshotStore(string database)
    {
        connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database, Pooling = false, DefaultTimeout = 5 }.ToString());
        connection.Open();
    }

    public void Initialize()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS summary (id INTEGER PRIMARY KEY CHECK(id=1), json TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS entries (
                id INTEGER PRIMARY KEY, parent INTEGER, path TEXT NOT NULL, kind TEXT NOT NULL,
                bytes INTEGER, files INTEGER NOT NULL, status TEXT NOT NULL, detail TEXT, empty INTEGER NOT NULL);
            CREATE INDEX IF NOT EXISTS ix_parent ON entries(parent);
            CREATE INDEX IF NOT EXISTS ix_size ON entries(bytes DESC);
            CREATE INDEX IF NOT EXISTS ix_status ON entries(status);
            """;
        command.ExecuteNonQuery();
        transaction = connection.BeginTransaction();
    }

    public void Write(ScanEntry entry)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT OR REPLACE INTO entries VALUES($id,$parent,$path,$kind,$bytes,$files,$status,$detail,$empty)";
        command.Parameters.AddWithValue("$id", entry.Id);
        command.Parameters.AddWithValue("$parent", (object?)entry.ParentId ?? DBNull.Value);
        command.Parameters.AddWithValue("$path", entry.Path);
        command.Parameters.AddWithValue("$kind", entry.Kind);
        command.Parameters.AddWithValue("$bytes", (object?)entry.LogicalBytes ?? DBNull.Value);
        command.Parameters.AddWithValue("$files", entry.Files);
        command.Parameters.AddWithValue("$status", entry.Status);
        command.Parameters.AddWithValue("$detail", (object?)entry.Detail ?? DBNull.Value);
        command.Parameters.AddWithValue("$empty", entry.IsEmpty ? 1 : 0);
        command.ExecuteNonQuery();
        if (++pending >= 1000) Flush();
    }

    public void SaveSummary(ScanSummary summary)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT OR REPLACE INTO summary VALUES (1,$json)";
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(summary));
        command.ExecuteNonQuery();
        Flush();
    }

    public void Flush()
    {
        transaction?.Commit(); transaction?.Dispose();
        transaction = connection.BeginTransaction(); pending = 0;
    }

    public IReadOnlyList<ScanEntry> GetEntries(long? parent = null, string? kind = null, string search = "", int limit = 200, int offset = 0)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT * FROM entries WHERE parent IS NOT NULL
            AND ($parent IS NULL OR parent=$parent) AND ($kind IS NULL OR kind=$kind)
            AND ($search='' OR instr(lower(path),lower($search))>0)
            ORDER BY bytes DESC, path COLLATE NOCASE LIMIT $limit OFFSET $offset
            """;
        command.Parameters.AddWithValue("$parent", (object?)parent ?? DBNull.Value);
        command.Parameters.AddWithValue("$kind", (object?)kind ?? DBNull.Value);
        command.Parameters.AddWithValue("$search", search);
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 1000));
        command.Parameters.AddWithValue("$offset", Math.Max(0, offset));
        return ReadEntries(command);
    }

    public IReadOnlyList<ScanEntry> GetIssues()
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM entries WHERE status NOT IN ('complete','partial') ORDER BY id LIMIT 1000";
        return ReadEntries(command);
    }

    public IEnumerable<ScanEntry> EnumerateEntries()
    {
        using var command = connection.CreateCommand(); command.CommandText = "SELECT * FROM entries ORDER BY id";
        using var reader = command.ExecuteReader();
        while (reader.Read()) yield return ReadEntry(reader);
    }

    private static List<ScanEntry> ReadEntries(SqliteCommand command)
    {
        var result = new List<ScanEntry>(); using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadEntry(reader));
        return result;
    }
    private static ScanEntry ReadEntry(SqliteDataReader r) => new(r.GetInt64(0), r.IsDBNull(1) ? null : r.GetInt64(1), r.GetString(2), r.GetString(3), r.IsDBNull(4) ? null : r.GetInt64(4), r.GetInt64(5), r.GetString(6), r.IsDBNull(7) ? null : r.GetString(7), r.GetInt64(8) != 0);

    public ScanSummary? GetSummary()
    {
        using var command = connection.CreateCommand(); command.CommandText = "SELECT json FROM summary WHERE id=1";
        return command.ExecuteScalar() is string json ? JsonSerializer.Deserialize<ScanSummary>(json) : null;
    }

    public void Dispose()
    {
        transaction?.Commit(); transaction?.Dispose(); connection.Dispose();
    }
}
