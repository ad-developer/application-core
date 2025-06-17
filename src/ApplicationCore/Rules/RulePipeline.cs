
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public class RulePipeline : IRulePipeline
{
    public object? FlowObject { get; set; }

    public Dictionary<string, object> FlowObjects { get; } = new Dictionary<string, object>();

   public ILogger Logger { get; }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IServiceProvider Services {get; }

    public RulePipeline(ILogger<RulePipeline> logger, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        Logger = logger;
        Services = services;
    }

    public IRule? RetrieveRule(Type ruleType)
    {
        Logger.LogInformation($"Retrieving rule of type {ruleType.FullName}");
        var rule = Services.GetService(ruleType) as IRule;
        return rule;
    }
}