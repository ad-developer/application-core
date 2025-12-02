namespace ApplicationCore.Rules;

public class RuleEngineResult
{
    public bool IsSuccess { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    
    // Captures the state of the object after all IRules have run
    public object? FinalFlowObject { get; set; } 
    public Dictionary<string, object> OutputValues { get; set; } = new();
}
