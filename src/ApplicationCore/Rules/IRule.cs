namespace ApplicationCore.Rules;

public interface IRule
{
    Task ExecuteAsync(IRulePipeline rulePipeline, Dictionary<string, object>? values = null);
}
