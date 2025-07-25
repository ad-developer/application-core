using ApplicationCore.Caching;

namespace ApplicationCore.DataPersistence;

public class CacheToken
{
    public bool CacheData { get; set; } = false;
    public CacheType CacheType { get; set; } = CacheType.UserSession;
    public ExpirationType ExpirationType { get; set; } = ExpirationType.Sliding;
    public int ExpirationTime { get; set; } = 15;
}
