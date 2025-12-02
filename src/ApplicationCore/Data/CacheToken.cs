using ApplicationCore.Caching;

namespace ApplicationCore.Data;

public sealed class CacheToken
{
    /// <summary>
    /// Indicates whether data should be cached.
    /// </summary>
    public bool CacheData { get; init; } = false;

    /// <summary>
    /// Type of cache to use (e.g., Memory, Distributed, Session, etc.).
    /// </summary>
    public CacheType CacheType { get; init; } = CacheType.UserSession;

    /// <summary>
    /// Defines how the cache expiration behaves (e.g., Sliding or Absolute).
    /// </summary>
    public ExpirationType ExpirationType { get; init; } = ExpirationType.Sliding;

    /// <summary>
    /// Expiration time (in minutes) for cached data.
    /// </summary>
    public int ExpirationTime { get; init; } = 15;

    /// <summary>
    /// Optional user identity to associate cache entries with a specific user.
    /// </summary>
    public string? UserIdentity { get; init; }

    /// <summary>
    /// Optional cancellation token source that can invalidate this cache scope.
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource { get; init; }

    /// <summary>
    /// Creates a default cache token with caching disabled.
    /// </summary>
    public static CacheToken None => new() { CacheData = false };

    /// <summary>
    /// Creates a fully configured cache token with caching enabled.
    /// </summary>
    public static CacheToken Create(
        bool cacheData = true,
        CacheType cacheType = CacheType.UserSession,
        ExpirationType expirationType = ExpirationType.Sliding,
        int expirationTime = 15,
        string? userIdentity = null,
        CancellationTokenSource? cancellationTokenSource = null)
    {
        return new CacheToken
        {
            CacheData = cacheData,
            CacheType = cacheType,
            ExpirationType = expirationType,
            ExpirationTime = expirationTime,
            UserIdentity = userIdentity,
            CancellationTokenSource = cancellationTokenSource
        };
    }
}

