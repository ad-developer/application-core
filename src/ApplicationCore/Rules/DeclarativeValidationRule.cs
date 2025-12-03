using System.Collections;
using System.Text.RegularExpressions;
using ApplicationCore.Rules.Abstractions;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;

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

        // 1. Resolve Property Value
        var prop = pipeline.FlowObject.GetType().GetProperty(_def.TargetProperty ?? "");
        if (prop == null)
        {
            pipeline.Logger.LogError($"Property '{_def.TargetProperty}' not found on {pipeline.FlowObject.GetType().Name}");
            onValidationComplete?.Invoke(false);
            return Task.CompletedTask;
        }

        object? actualValue = prop.GetValue(pipeline.FlowObject);
        string operatorType = _def.Operator?.ToLower() ?? "equals";
        string targetValueStr = _def.TargetValue ?? string.Empty;

        bool isValid = false;

        try
        {
            // 2. Route to logic handlers
            if (operatorType == "isnull") isValid = actualValue == null;
            else if (operatorType == "isnotnull") isValid = actualValue != null;
            else if (actualValue == null) isValid = false; // Non-null checks fail on null
            else
            {
                isValid = operatorType switch
                {
                    // Equality
                    "equals" or "==" => CompareEquality(actualValue, targetValueStr, prop.PropertyType),
                    "notequals" or "!=" => !CompareEquality(actualValue, targetValueStr, prop.PropertyType),

                    // Numeric / DateTime
                    "greaterthan" or ">" => CompareNumeric(actualValue, targetValueStr) > 0,
                    "greaterthanorequal" or ">=" => CompareNumeric(actualValue, targetValueStr) >= 0,
                    "lessthan" or "<" => CompareNumeric(actualValue, targetValueStr) < 0,
                    "lessthanorequal" or "<=" => CompareNumeric(actualValue, targetValueStr) <= 0,
                    "between" => CheckBetween(actualValue, targetValueStr),

                    // String Specific
                    "contains" => CheckString(actualValue, targetValueStr, s => s.Contains(targetValueStr, StringComparison.OrdinalIgnoreCase)),
                    "startswith" => CheckString(actualValue, targetValueStr, s => s.StartsWith(targetValueStr, StringComparison.OrdinalIgnoreCase)),
                    "endswith" => CheckString(actualValue, targetValueStr, s => s.EndsWith(targetValueStr, StringComparison.OrdinalIgnoreCase)),
                    "regex" or "match" => CheckRegex(actualValue, targetValueStr),
                    
                    // Collections (List<T>, Arrays)
                    "listcontains" => CheckListContains(actualValue, targetValueStr),
                    "listcount" => CheckListCount(actualValue, targetValueStr),

                    _ => throw new NotImplementedException($"Operator '{operatorType}' not supported")
                };
            }
        }
        catch (Exception ex)
        {
            pipeline.Logger.LogWarning($"Declarative Rule Error [{_def.Name}]: {ex.Message}");
            isValid = false; 
        }

        if (!isValid)
        {
            pipeline.Logger.LogInformation($"Validation Failed: {_def.ErrorMessage ?? $"Check {_def.TargetProperty} {operatorType} {targetValueStr}"}");
        }

        onValidationComplete?.Invoke(isValid);
        return Task.CompletedTask;
    }

    // --- Helpers ---

    private bool CompareEquality(object actual, string targetStr, Type type)
    {
        // Handle Enums specifically
        if (type.IsEnum)
        {
            try {
                var targetEnum = Enum.Parse(type, targetStr);
                return actual.Equals(targetEnum);
            } catch { return false; }
        }
        
        // Handle basic types
        var target = ConvertType(targetStr, type);
        return actual.Equals(target);
    }

    private int CompareNumeric(object actual, string targetStr)
    {
        // Convert both to Decimal for safe comparison of ints, doubles, floats
        var d1 = Convert.ToDecimal(actual);
        var d2 = Convert.ToDecimal(targetStr);
        return d1.CompareTo(d2);
    }

    private bool CheckBetween(object actual, string targetStr)
    {
        // Expect format: "10,20" (Inclusive)
        var parts = targetStr.Split(',');
        if (parts.Length != 2) throw new ArgumentException("Between operator requires two comma-separated values (e.g., '10,20')");

        var val = Convert.ToDecimal(actual);
        var min = Convert.ToDecimal(parts[0]);
        var max = Convert.ToDecimal(parts[1]);

        return val >= min && val <= max;
    }

    private bool CheckString(object actual, string targetStr, Func<string, bool> check)
    {
        return check(actual.ToString() ?? "");
    }

    private bool CheckRegex(object actual, string pattern)
    {
        return Regex.IsMatch(actual.ToString() ?? "", pattern);
    }

    private bool CheckListContains(object collection, string targetStr)
    {
        if (collection is IEnumerable list)
        {
            foreach (var item in list)
            {
                if (item?.ToString()?.Equals(targetStr, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
        }
        return false;
    }

    private bool CheckListCount(object collection, string targetStr)
    {
        if (collection is ICollection list)
        {
            // Parses targetStr (e.g., ">0" or "5")
            // Simple implementation: check exact match
            if (int.TryParse(targetStr, out int count))
            {
                return list.Count == count;
            }
        }
        return false;
    }

    private object? ConvertType(string value, Type type)
    {
        // Handle Nullable types
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return Convert.ChangeType(value, underlying);
    }
}