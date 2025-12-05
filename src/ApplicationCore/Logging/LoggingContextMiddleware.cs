using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public class LoggingContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingContextMiddleware> _logger;

    public LoggingContextMiddleware(RequestDelegate next, ILogger<LoggingContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // You can customize these however you obtain them
        string trackingId   = context.TraceIdentifier;
        string userLanId    = context.User?.Identity?.Name ?? "Anonymous";
        string userFullName = context.Items["UserFullName"]?.ToString() ?? "Unknown";

        using (_logger.BeginTrackingIdScope(trackingId))
        using (_logger.BeginUserLanIdScope(userLanId))
        using (_logger.BeginUserFullNameScope(userFullName))
        {
            await _next(context);
        }
    }
}
