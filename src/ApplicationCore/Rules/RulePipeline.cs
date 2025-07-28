
using ApplicationCore.Logging;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public class RulePipeline : IRulePipeline
{
    public object? FlowObject { get; set; }

    public Dictionary<string, object> FlowObjects { get; } = new Dictionary<string, object>();

    public ITrackingLogger TrackingLogger { get; }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IServiceProvider Services { get; }

    public RulePipeline(ITrackingLogger<RulePipeline> trackingLogger, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(trackingLogger, nameof(trackingLogger));
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        TrackingLogger = trackingLogger;
        Services = services;
    }

    public IRule? RetrieveRule(Type ruleType)
    {
        TrackingLogger.LogInformation($"Retrieving rule of type {ruleType.FullName}");

        var rule = Services.GetService(ruleType) as IRule;
        return rule;
    }

    public IValidationRule? RetrieveValidationRule(Type ruleType)
    {
        TrackingLogger.LogInformation($"Retrieving validation rule of type {ruleType.FullName}");

        var rule = Services.GetService(ruleType) as IValidationRule;
        return rule;
    }
}