namespace ApplicationCore.ExceptionHandlers;

public interface IExceptionNotificationTarget
{
    public IServiceProvider ServiceProvider { get; set; }
    Task NotifyAsync(IWebApplicationExceptionHandler webApplicationExceptionHandler, string environmentName, CancellationToken cancellationToken);
}
