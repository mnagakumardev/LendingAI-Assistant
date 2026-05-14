namespace LendingAI.MCP.Models;

/// <summary>
/// Represents a Tool in the MCP system
/// </summary>
public class MCPTool
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> InputSchema { get; set; }
    public Func<Dictionary<string, object>, Task<MCPToolResult>> Handler { get; set; }
}

/// <summary>
/// Result returned from MCP tool execution
/// </summary>
public class MCPToolResult
{
    public bool Success { get; set; }
    public object Data { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime ExecutedAt { get; set; }
}

/// <summary>
/// Represents a Resource in the MCP system
/// </summary>
public class MCPResource
{
    public string Name { get; set; }
    public string Uri { get; set; }
    public string Description { get; set; }
    public string MimeType { get; set; }
    public Func<Task<string>> ContentProvider { get; set; }
}

/// <summary>
/// Request to execute an MCP tool
/// </summary>
public class MCPToolExecutionRequest
{
    public string ToolName { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

/// <summary>
/// Response from executing an MCP tool
/// </summary>
public class MCPToolExecutionResponse
{
    public string ToolName { get; set; }
    public bool Success { get; set; }
    public object Result { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// MCP Server metadata
/// </summary>
public class MCPServerInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
    public List<string> AvailableTools { get; set; }
    public List<string> AvailableResources { get; set; }
    public DateTime StartedAt { get; set; }
}

/// <summary>
/// Loan Assessment Result from MCP
/// </summary>
public class MCPLoanAssessment
{
    public string ApplicationId { get; set; }
    public DateTime AssessedAt { get; set; }
    public LoanAssessmentFactors Factors { get; set; }
    public string Recommendation { get; set; }
    public double RiskScore { get; set; }
    public Dictionary<string, object> DetailedAnalysis { get; set; }
}

public class LoanAssessmentFactors
{
    public FinancialAnalysis Financial { get; set; }
    public CreditAnalysis Credit { get; set; }
    public EmploymentAnalysis Employment { get; set; }
    public DebtAnalysis Debt { get; set; }
    public FraudAnalysis Fraud { get; set; }
    public MarketAnalysis Market { get; set; }
    public ComplianceAnalysis Compliance { get; set; }
}

public class FinancialAnalysis
{
    public decimal AnnualIncome { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal DTI { get; set; }
    public string DTIRisk { get; set; }
    public string Recommendation { get; set; }
}

public class CreditAnalysis
{
    public int AverageScore { get; set; }
    public Dictionary<string, int> BureauScores { get; set; }
    public string CreditHistory { get; set; }
    public string Recommendation { get; set; }
}

public class EmploymentAnalysis
{
    public string Status { get; set; }
    public string Risk { get; set; }
    public string Recommendation { get; set; }
}

public class DebtAnalysis
{
    public List<object> ExistingDebts { get; set; }
    public decimal TotalMonthlyObligations { get; set; }
    public decimal RemainingCapacity { get; set; }
}

public class FraudAnalysis
{
    public string RiskLevel { get; set; }
    public List<string> Indicators { get; set; }
    public double MLScore { get; set; }
    public Dictionary<string, object> Details { get; set; }
}

public class MarketAnalysis
{
    public double CurrentRate { get; set; }
    public double MarketAverage { get; set; }
    public string CompetitivePosition { get; set; }
    public string Recommendation { get; set; }
}

public class ComplianceAnalysis
{
    public bool IsCompliant { get; set; }
    public List<string> CheckedRegulations { get; set; }
    public List<string> Issues { get; set; }
}