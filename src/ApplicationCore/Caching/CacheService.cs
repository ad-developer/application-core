using ApplicationCore.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace ApplicationCore.Caching;

public class CacheService : ICacheService
{
    public CancellationTokenSource ResetCacheToken { get; set; }
    public ITrackingLogger? TrackingLogger { get; set; }

    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IMemoryCache _memoryCache;

    public CacheService(ITrackingLogger<CacheService> trackingLogger, IMemoryCache memoryCache)
    {
        ArgumentNullException.ThrowIfNull(trackingLogger, nameof(trackingLogger));
        ArgumentNullException.ThrowIfNull(memoryCache, nameof(memoryCache));

        _memoryCache = memoryCache;
        TrackingLogger = trackingLogger;
        TrackingLogger?.LogInformation($"{GetType().Name} initialized.");

        ResetCacheToken = new CancellationTokenSource();
    }

    public CacheService(IMemoryCache memoryCache)
    {
        ArgumentNullException.ThrowIfNull(memoryCache, nameof(memoryCache));
        
        _memoryCache = memoryCache;
        ResetCacheToken = new CancellationTokenSource();
    }

    public void AddObject<T>(string key, T value, CacheType cacheType = CacheType.UserSession, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        cancellationTokenSource ??= ResetCacheToken;

        key = GenerateCacheKey(key, cacheType, TrackingLogger);

        MemoryCacheEntryOptions? options = null;

        if (expirationType == ExpirationType.Sliding)
        {
            options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(expirationTime))
            .SetPriority(CacheItemPriority.Low)
            .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));
        }

        if (expirationType == ExpirationType.Absolute)
        {
            options = new MemoryCacheEntryOptions()
          .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationTime))
          .SetPriority(CacheItemPriority.Low)
          .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));
        }

        if (options is not null)
            _memoryCache.Set(key, value, options);
    }

    public async Task AddObjectAsync<T>(string key, T value, CacheType cacheType = CacheType.UserSession, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            cancellationTokenSource ??= ResetCacheToken;

            key = GenerateCacheKey(key, cacheType, TrackingLogger);

            MemoryCacheEntryOptions? options = null;

            if (expirationType == ExpirationType.Sliding)
            {
                options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(expirationTime))
                .SetPriority(CacheItemPriority.Low)
                .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));
            }

            if (expirationType == ExpirationType.Absolute)
            {
                options = new MemoryCacheEntryOptions()
              .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationTime))
              .SetPriority(CacheItemPriority.Low)
              .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));
            }

            if (options is not null)
                _memoryCache.Set(key, value, options);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public T? GetObject<T>(string key, CacheType cacheType = CacheType.UserSession, CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        cancellationTokenSource ??= ResetCacheToken;

        key = GenerateCacheKey(key, cacheType, TrackingLogger);

        if (_memoryCache.TryGetValue(key, out T? value))
            return value;
       
        TrackingLogger?.LogInformation($"Cache miss for key: {key}");
        return default!;
    }
   
    public Task<T?> GetObjectAsync<T>(string key, CacheType cacheType, CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        return Task.Run(() => GetObject<T>(key, cacheType, cancellationTokenSource));     
    }

    public T? GetOrAddObject<T>(string key, Func<T> valueFactory, CacheType cacheType, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));

        cancellationTokenSource ??= ResetCacheToken;

        key = GenerateCacheKey(key, cacheType, TrackingLogger);

        if (_memoryCache.TryGetValue(key, out T? value))
        {
            TrackingLogger?.LogInformation($"Cache hit for key: {key}");
            return value;
        }

        value = valueFactory();
        AddObject(key, value, cacheType, expirationType, expirationTime, cancellationTokenSource);

        return value;
    }

    public Task<T?> GetOrAddObjectAsync<T>(string key, Func<Task<T>> valueFactory, CacheType cacheType, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));

        return Task.Run(async () =>
        {
            cancellationTokenSource ??= ResetCacheToken;

            key = GenerateCacheKey(key, cacheType, TrackingLogger);

            if (_memoryCache.TryGetValue(key, out T? value))
            {
                TrackingLogger?.LogInformation($"Cache hit for key: {key}");
                return value;
            }

            value = await valueFactory().ConfigureAwait(false);
            AddObject(key, value, cacheType, expirationType, expirationTime, cancellationTokenSource);
            return value;
        });
    }

    public void ClearCache(CancellationTokenSource cancellationTokenSource)
    {
        if (cancellationTokenSource is null)
            cancellationTokenSource = ResetCacheToken;

        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();

        if (cancellationTokenSource == ResetCacheToken)
            ResetCacheToken = new CancellationTokenSource();

        TrackingLogger?.LogInformation("Cache clered.");
    }
    
    public static string GenerateCacheKey(string key, CacheType cacheType, ITrackingLogger? trackingLogger)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        // trackingLogger can be null now

        if (cacheType == CacheType.UserSession && trackingLogger != null)
            key = $"{trackingLogger.LoggerIdentity.LoggerId}:{key}";

        if (cacheType == CacheType.GlobalSession)
            key = $"global:{key}";
       
        trackingLogger?.LogInformation($"Generated cache key: {key}");

        return key;
    }

}