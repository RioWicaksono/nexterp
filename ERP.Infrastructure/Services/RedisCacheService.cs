using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Redis-backed caching service with distributed cache support
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IDatabase _db;
    private const string KeyPrefix = "nexterp:cache:";

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var fullKey = $"{KeyPrefix}{key}";
            var value = await _db.StringGetAsync(fullKey);

            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache get failed for key {Key}", key);
            return null;
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key {Key}", key);
            return cached;
        }

        // Cache miss - call factory
        _logger.LogDebug("Cache miss for key {Key}, calling factory", key);
        var value = await factory();

        if (value != null)
        {
            await SetAsync(key, value, expiry, cancellationToken);
        }

        return value;
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var fullKey = $"{KeyPrefix}{key}";
            var serialized = JsonSerializer.Serialize(value);

            await _db.StringSetAsync(fullKey, serialized, expiry ?? TimeSpan.FromMinutes(30));
            _logger.LogDebug("Cache set for key {Key}, expiry: {Expiry}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache set failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = $"{KeyPrefix}{key}";
            await _db.KeyDeleteAsync(fullKey);
            _logger.LogDebug("Cache removed for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache remove failed for key {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPattern = $"{KeyPrefix}{pattern}";
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            var keys = server.Keys(pattern: fullPattern).ToArray();
            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys);
                _logger.LogDebug("Cache removed {Count} keys matching pattern {Pattern}", keys.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache remove by pattern failed for {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = $"{KeyPrefix}{key}";
            return await _db.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache exists check failed for key {Key}", key);
            return false;
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<T> factory,
        TimeSpan? expiry = null) where T : class
    {
        var result = await GetOrSetAsync<T>(key, () => Task.FromResult(factory()), expiry);
        return result;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        await SetAsync(key, value, expiry, CancellationToken.None);
    }
}

/// <summary>
/// Interface for caching service
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task<T> GetOrSetAsync<T>(string key, Func<T> factory, TimeSpan? expiry = null) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
