namespace ApplicationCore.Rules;

public interface IRulePipeline
{
    object? FlowObject { get; set; }
    Dictionary<string, object> FlowObjects { get; }
    IRule? RetrieveRule(Type ruleType);
}
