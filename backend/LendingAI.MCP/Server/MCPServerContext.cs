namespace LendingAI.MCP.Server;

public interface IMCPServerContext
{
    string Name { get; }
    string Version { get; }
    DateTime StartedAt { get; }
    List<string> GetAvailableTools();
    List<string> GetAvailableResources();
}

public class MCPServerContext : IMCPServerContext
{
    private readonly ILogger<MCPServerContext> _logger;
    private List<string> _tools = new();
    private List<string> _resources = new();

    public string Name => "LendingAI-MCP-Server";
    public string Version => "1.0.0";
    public DateTime StartedAt { get; }

    public MCPServerContext(ILogger<MCPServerContext> logger)
    {
        _logger = logger;
        StartedAt = DateTime.UtcNow;
        _logger.LogInformation("MCP Server Context initialized at {StartedAt}", StartedAt);
    }

    public List<string> GetAvailableTools()
    {
        return _tools;
    }

    public List<string> GetAvailableResources()
    {
        return _resources;
    }

    public void RegisterTool(string toolName)
    {
        if (!_tools.Contains(toolName))
        {
            _tools.Add(toolName);
            _logger.LogInformation("Tool registered: {ToolName}", toolName);
        }
    }

    public void RegisterResource(string resourceName)
    {
        if (!_resources.Contains(resourceName))
        {
            _resources.Add(resourceName);
            _logger.LogInformation("Resource registered: {ResourceName}", resourceName);
        }
    }
}