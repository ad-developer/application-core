namespace ApplicationCore.Rules;

public interface IValidationRule
{
    Task ExecuteAsync(IRulePipeline rulePipeline, Dictionary<string, object>? values = null, Action<bool>? onValidationComplete = null);
}
