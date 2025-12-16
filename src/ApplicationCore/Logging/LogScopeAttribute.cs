namespace ApplicationCore.Logging;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class LogScopeAttribute : Attribute
{
    public string Name { get; }
    public string? Value { get; }

    public LogScopeAttribute(string name, string? value = null)
    {
        Name = name;
        Value = value;
    }
}