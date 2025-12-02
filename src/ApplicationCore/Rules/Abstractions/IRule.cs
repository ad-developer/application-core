namespace ApplicationCore.Rules.Abstractions;

public interface IRule
{
    // Execute logic, modify FlowObject, or add to FlowObjects dictionary
    Task ExecuteAsync(IRulePipeline rulePipeline, Dictionary<string, object>? values = null, CancellationToken ct = default);
}
