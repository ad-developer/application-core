using ApplicationCore.Logging;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public interface IRulePipeline
{
    ITrackingLogger TrackingLogger { get; }
    Guid InstanceId { get; }
    IServiceProvider Services { get; }
    object? FlowObject { get; set; }
    Dictionary<string, object> FlowObjects { get; }
    IRule? RetrieveRule(Type ruleType);
    IValidationRule? RetrieveValidationRule(Type ruleType);
}
