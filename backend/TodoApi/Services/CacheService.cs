using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace TodoApi.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _defaultExpiration;

    public CacheService(
        IDistributedCache distributedCache,
        ILogger<CacheService> logger,
        IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        var defaultExpirationMinutes = configuration.GetValue<int>("Cache:DefaultExpirationMinutes", 5);
        _defaultExpiration = TimeSpan.FromMinutes(defaultExpirationMinutes);
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
                return null;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving cache key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
            };

            await _distributedCache.SetStringAsync(key, json, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Note: Distributed cache doesn't support pattern matching directly
        // This is a simplified implementation. For production, consider using Redis directly
        // or implementing a more sophisticated key management system
        _logger.LogWarning("Pattern-based cache removal is not fully supported with distributed cache. Pattern: {Pattern}", pattern);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }
}

