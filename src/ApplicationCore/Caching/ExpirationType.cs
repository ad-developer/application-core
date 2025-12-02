namespace ApplicationCore.Caching;

/// <summary>
/// Defines how cache entry expiration is handled.
/// </summary>
public enum ExpirationType
{
    /// <summary>
    /// Sliding expiration — resets the expiration timer each time the cache entry is accessed.
    /// </summary>
    Sliding = 0,

    /// <summary>
    /// Absolute expiration — cache entry expires after a fixed duration, regardless of access.
    /// </summary>
    Absolute = 1
}