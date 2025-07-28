using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Logging;

 public static class LoggerServiceCollectionExtensions
    {
        public static IServiceCollection AddTrackingLoggers(this IServiceCollection services) 
        {
            services.AddScoped(typeof(ITrackingLogger<>), typeof(TrackingLogger<>)); 
            
            return services;
        }
    }
