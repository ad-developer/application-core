namespace ApplicationCore.Rules.Abstractions;

public interface IWorkflowRepository
{
    // Returns the JSON string for a given workflow name/ID
    Task<string?> GetWorkflowConfigAsync(string workflowName, CancellationToken ct = default);
}
