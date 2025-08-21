namespace ApplicationCore.ExceptionHandlers;

public interface IWebApplicationExceptionHandler
{
    public Dictionary<string, object> ExceptionDetails { get; set; }
    Task HandleExceptionAsync(HttpContent httpContent, string environmentName, CancellationToken cancellationToken);
}
