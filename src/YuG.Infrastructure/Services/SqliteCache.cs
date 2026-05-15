using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using YuG.Domain.Common.Interfaces;

namespace YuG.Infrastructure.Services;

/// <summary>
/// SQLite 缓存配置
/// </summary>
public class SqliteCacheOptions
{
    /// <summary>
    /// 缓存数据库文件路径（默认 "cache.db"）
    /// </summary>
    public string FilePath { get; set; } = "cache.db";
}

/// <summary>
/// SQLite 缓存实现
/// </summary>
public class SqliteCache : ICache, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// 初始化 SQLite 缓存
    /// </summary>
    /// <param name="options">缓存配置</param>
    public SqliteCache(IOptions<SqliteCacheOptions> options)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = options.Value.FilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS CacheEntry (
                Key       TEXT PRIMARY KEY NOT NULL,
                Value     TEXT NOT NULL,
                ExpiresAt TEXT
            )
            """;
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await CleanupExpiredAsync(cancellationToken);

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT Value FROM CacheEntry WHERE Key = @Key";
            cmd.Parameters.AddWithValue("@Key", key);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result is null || result is DBNull)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>((string)result);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO CacheEntry (Key, Value, ExpiresAt)
                VALUES (@Key, @Value, @ExpiresAt)
                ON CONFLICT(Key) DO UPDATE SET
                    Value = @Value,
                    ExpiresAt = @ExpiresAt
                """;

            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Value", JsonSerializer.Serialize(value));
            cmd.Parameters.AddWithValue("@ExpiresAt", (object?)expiration is not null
                ? DateTime.UtcNow.Add(expiration.Value).ToString("O")
                : DBNull.Value);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM CacheEntry WHERE Key = @Key";
            cmd.Parameters.AddWithValue("@Key", key);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await CleanupExpiredAsync(cancellationToken);

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM CacheEntry WHERE Key = @Key AND (ExpiresAt IS NULL OR ExpiresAt > @Now)";
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow.ToString("O"));

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result is not null && result is not DBNull;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 清理过期缓存
    /// </summary>
    private async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM CacheEntry WHERE ExpiresAt IS NOT NULL AND ExpiresAt <= @Now";
        cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _lock?.Dispose();
    }
}
