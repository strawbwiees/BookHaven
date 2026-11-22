using System.IO;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace BookHaven.Services;

public static class Database
{
    private static string? _connectionString;

    public static void Configure(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static SqlConnection Open()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public static void EnsureSchema()
    {
        using var conn = Open();

        RunSqlFile(conn, "Database/schema.sql");

        // Seed only if Books table is empty
        if (IsEmpty(conn, "dbo.Books"))
            RunSqlFile(conn, "Database/seed.sql");
    }

    private static bool IsEmpty(SqlConnection conn, string table)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM {table}) THEN 1 ELSE 0 END";
        return (int)cmd.ExecuteScalar() == 0;
    }

    private static void RunSqlFile(SqlConnection conn, string relativePath)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var fullPath = Path.Combine(baseDir, relativePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"SQL file not found: {fullPath}");

        var sql = File.ReadAllText(fullPath);
        var batches = SplitBatches(sql);

        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = trimmed;
            cmd.ExecuteNonQuery();
        }
    }

    private static List<string> SplitBatches(string sql)
    {
        var batches = new List<string>();
        var current = new System.Text.StringBuilder();
        var inString = false;
        var stringChar = '\0';
        var escaped = false;

        for (int i = 0; i < sql.Length; i++)
        {
            var c = sql[i];

            if (escaped)
            {
                current.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\' && inString)
            {
                current.Append(c);
                escaped = true;
                continue;
            }

            if (!inString && (c == '\'' || c == '"'))
            {
                inString = true;
                stringChar = c;
            }
            else if (inString && c == stringChar)
            {
                inString = false;
                stringChar = '\0';
            }

            if (!inString && c == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
            {
                // Single-line comment: skip to end of line
                while (i < sql.Length && sql[i] != '\n')
                    i++;
                current.Append('\n');
                continue;
            }

            if (!inString && c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
            {
                // Multi-line comment: skip to */
                current.Append("/*");
                i++;
                while (i < sql.Length - 1 && !(sql[i] == '*' && sql[i + 1] == '/'))
                {
                    current.Append(sql[i]);
                    i++;
                }
                if (i < sql.Length)
                {
                    current.Append("*");
                    i++;
                }
                current.Append("/");
                continue;
            }

            if (!inString && c == 'G' && i + 1 < sql.Length && char.ToUpperInvariant(sql[i + 1]) == 'O'
                && (i + 2 >= sql.Length || char.IsWhiteSpace(sql[i + 2])))
            {
                batches.Add(current.ToString());
                current.Clear();
                i++; // skip the 'O'
                continue;
            }

            current.Append(c);
        }

        var remainder = current.ToString().Trim();
        if (!string.IsNullOrEmpty(remainder))
            batches.Add(remainder);

        return batches;
    }
}
