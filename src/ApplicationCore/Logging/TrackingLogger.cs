using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public class TrackingLogger<T> : ITrackingLogger
{
    public ILogger Logger { get; }
    public Guid TrackingId { get; set; } = Guid.NewGuid();
    public LoggerIdentity LoggerIdentity { get; set; }
    public TrackingLogger(ILogger<T> logger, ILoggerIdentityService loggerIdentityService)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(loggerIdentityService, nameof(loggerIdentityService));

        Logger = logger;
        LoggerIdentity = loggerIdentityService.GetLoggerIdentity();
    }     
}