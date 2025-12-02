using System.Reflection.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace ApplicationCore.Caching;

public class CacheService : ICacheService
{
    public CancellationTokenSource ResetCacheToken { get; set; }

    private readonly ILogger<CacheService> _logger;
    private readonly IMemoryCache _memoryCache;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public CacheService(ILogger<CacheService> logger, IMemoryCache memoryCache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

        ResetCacheToken = new CancellationTokenSource();
        _logger.LogInformation("CacheService initialized.");
    }

    public void AddObject<T>(
        string key,
        T value,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        cancellationTokenSource ??= ResetCacheToken;
        var cacheKey = GenerateCacheKey(key, cacheType, userIdentity);

        var options = CreateCacheOptions(expirationType, expirationTime, cancellationTokenSource);
        _memoryCache.Set(cacheKey, value, options);

        _logger.LogInformation("Object cached: {Key}", cacheKey);
    }

    public async Task AddObjectAsync<T>(
        string key,
        T value,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            AddObject(key, value, userIdentity, cacheType, expirationType, expirationTime, cancellationTokenSource);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public T? GetObject<T>(
        string key,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key);

        cancellationTokenSource ??= ResetCacheToken;
        var cacheKey = GenerateCacheKey(key, cacheType, userIdentity);

        if (_memoryCache.TryGetValue(cacheKey, out T? value))
        {
            _logger.LogDebug("Cache hit: {Key}", cacheKey);
            return value;
        }

        _logger.LogDebug("Cache miss: {Key}", cacheKey);
        return default;
    }

    public Task<T?> GetObjectAsync<T>(
        string key,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        CancellationTokenSource? cancellationTokenSource = null)
        => Task.Run(() => GetObject<T>(key, userIdentity, cacheType, cancellationTokenSource));

    public T? GetOrAddObject<T>(
        string key,
        Func<T> valueFactory,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        cancellationTokenSource ??= ResetCacheToken;
        var cacheKey = GenerateCacheKey(key, cacheType, userIdentity);

        if (_memoryCache.TryGetValue(cacheKey, out T? existing))
        {
            _logger.LogDebug("Cache hit: {Key}", cacheKey);
            return existing;
        }

        var value = valueFactory();
        AddObject(cacheKey, value, userIdentity, cacheType, expirationType, expirationTime, cancellationTokenSource);
        return value;
    }

    public async Task<T?> GetOrAddObjectAsync<T>(
        string key,
        Func<Task<T>> valueFactory,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        cancellationTokenSource ??= ResetCacheToken;
        var cacheKey = GenerateCacheKey(key, cacheType, userIdentity);

        if (_memoryCache.TryGetValue(cacheKey, out T? existing))
        {
            _logger.LogDebug("Cache hit: {Key}", cacheKey);
            return existing;
        }

        var value = await valueFactory().ConfigureAwait(false);
        AddObject(cacheKey, value, userIdentity, cacheType, expirationType, expirationTime, cancellationTokenSource);
        return value;
    }

    public void ClearCache(CancellationTokenSource? cancellationTokenSource = null)
    {
        cancellationTokenSource ??= ResetCacheToken;

        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();

        if (cancellationTokenSource == ResetCacheToken)
            ResetCacheToken = new CancellationTokenSource();

        _logger.LogInformation("Cache cleared.");
    }

    private static MemoryCacheEntryOptions CreateCacheOptions(
        ExpirationType expirationType,
        int expirationTime,
        CancellationTokenSource cancellationTokenSource)
    {
        var options = new MemoryCacheEntryOptions()
            .SetPriority(CacheItemPriority.Low)
            .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

        return expirationType switch
        {
            ExpirationType.Sliding => options.SetSlidingExpiration(TimeSpan.FromMinutes(expirationTime)),
            ExpirationType.Absolute => options.SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationTime)),
            _ => options
        };
    }

    public static string GenerateCacheKey(string key, CacheType cacheType, string? userIdentity = null)
    {
        ArgumentNullException.ThrowIfNull(key);

        return cacheType switch
        {
            CacheType.GlobalSession => $"global:{key}",
            CacheType.UserSession when !string.IsNullOrEmpty(userIdentity) => $"{userIdentity}:{key}",
            CacheType.UserSession => $"user:{key}",
            _ => key
        };
    }
}
