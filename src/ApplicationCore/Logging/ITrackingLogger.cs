using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public interface ITrackingLogger
{
    ILogger Logger { get; }
    Guid TrackingId { get; set; }
    LoggerIdentity LoggerIdentity { get; set; }
}