using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public interface IRulePipeline
{
    ILogger Logger { get; }
    Guid InstanceId { get; }
    IServiceProvider Services { get; }
    object? FlowObject { get; set; }
    Dictionary<string, object> FlowObjects { get; }
    IRule? RetrieveRule(Type ruleType);
}
