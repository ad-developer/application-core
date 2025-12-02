namespace ApplicationCore.Rules;

public class RuleDefinition
{
    public string? Name { get; set; }
    
    // Types: "Rule", "Validation", or "SubFlow"
    public string Type { get; set; } = "Rule"; 
    
    public string Mode { get; set; } = "Complex"; 
    public string? ImplementationType { get; set; }
    
    // NEW: Reference to another JSON file/ID
    public string? SubFlowRef { get; set; } 

    // Declarative properties (from previous steps)
    public string? TargetProperty { get; set; }
    public string? Operator { get; set; }
    public string? TargetValue { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Condition { get; set; }

    // NEW: For "Foreach", the property name of the collection (e.g., "Items")
    public string? CollectionProperty { get; set; }
}
