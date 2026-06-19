using System.IO;
using Microsoft.Data.Sqlite;

namespace POS.Data;

public static class DatabaseService
{
    private static string _dbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "database.db");

    public static string ConnectionString =>
        $"Data Source={_dbPath};";

    public static SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    public static int InsertAndGetId(string sql, Dictionary<string, object?>? parameters = null)
    {
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        AddParameters(cmd, parameters);
        cmd.ExecuteNonQuery();
        using var idCmd = new SqliteCommand("SELECT last_insert_rowid()", conn);
        return (int)(long)(idCmd.ExecuteScalar() ?? 0L);
    }

    public static int ExecuteNonQuery(string sql, Dictionary<string, object?>? parameters = null)
    {
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        AddParameters(cmd, parameters);
        return cmd.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string sql, Dictionary<string, object?>? parameters = null)
    {
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        AddParameters(cmd, parameters);
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? null : result;
    }

    public static List<T> ExecuteReader<T>(string sql,
        Func<SqliteDataReader, T> map,
        Dictionary<string, object?>? parameters = null)
    {
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        AddParameters(cmd, parameters);
        using var reader = cmd.ExecuteReader();
        var list = new List<T>();
        while (reader.Read())
            list.Add(map(reader));
        return list;
    }

    public static T? ExecuteReaderSingle<T>(string sql,
        Func<SqliteDataReader, T> map,
        Dictionary<string, object?>? parameters = null) where T : class
    {
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(sql, conn);
        AddParameters(cmd, parameters);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? map(reader) : null;
    }

    private static void AddParameters(SqliteCommand cmd, Dictionary<string, object?>? parameters)
    {
        if (parameters == null) return;
        foreach (var kv in parameters)
            cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
    }

    public static string GetString(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        return r.IsDBNull(ord) ? string.Empty : r.GetString(ord);
    }

    public static string? GetStringNull(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        return r.IsDBNull(ord) ? null : r.GetString(ord);
    }

    public static int GetInt(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        return r.IsDBNull(ord) ? 0 : r.GetInt32(ord);
    }

    public static int? GetIntNull(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        return r.IsDBNull(ord) ? null : r.GetInt32(ord);
    }

    public static decimal GetDecimal(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        return r.IsDBNull(ord) ? 0m : (decimal)r.GetDouble(ord);
    }

    public static bool GetBool(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        return !r.IsDBNull(ord) && r.GetInt32(ord) == 1;
    }

    public static DateTime GetDateTime(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        if (r.IsDBNull(ord)) return DateTime.MinValue;
        return DateTime.TryParse(r.GetString(ord), out var dt) ? dt : DateTime.MinValue;
    }

    public static DateTime? GetDateTimeNull(SqliteDataReader r, string col)
    {
        int ord = r.GetOrdinal(col);
        if (r.IsDBNull(ord)) return null;
        return DateTime.TryParse(r.GetString(ord), out var dt) ? dt : null;
    }
}
