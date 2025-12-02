using ApplicationCore.Rules.Abstractions;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Rules;


public class RulePipeline : IRulePipeline
{
    public object? FlowObject { get; set; }
    public Dictionary<string, object> FlowObjects { get; } = new Dictionary<string, object>();
    public ILogger Logger { get; }
    public Guid InstanceId { get; } = Guid.NewGuid();
    public IServiceProvider Services { get; }

    // Main Constructor
    public RulePipeline(ILogger<RulePipeline> logger, IServiceProvider services)
    {
        Logger = logger;
        Services = services;
        FlowObjects = new Dictionary<string, object>();
    }
    
    // NEW: Scoped Constructor (For Loops)
    // Inherits services and the state dictionary from the parent
    public RulePipeline(IRulePipeline parent, object childContextObject)
    {
        Logger = parent.Logger;
        Services = parent.Services;
        InstanceId = parent.InstanceId; // Keep same trace ID
        
        // Context is the specific item (e.g., OrderItem)
        FlowObject = childContextObject; 
        
        // SHARED State: Updates here reflect in the parent
        FlowObjects = parent.FlowObjects; 
        
        // Optional: specific logic to add "_Parent" reference if needed
        if (!FlowObjects.ContainsKey("_Parent"))
        {
            FlowObjects["_Parent"] = parent.FlowObject;
        }
    }

    public IRule? RetrieveRule(Type ruleType)
    {
        return Services.GetService(ruleType) as IRule;
    }

    public IValidationRule? RetrieveValidationRule(Type ruleType)
    {
        return Services.GetService(ruleType) as IValidationRule;
    }
}