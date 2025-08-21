
namespace ApplicationCore.ExceptionHandlers;

public class WebApplicationExceptionHandler : IWebApplicationExceptionHandler
{
    public Dictionary<string, object> ExceptionDetails { get; set; } = new Dictionary<string, object>();
    private readonly IServiceProvider _serviceProvider;

    public WebApplicationExceptionHandler(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }
    public async Task HandleExceptionAsync(HttpContent httpContent, string environmentName, CancellationToken cancellationToken)
    {
        try
        {
            await ProcessAllExeptionDetailsAsync(httpContent, cancellationToken);
            await NotifyAllTargetsAsync(environmentName, cancellationToken);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing the exception: {ex.Message}");
        }
    }

    internal async Task ProcessAllExeptionDetailsAsync(HttpContent httpContent, CancellationToken cancellationToken = default)
    {
        foreach (var processorType in ExceptionDetailProcessors.Items)
        {
            if (processorType is Type type && typeof(IExceptionDetailProcessor).IsAssignableFrom(type))
            {
                var processor = Activator.CreateInstance(type) as IExceptionDetailProcessor;
                if (processor != null)
                {
                    processor.ServiceProvider = _serviceProvider;
                    await processor.ProcessExceptionDetailsAsync(this, httpContent, cancellationToken);
                }
            }
        }
    }

    internal async Task NotifyAllTargetsAsync(string environmentName, CancellationToken cancellationToken = default)
    {
        foreach (var targetType in ExceptionNotificationTargets.Items)
        {
            if (targetType is Type type && typeof(IExceptionNotificationTarget).IsAssignableFrom(type))
            {
                var target = Activator.CreateInstance(type) as IExceptionNotificationTarget;
                if (target != null)
                {
                    target.ServiceProvider = _serviceProvider;
                    await target.NotifyAsync(this, environmentName, cancellationToken);
                }
            }
        }
    }
}