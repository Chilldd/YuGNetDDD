using System.Collections.Concurrent;
using System.Text.Json;
using YuG.Common.Interfaces;

namespace YuG.AI.Gateway.Services;

/// <summary>
/// 内存缓存实现（用于 AI.Gateway 轻量级缓存场景）
/// </summary>
public class InMemoryCache : ICache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt is null || entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Value));
            }

            _cache.TryRemove(key, out _);
        }

        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        _cache[key] = new CacheEntry(json, expiration is not null ? DateTime.UtcNow.Add(expiration.Value) : null);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt is null || entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }

            _cache.TryRemove(key, out _);
        }

        return Task.FromResult(false);
    }

    private sealed record CacheEntry(string Value, DateTime? ExpiresAt);
}
