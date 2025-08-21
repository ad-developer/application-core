using System.Reflection;

namespace ApplicationCore.ExceptionHandlers;

public class WebApplicationExceptionHandlerConfiguration
{
    internal List<Assembly> AssembliesToRegister { get; } = new();
   
    public WebApplicationExceptionHandlerConfiguration RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssemblyContaining(typeof(T));

    public WebApplicationExceptionHandlerConfiguration RegisterServicesFromAssemblyContaining(Type type)
        => RegisterServicesFromAssembly(type.Assembly);
    
    public WebApplicationExceptionHandlerConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);

        return this;
    }

    public WebApplicationExceptionHandlerConfiguration RegisterHandlersFromAssembly()
    {
        var exceptionDetailProcessor = typeof(IExceptionDetailProcessor);
        var exceptionDetailProcessors = AssembliesToRegister
                .SelectMany(a => a.DefinedTypes)
                .Where(p => exceptionDetailProcessor.IsAssignableFrom(p))
                .ToList();
        
        exceptionDetailProcessors.ForEach(type =>{
            ExceptionDetailProcessors.Items.Add(type);
        });


        var exceptionBotificationTarget = typeof(IExceptionNotificationTarget);
        var exceptionBotificationTargets = AssembliesToRegister
                .SelectMany(a => a.DefinedTypes)
                .Where(p => exceptionBotificationTarget.IsAssignableFrom(p))
                .ToList();
        
        exceptionBotificationTargets.ForEach(type =>{
            ExceptionNotificationTargets.Items.Add(type);
        });

        return this;
    }
}
