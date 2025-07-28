using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public static class Extensions
{
    public static void LogInformation(this ITrackingLogger trackingLogger, string message, Guid? instanceId = null)
    {
        message = BuildLogMessage(trackingLogger, message, instanceId);
        trackingLogger.Logger.LogInformation(message);
    }
    
    public static void LogInformation(this ITrackingLogger trackingLogger, string message, EventId eventId, Guid? instanceId = null)
    {
        message = BuildLogMessage(trackingLogger, message, instanceId);
        trackingLogger.Logger.LogInformation(eventId, message);
    }
    
    public static void LogWarning(this ITrackingLogger trackingLogger, string message, Guid? instanceId = null)
    {
        message = BuildLogMessage(trackingLogger, message, instanceId);
        trackingLogger.Logger.LogWarning(message);
    }
    
    public static void LogWarning(this ITrackingLogger trackingLogger, string message, EventId eventId, Guid? instanceId = null)
    {
        message = BuildLogMessage(trackingLogger, message, instanceId);
        trackingLogger.Logger.LogWarning(eventId, message);
    }
    
    public static void LogError(this ITrackingLogger trackingLogger,Exception exception, string message, Guid? instanceId = null)
    {
        message = BuildLogMessage(trackingLogger, message, instanceId);
        trackingLogger.Logger.LogError(exception, message);
    }
   
    public static void LogError(this ITrackingLogger trackingLogger, Exception exception, string message, EventId eventId, Guid? instanceId = null)
    {
        message = BuildLogMessage(trackingLogger, message, instanceId);
        trackingLogger.Logger.LogError(eventId, exception, message);
    }

    internal static string BuildLogMessage(ITrackingLogger logger, string message, Guid? instanceId = null)
    {
        var instanceIdMessage = instanceId.HasValue ? $" InstanceId: {instanceId.Value}" : string.Empty;
        var byName = logger.LoggerIdentity?.Name ?? "Unknown";
        var byLoggerId = logger.LoggerIdentity?.LoggerId ?? "Unknown";

        var finalMessage = $"{message}. TrackingId: {logger.TrackingId}{instanceIdMessage}  By: {byName} with logger id ({byLoggerId}) at {DateTime.UtcNow}";

        return finalMessage;
    }
}