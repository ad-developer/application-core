using ApplicationCore.Rules.Abstractions;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

// Handler for declarative JSON rules
public class DeclarativeValidationRule : IValidationRule
{
    private readonly RuleDefinition _def;

    public DeclarativeValidationRule(RuleDefinition def)
    {
        _def = def;
    }

    public Task ExecuteAsync(IRulePipeline pipeline, Dictionary<string, object>? values, Action<bool>? onValidationComplete, CancellationToken ct = default)
    {
        if (pipeline.FlowObject == null) 
        {
            onValidationComplete?.Invoke(false);
            return Task.CompletedTask;
        }

        var prop = pipeline.FlowObject.GetType().GetProperty(_def.TargetProperty ?? "");
        if (prop == null)
        {
            pipeline.Logger.LogError($"Property {_def.TargetProperty} missing");
            onValidationComplete?.Invoke(false);
            return Task.CompletedTask;
        }

        var actual = prop.GetValue(pipeline.FlowObject);
        // Use Convert.ChangeType to handle string-to-int/decimal comparisons from JSON
        var expected = Convert.ChangeType(_def.TargetValue, prop.PropertyType);

        bool success = _def.Operator switch
        {
            "Equals" => object.Equals(actual, expected),
            "NotEquals" => !object.Equals(actual, expected),
            _ => false
        };

        onValidationComplete?.Invoke(success);
        return Task.CompletedTask;
    }
}