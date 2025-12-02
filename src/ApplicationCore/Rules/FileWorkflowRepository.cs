using ApplicationCore.Rules.Abstractions;

namespace ApplicationCore.Rules;

// Simple File-Based Implementation
public class FileWorkflowRepository : IWorkflowRepository
{
    private readonly string _basePath;

    public FileWorkflowRepository(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<string?> GetWorkflowConfigAsync(string workflowName, CancellationToken ct = default)
    {
        var path = Path.Combine(_basePath, $"{workflowName}.json");
        if (!File.Exists(path)) return null;
        return await File.ReadAllTextAsync(path, ct);
    }
}
