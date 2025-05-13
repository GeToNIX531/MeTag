using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class SqlStorage : IStorage
{
    private readonly string _connectionString;
    private const string TableName = "AnalysisHistory";

    public SqlStorage(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabaseAsync().Wait();
    }

    private async Task InitializeDatabaseAsync()
    {
        var createTableQuery = $@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{TableName}')
            CREATE TABLE {TableName} (
                Id INT PRIMARY KEY IDENTITY,
                AnalysisDate DATETIME2 NOT NULL,
                Url NVARCHAR(500) NOT NULL,
                JsonData NVARCHAR(MAX) NOT NULL,
                INDEX IX_Url NONCLUSTERED (Url),
                INDEX IX_Date NONCLUSTERED (AnalysisDate)
            )";

        await ExecuteNonQueryAsync(createTableQuery);
    }

    public async Task SaveAsync<T>(IEnumerable<T> items)
    {
        var insertQuery = $@"
            INSERT INTO {TableName} (AnalysisDate, Url, JsonData)
            VALUES (@Date, @Url, @Json)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            foreach (var item in items)
            {
                using (var command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Date", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Url", (item as AnalysisResult)?.Url ?? "");
                    command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(item));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }

    public async Task<List<T>> LoadAsync<T>()
    {
        var results = new List<T>();
        var query = $"SELECT JsonData FROM {TableName} ORDER BY AnalysisDate DESC";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {

                    while (await reader.ReadAsync())
                    {
                        var json = reader.GetString(0);
                        results.Add(JsonSerializer.Deserialize<T>(json));
                    }

                    return results;
                }
            }
        }
    }

    public async Task ClearAsync()
    {
        var query = $"DELETE FROM {TableName}";
        await ExecuteNonQueryAsync(query);
    }

    private async Task ExecuteNonQueryAsync(string query)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
                await command.ExecuteNonQueryAsync();
        }
    }

    //var filtered = await storage.LoadFilteredAsync<AnalysisResult>("WHERE Url LIKE '%example.com%'");
    public async Task<List<T>> LoadFilteredAsync<T>(string whereClause = "", int? limit = null)
    {
        var results = new List<T>();
        var query = new StringBuilder($"SELECT TOP {limit ?? 1000} JsonData FROM {TableName}");

        if (!string.IsNullOrEmpty(whereClause))
        {
            query.Append($" {whereClause}");
        }

        query.Append(" ORDER BY AnalysisDate DESC");

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query.ToString(), connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {

                    while (await reader.ReadAsync())
                    {
                        var json = reader.GetString(0);
                        results.Add(JsonSerializer.Deserialize<T>(json));
                    }

                    return results;
                }
            }
        }
    }
}