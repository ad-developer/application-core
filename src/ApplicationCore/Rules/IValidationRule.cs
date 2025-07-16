namespace ApplicationCore.Rules;

public interface IValidationRule
{
    void Execute(IRulePipeline rulePipeline, Dictionary<string, object>? values = null, Action<bool>? onValidationComplete = null);
}
