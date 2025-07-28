using ApplicationCore.Logging;

namespace ApplicationCore.Caching;

public interface ICacheService : ITrackable
{
    CancellationTokenSource ResetCacheToken { get; set; }
   
    void ClearCache(CancellationTokenSource cancellationTokenSource);

    void AddObject<T>(string key, T value, CacheType cacheType, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null);

    Task AddObjectAsync<T>(string key, T value, CacheType cacheType, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null);

    T? GetObject<T>(string key, CacheType cacheType, CancellationTokenSource? cancellationTokenSource = null);

    Task<T?> GetObjectAsync<T>(string key, CacheType cacheType, CancellationTokenSource? cancellationTokenSource = null);

    T? GetOrAddObject<T>(string key, Func<T> valueFactory, CacheType cacheType, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null);

    Task<T?> GetOrAddObjectAsync<T>(string key, Func<Task<T>> valueFactory, CacheType cacheType, ExpirationType expirationType = ExpirationType.Sliding, int expirationTime = 15, CancellationTokenSource? cancellationTokenSource = null);

}
