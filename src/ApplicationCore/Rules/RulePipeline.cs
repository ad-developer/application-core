
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public class RulePipeline : IRulePipeline
{
    public object? FlowObject { get; set; }

    public Dictionary<string, object> FlowObjects { get; } = new Dictionary<string, object>();

    private readonly ILogger<RulePipeline> _logger;
    IServiceProvider _services;
    public RulePipeline(ILogger<RulePipeline> logger, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        
        _logger = logger;
        _services = services;
    }

    public IRule? RetrieveRule(Type ruleType)
    {
        _logger.LogInformation($"Retrieving rule of type {ruleType.FullName}");
        var rule = _services.GetService(ruleType) as IRule;
        return rule;
    }
}