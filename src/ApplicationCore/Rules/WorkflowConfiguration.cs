namespace ApplicationCore.Rules;

public class WorkflowConfiguration
{
    public string WorkflowName { get; set; } = string.Empty;
    public List<RuleDefinition> Rules { get; set; } = new();
}
