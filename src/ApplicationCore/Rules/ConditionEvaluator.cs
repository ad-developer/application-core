using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using ApplicationCore.Rules.Abstractions;

namespace ApplicationCore.Rules;

public static class ConditionEvaluator
{
    // Cache compiled delegates for performance
    private static readonly Dictionary<string, Delegate> _cache = new();

    public static bool Evaluate(string condition, IRulePipeline context)
    {
        if (string.IsNullOrWhiteSpace(condition)) return true;

        if (!_cache.TryGetValue(condition, out var compiledFunc))
        {
            // Define the parameter (The Pipeline itself)
            var p = Expression.Parameter(typeof(IRulePipeline), "x");

            // Parse the string into a Lambda Expression
            // We expect the expression to return a boolean
            try 
            {
                var e = DynamicExpressionParser.ParseLambda(
                    new[] { p }, 
                    typeof(bool), 
                    condition
                );
                compiledFunc = e.Compile();
                _cache[condition] = compiledFunc;
            }
            catch (Exception ex)
            {
                // Log or throw depending on strictness
                throw new InvalidOperationException($"Invalid Condition Syntax: {condition}", ex);
            }
        }

        // Execute
        return (bool)compiledFunc.DynamicInvoke(context)!;
    }
}
