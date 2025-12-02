namespace ApplicationCore.Caching;

public interface ICacheService
{
    CancellationTokenSource ResetCacheToken { get; set; }

    void AddObject<T>(
        string key,
        T value,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null);

    Task AddObjectAsync<T>(
        string key,
        T value,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null);

    T? GetObject<T>(
        string key,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        CancellationTokenSource? cancellationTokenSource = null);

    Task<T?> GetObjectAsync<T>(
        string key,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        CancellationTokenSource? cancellationTokenSource = null);

    T? GetOrAddObject<T>(
        string key,
        Func<T> valueFactory,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null);

    Task<T?> GetOrAddObjectAsync<T>(
        string key,
        Func<Task<T>> valueFactory,
        string? userIdentity = null,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        CancellationTokenSource? cancellationTokenSource = null);

    void ClearCache(CancellationTokenSource? cancellationTokenSource = null);
}