using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules.Abstractions;

public interface IRulePipeline
{
    ILogger Logger { get; }
    Guid InstanceId { get; }
    IServiceProvider Services { get; }
    object? FlowObject { get; set; }
    Dictionary<string, object> FlowObjects { get; } // Shared state for IRules
    IRule? RetrieveRule(Type ruleType);
    IValidationRule? RetrieveValidationRule(Type ruleType);
}
