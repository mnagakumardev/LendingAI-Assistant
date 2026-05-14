using LendingAI.MCP.Models;
using LendingAI.MCP.Server;
using Microsoft.AspNetCore.Mvc;

namespace LendingAI.MCP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MCPToolsController : ControllerBase
{
    private readonly IToolRegistry _toolRegistry;
    private readonly IMCPServerContext _serverContext;
    private readonly ILogger<MCPToolsController> _logger;

    public MCPToolsController(
        IToolRegistry toolRegistry,
        IMCPServerContext serverContext,
        ILogger<MCPToolsController> logger)
    {
        _toolRegistry = toolRegistry;
        _serverContext = serverContext;
        _logger = logger;
    }

    [HttpGet("info")]
    public ActionResult<MCPServerInfo> GetServerInfo()
    {
        var info = new MCPServerInfo
        {
            Name = _serverContext.Name,
            Version = _serverContext.Version,
            AvailableTools = _serverContext.GetAvailableTools(),
            AvailableResources = _serverContext.GetAvailableResources(),
            StartedAt = _serverContext.StartedAt
        };
        return Ok(info);
    }

    [HttpGet("tools")]
    public ActionResult<List<MCPTool>> GetAllTools()
    {
        var tools = _toolRegistry.GetAllTools();
        return Ok(tools.Select(t => new { t.Name, t.Description, t.InputSchema }).ToList());
    }

    [HttpPost("execute/{toolName}")]
    public async Task<ActionResult<MCPToolExecutionResponse>> ExecuteTool(
        string toolName,
        [FromBody] Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Executing tool: {ToolName}", toolName);

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _toolRegistry.ExecuteToolAsync(toolName, parameters);
            stopwatch.Stop();

            var response = new MCPToolExecutionResponse
            {
                ToolName = toolName,
                Success = result.Success,
                Result = result.Data,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.Metadata,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Tool not found: {ToolName}", toolName);
            return NotFound(new { error = $"Tool '{toolName}' not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("assess-loan")]
    public async Task<ActionResult<MCPToolExecutionResponse>> AssessLoan(
        [FromBody] AssessLoanRequest request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "applicationId", request.ApplicationId },
            { "includeFraudCheck", request.IncludeFraudCheck ?? true },
            { "includeMarketAnalysis", request.IncludeMarketAnalysis ?? true },
            { "includeComplianceCheck", request.IncludeComplianceCheck ?? true },
            { "deepAnalysis", request.DeepAnalysis ?? false }
        };

        return await ExecuteTool("assess_loan_application", parameters);
    }

    [HttpPost("verify-credit")]
    public async Task<ActionResult<MCPToolExecutionResponse>> VerifyCredit(
        [FromBody] VerifyCreditRequest request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "ssn", request.SSN },
            { "bureau", request.Bureau ?? "all" }
        };

        return await ExecuteTool("verify_credit_score", parameters);
    }

    [HttpPost("check-fraud")]
    public async Task<ActionResult<MCPToolExecutionResponse>> CheckFraud(
        [FromBody] CheckFraudRequest request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "applicationId", request.ApplicationId },
            { "deepAnalysis", request.DeepAnalysis ?? true }
        };

        return await ExecuteTool("check_fraud_indicators", parameters);
    }

    [HttpPost("verify-documents")]
    public async Task<ActionResult<MCPToolExecutionResponse>> VerifyDocuments(
        [FromBody] VerifyDocumentsRequest request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "applicationId", request.ApplicationId },
            { "documentTypes", request.DocumentTypes ?? new List<string>() }
        };

        return await ExecuteTool("verify_documents", parameters);
    }

    [HttpPost("analyze-market-rates")]
    public async Task<ActionResult<MCPToolExecutionResponse>> AnalyzeMarketRates(
        [FromBody] AnalyzeMarketRatesRequest request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "loanType", request.LoanType },
            { "amount", request.Amount },
            { "term", request.Term },
            { "creditScore", request.CreditScore }
        };

        return await ExecuteTool("analyze_market_rates", parameters);
    }

    [HttpPost("check-compliance")]
    public async Task<ActionResult<MCPToolExecutionResponse>> CheckCompliance(
        [FromBody] CheckComplianceRequest request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "applicationId", request.ApplicationId },
            { "regulations", request.Regulations ?? new List<string>() }
        };

        return await ExecuteTool("check_compliance", parameters);
    }
}

// Request DTOs
public class AssessLoanRequest
{
    public string ApplicationId { get; set; }
    public bool? IncludeFraudCheck { get; set; }
    public bool? IncludeMarketAnalysis { get; set; }
    public bool? IncludeComplianceCheck { get; set; }
    public bool? DeepAnalysis { get; set; }
}

public class VerifyCreditRequest
{
    public string SSN { get; set; }
    public string Bureau { get; set; }
}

public class CheckFraudRequest
{
    public string ApplicationId { get; set; }
    public bool? DeepAnalysis { get; set; }
}

public class VerifyDocumentsRequest
{
    public string ApplicationId { get; set; }
    public List<string> DocumentTypes { get; set; }
}

public class AnalyzeMarketRatesRequest
{
    public string LoanType { get; set; }
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public int CreditScore { get; set; }
}

public class CheckComplianceRequest
{
    public string ApplicationId { get; set; }
    public List<string> Regulations { get; set; }
}