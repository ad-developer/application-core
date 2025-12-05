using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public static class LoggerExtensions
{
    public static IDisposable BeginTrackingIdScope(this ILogger logger, string trackingId)
        => logger.BeginScope(new Dictionary<string, object> { ["TrackingId"] = trackingId });

    public static IDisposable BeginUserLanIdScope(this ILogger logger, string lanId)
        => logger.BeginScope(new Dictionary<string, object> { ["UserLanId"] = lanId });

    public static IDisposable BeginUserFullNameScope(this ILogger logger, string fullName)
        => logger.BeginScope(new Dictionary<string, object> { ["UserFullName"] = fullName });
}
