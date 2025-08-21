namespace ApplicationCore.ExceptionHandlers;

public interface IExceptionDetailProcessor
{
    public IServiceProvider ServiceProvider { get; set; }
    Task ProcessExceptionDetailsAsync(IWebApplicationExceptionHandler exceptionHandler, HttpContent httpContent, CancellationToken cancellationToken);
}
