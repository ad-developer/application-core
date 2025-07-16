namespace ApplicationCore.Rules;

public interface IRule
{
    void Execute(IRulePipeline rulePipeline, Dictionary<string, object>? values = null);
}
