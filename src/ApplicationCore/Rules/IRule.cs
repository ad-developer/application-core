using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public interface IRule
{
    ILogger Logger { get; } 
    void Execute(IRulePipeline rulePipeline, object values);
}
