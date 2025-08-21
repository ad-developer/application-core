using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.ExceptionHandlers;

public static class Extensions
{
    public static IServiceCollection AddExceptionHandler(this IServiceCollection services,
        Action<WebApplicationExceptionHandlerConfiguration> configuration)
    {
        var serviceConfig = new WebApplicationExceptionHandlerConfiguration();

        configuration.Invoke(serviceConfig);

        return services.AddExceptionHandler(serviceConfig);
    }
    

     public static IServiceCollection AddExceptionHandler(this IServiceCollection services, 
        WebApplicationExceptionHandlerConfiguration configuration)
    {   
        if (!configuration.AssembliesToRegister.Any())
            throw new ArgumentException("No assemblies found to register any exception detail processors or exception notification targets. Supply at least one assembly.");
        
        services.AddScoped<IWebApplicationExceptionHandler, WebApplicationExceptionHandler>();
        
        configuration.RegisterHandlersFromAssembly();

        return services;
    }
}
