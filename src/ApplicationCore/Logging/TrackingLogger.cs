using ApplicationCore.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public class TrackingLogger<T> : ITrackingLogger<T>
{
    public ILogger Logger { get; }
    public Guid TrackingId { get; set; } = Guid.NewGuid();
    public LoggerIdentity LoggerIdentity { get; set; }
    public TrackingLogger(ILogger<T> logger, ILoggerIdentityService loggerIdentityService, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(loggerIdentityService, nameof(loggerIdentityService));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        Logger = logger;
        LoggerIdentity = loggerIdentityService.GetLoggerIdentity();

        TryCacheTrackingId(serviceProvider);
    }

    internal void TryCacheTrackingId(IServiceProvider serviceProvider)
    {
        var memoryCache = serviceProvider.GetService<IMemoryCache>();
        if (memoryCache is not null)
        {
            var cacheService = new CacheService(memoryCache);
            cacheService.TrackingLogger = this;
            cacheService.AddObject($"TrackingId", TrackingId, CacheType.UserSession);
        }
    }
}

public static class TrackingLogger
{
    public static Guid? GetLastTrackingId(IServiceProvider serviceProvider)
    {
        Guid? trackingId = null;
        var memoryCache = serviceProvider.GetService<IMemoryCache>();
        var loggerIdentityService = serviceProvider.GetService<ILoggerIdentityService>();

        if (memoryCache is not null)
        {
            var cacheService = new CacheService(memoryCache);
            var loggerIdentity = loggerIdentityService?.GetLoggerIdentity();

            if (loggerIdentity is not null)
                trackingId = cacheService.GetObject<Guid?>($"{loggerIdentity.LoggerId}TrackingId");
        }

        return trackingId;
    }
}