using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Logging;

public class LoggingProxy<T> : DispatchProxy
{
    private T _decorated = default!;
    private ILogger _logger = default!;
    public static T Create(T decorated, ILogger logger)
    {
        if (decorated == null) throw new ArgumentNullException(nameof(decorated));
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        var proxy = DispatchProxy.Create<T, LoggingProxy<T>>();
        var loggingProxy = (LoggingProxy<T>)(object)proxy;
        loggingProxy._decorated = decorated;
        loggingProxy._logger = logger;
        return proxy;
    }
    
    protected override object? Invoke(MethodInfo targetMethod, object?[]? args)
    {
        var scopes = new List<IDisposable>();

        try
        {
            // 1. Read [LogScope] attributes
            var logScopes = targetMethod.GetCustomAttributes<LogScopeAttribute>();

            // 2. Create a scope for each attribute
            foreach (var s in logScopes)
            {
                // Value: if null, treat as auto-generated or method argument reference
                var value = s.Value ?? $"[{s.Name}]";

                var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    [s.Name] = value
                });

                scopes.Add(scope);
            }

            // 3. Invoke method (sync or async)
            var result = targetMethod.Invoke(_decorated, args);

            // 4. Unwrap async
            if (result is Task task)
            {
                return AwaitAsync(task, scopes);
            }

            return result;
        }
        catch
        {
            DisposeScopes(scopes);
            throw;
        }
    }

    private async Task AwaitAsync(Task task, List<IDisposable> scopes)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        finally
        {
            DisposeScopes(scopes);
        }
    }

    private async Task<TOut> AwaitAsync<TOut>(Task<TOut> task, List<IDisposable> scopes)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        finally
        {
            DisposeScopes(scopes);
        }
    }

    private void DisposeScopes(List<IDisposable> scopes)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
            scopes[i].Dispose();
    }
}
