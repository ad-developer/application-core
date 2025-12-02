namespace ApplicationCore.Rules.Abstractions;

public interface IValidationRule
{
    // If onValidationComplete(false) is called, the pipeline STOPS.
    Task ExecuteAsync(IRulePipeline rulePipeline, Dictionary<string, object>? values = null, Action<bool>? onValidationComplete = null, CancellationToken ct = default);
}
