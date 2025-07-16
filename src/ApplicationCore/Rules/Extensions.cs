using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public static class Extensions
{
    public static IRulePipeline SetFlowObject<T>(this IRulePipeline rulePipeline, T flowObject)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(flowObject, nameof(flowObject));

        rulePipeline.Logger.LogInformation($"Setting flow object of type {flowObject.GetType().Name} with InstanceId {rulePipeline.InstanceId}");

        rulePipeline.FlowObject = flowObject;

        return rulePipeline;
    }

    public static IRulePipeline ExecuteRule<T>(this IRulePipeline rulePipeline, Dictionary<string, object>? values = null)
    {
        var rule = rulePipeline.RetrieveRule(typeof(T));
        if (rule is not null)
        {
            try
            {
                rulePipeline.Logger.LogInformation($"Executing rule of type {typeof(T).Name} with InstanceId {rulePipeline.InstanceId}");
                rule.Execute(rulePipeline, values);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing rule of type {typeof(T).Name}", ex);
            }
        }

        return rulePipeline;
    }
    
    public static IRulePipeline ExecuteValidationRule<T>(this IRulePipeline rulePipeline, Dictionary<string, object>? values = null, Action<bool>? onValidationComplete = null)
    {
        var rule = rulePipeline.RetrieveValidationRule(typeof(T));
        if (rule is not null)
        {
            try
            {
                rulePipeline.Logger.LogInformation($"Executing validation rule of type {typeof(T).Name} with InstanceId {rulePipeline.InstanceId}");
                rule.Execute(rulePipeline, values, onValidationComplete);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing validation rule of type {typeof(T).Name}", ex);
            }
        }

        return rulePipeline;
    }
}
