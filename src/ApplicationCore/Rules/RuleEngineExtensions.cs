using System.Text.Json;
using ApplicationCore.Rules.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

public static class RuleEngineExtensions
{
    public static async Task<RuleEngineResult> ExecutePolicyAsync(
        this IRulePipeline pipeline,
        string jsonPolicy,
        CancellationToken ct = default)
    {
        var result = new RuleEngineResult();
        var workflow = JsonSerializer.Deserialize<WorkflowConfiguration>(jsonPolicy);

        if (workflow == null) return result;

        pipeline.Logger.LogInformation($"--- Starting Scope: {workflow.WorkflowName} ---");

        foreach (var ruleDef in workflow.Rules)
        {
            if (ct.IsCancellationRequested) break;

            // 1. Condition Check (Existing logic)
            if (!string.IsNullOrEmpty(ruleDef.Condition))
            {
                bool shouldExecute = ConditionEvaluator.Evaluate(ruleDef.Condition, pipeline);
                if (!shouldExecute) continue;
            }

            try
            {
                // --- NEW: FOREACH LOGIC ---
                if (ruleDef.Type == "Foreach")
                {
                    await ExecuteForeachAsync(pipeline, ruleDef, result, ct);
                    
                    // If any item in the loop failed a validation, stop the whole flow
                    if (!result.IsSuccess) break;
                }
                // ---------------------------    

                // --- NEW: SUB-FLOW LOGIC ---
                if (ruleDef.Type == "SubFlow")
                {
                    await ExecuteSubFlowAsync(pipeline, ruleDef, result, ct);

                    // If the sub-flow failed (e.g. a validation rule inside it failed), 
                    // we must stop the parent flow too.
                    if (!result.IsSuccess)
                    {
                        pipeline.Logger.LogWarning($"SubFlow '{ruleDef.SubFlowRef}' failed. Stopping parent flow.");
                        break;
                    }
                }
                // ---------------------------

                // 2. Validation Logic (Existing)
                else if (ruleDef.Type == "Validation")
                {
                    bool isValid = true;
                    if (ruleDef.Mode == "Simple") { /* ... Declarative Check ... */ }
                    else
                    {
                        /* ... Retrieve ValidationRule ... */
                        // Simplified for brevity:
                        var ruleType = Type.GetType(ruleDef.ImplementationType ?? "");
                        var rule = pipeline.RetrieveValidationRule(ruleType!);
                        await rule!.ExecuteAsync(pipeline, null, valid => isValid = valid, ct);
                    }

                    if (!isValid)
                    {
                        result.IsSuccess = false;
                        result.Errors.Add(ruleDef.ErrorMessage ?? $"Validation failed in {ruleDef.Name}");
                        break; // Stop execution
                    }
                }

                // 3. Standard Rule Logic (Existing)
                else if (ruleDef.Type == "Rule")
                {
                    var ruleType = Type.GetType(ruleDef.ImplementationType ?? "");
                    var rule = pipeline.RetrieveRule(ruleType!);
                    await rule!.ExecuteAsync(pipeline, null, ct);
                }
            }
            catch (Exception ex)
            {
                pipeline.Logger.LogError(ex, $"Error in {ruleDef.Name}");
                result.Errors.Add(ex.Message);
                result.IsSuccess = false;
                break;
            }
        }

        pipeline.Logger.LogInformation($"--- Ending Scope: {workflow.WorkflowName} ---");

        result.FinalFlowObject = pipeline.FlowObject;
        result.OutputValues = pipeline.FlowObjects;
        return result;
    }

    // Helper to handle the recursion
    private static async Task ExecuteSubFlowAsync(
        IRulePipeline pipeline,
        RuleDefinition def,
        RuleEngineResult parentResult,
        CancellationToken ct)
    {
        var repo = pipeline.Services.GetService<IWorkflowRepository>();
        if (repo == null) throw new InvalidOperationException("IWorkflowRepository not registered.");

        var subJson = await repo.GetWorkflowConfigAsync(def.SubFlowRef!, ct);
        if (string.IsNullOrEmpty(subJson))
        {
            throw new FileNotFoundException($"SubFlow '{def.SubFlowRef}' not found.");
        }

        pipeline.Logger.LogInformation($"-> Entering SubFlow: {def.SubFlowRef}");

        // RECURSION: Call the same extension method
        // We pass the SAME pipeline instance to share state (FlowObject/FlowObjects)
        var subResult = await pipeline.ExecutePolicyAsync(subJson, ct);

        // Merge results back to parent
        if (!subResult.IsSuccess)
        {
            parentResult.IsSuccess = false;
            parentResult.Errors.AddRange(subResult.Errors);
        }

        pipeline.Logger.LogInformation($"<- Exiting SubFlow: {def.SubFlowRef}");
    }

    private static async Task ExecuteForeachAsync(
        IRulePipeline parentPipeline, 
        RuleDefinition def, 
        RuleEngineResult result, 
        CancellationToken ct)
    {
        if (parentPipeline.FlowObject == null) return;

        // 1. Get the Collection via Reflection
        var prop = parentPipeline.FlowObject.GetType().GetProperty(def.CollectionProperty ?? "");
        if (prop == null)
        {
            result.Errors.Add($"Collection property '{def.CollectionProperty}' not found on {parentPipeline.FlowObject.GetType().Name}");
            result.IsSuccess = false;
            return;
        }

        var collectionValue = prop.GetValue(parentPipeline.FlowObject);
        
        // Cast to IEnumerable to iterate
        if (collectionValue is not System.Collections.IEnumerable list)
        {
            result.Errors.Add($"Property '{def.CollectionProperty}' is not a collection.");
            result.IsSuccess = false;
            return;
        }

        // 2. Load the SubFlow JSON once
        var repo = parentPipeline.Services.GetService<IWorkflowRepository>();
        var subJson = await repo.GetWorkflowConfigAsync(def.SubFlowRef!, ct);

        if (string.IsNullOrEmpty(subJson)) throw new FileNotFoundException($"SubFlow '{def.SubFlowRef}' not found");

        parentPipeline.Logger.LogInformation($"--- Starting Loop: {def.Name} ---");

        int index = 0;
        foreach (var item in list)
        {
            if (ct.IsCancellationRequested) break;

            // 3. Create Scoped Pipeline
            // The 'item' becomes the FlowObject for this run
            var childPipeline = new RulePipeline(parentPipeline, item);

            // 4. Execute SubFlow on the item
            var childResult = await childPipeline.ExecutePolicyAsync(subJson, ct);

            if (!childResult.IsSuccess)
            {
                parentPipeline.Logger.LogWarning($"Loop Item #{index} Failed.");
                result.IsSuccess = false;
                result.Errors.Add($"Failure in loop '{def.Name}' at index {index}: {string.Join(", ", childResult.Errors)}");
                
                // Fail fast: Stop processing rest of list
                break; 
            }
            index++;
        }
        
        parentPipeline.Logger.LogInformation($"--- Ending Loop: {def.Name} ---");
    }
}