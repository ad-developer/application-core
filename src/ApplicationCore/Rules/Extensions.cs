namespace ApplicationCore.Rules;

public static class Extensions
{
    public static IRulePipeline SetFlowObject<T>(this IRulePipeline rulePipeline, T flowObject)
    {
        ArgumentNullException.ThrowIfNull(rulePipeline, nameof(rulePipeline));
        ArgumentNullException.ThrowIfNull(flowObject, nameof(flowObject));

        rulePipeline.FlowObject = flowObject;

        return rulePipeline;
    }
    
    public static IRulePipeline ExecuteRule<T>(this IRulePipeline rulePipeline, object values)
    {
        var rule = rulePipeline.RetrieveRule(typeof(T));
        if (rule is not null)
        {
            try
            {
                rule.Execute(rulePipeline, values);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing rule of type {typeof(T).Name}", ex);
            }
        }

        return rulePipeline;
    }
}
