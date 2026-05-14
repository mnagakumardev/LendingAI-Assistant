using LendingAI.MCP.Models;

namespace LendingAI.MCP.Server;

public interface IToolRegistry
{
    Task InitializeAsync();
    MCPTool GetTool(string toolName);
    List<MCPTool> GetAllTools();
    Task<MCPToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters);
    void RegisterTool(MCPTool tool);
}

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, MCPTool> _tools = new();
    private readonly ILoanAssessmentService _loanAssessmentService;
    private readonly ICreditVerificationService _creditVerificationService;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly IDocumentVerificationService _documentVerificationService;
    private readonly IMarketAnalysisService _marketAnalysisService;
    private readonly IComplianceCheckService _complianceCheckService;
    private readonly IMCPServerContext _serverContext;
    private readonly ILogger<ToolRegistry> _logger;

    public ToolRegistry(
        ILoanAssessmentService loanAssessmentService,
        ICreditVerificationService creditVerificationService,
        IFraudDetectionService fraudDetectionService,
        IDocumentVerificationService documentVerificationService,
        IMarketAnalysisService marketAnalysisService,
        IComplianceCheckService complianceCheckService,
        IMCPServerContext serverContext,
        ILogger<ToolRegistry> logger)
    {
        _loanAssessmentService = loanAssessmentService;
        _creditVerificationService = creditVerificationService;
        _fraudDetectionService = fraudDetectionService;
        _documentVerificationService = documentVerificationService;
        _marketAnalysisService = marketAnalysisService;
        _complianceCheckService = complianceCheckService;
        _serverContext = serverContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Tool Registry...");

        // Register all tools
        await RegisterAllToolsAsync();

        _logger.LogInformation("Tool Registry initialized with {ToolCount} tools", _tools.Count);
    }

    private async Task RegisterAllToolsAsync()
    {
        await Task.Run(() =>
        {
            // 1. Assess Loan Application Tool
            RegisterTool(new MCPTool
            {
                Name = "assess_loan_application",
                Description = "Comprehensive loan assessment using business rules and ML models",
                InputSchema = new Dictionary<string, object>
                {
                    { "applicationId", "string" },
                    { "includeFraudCheck", "boolean" },
                    { "includeMarketAnalysis", "boolean" },
                    { "includeComplianceCheck", "boolean" },
                    { "deepAnalysis", "boolean" }
                },
                Handler = async (parameters) =>
                {
                    var applicationId = parameters["applicationId"].ToString();
                    var includeFraud = (bool)parameters.GetValueOrDefault("includeFraudCheck", false);
                    var includeMarket = (bool)parameters.GetValueOrDefault("includeMarketAnalysis", false);
                    var includeCompliance = (bool)parameters.GetValueOrDefault("includeComplianceCheck", false);
                    var deepAnalysis = (bool)parameters.GetValueOrDefault("deepAnalysis", false);

                    try
                    {
                        var result = await _loanAssessmentService.AssessLoanApplicationAsync(
                            applicationId, includeFraud, includeMarket, includeCompliance, deepAnalysis);
                        return new MCPToolResult
                        {
                            Success = true,
                            Data = result,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex)
                    {
                        return new MCPToolResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                }
            });
            _serverContext.RegisterTool("assess_loan_application");

            // 2. Verify Credit Score Tool
            RegisterTool(new MCPTool
            {
                Name = "verify_credit_score",
                Description = "Verify credit score from external credit bureaus",
                InputSchema = new Dictionary<string, object>
                {
                    { "ssn", "string" },
                    { "bureau", "string" }
                },
                Handler = async (parameters) =>
                {
                    var ssn = parameters["ssn"].ToString();
                    var bureau = parameters.GetValueOrDefault("bureau", "all").ToString();

                    try
                    {
                        var result = await _creditVerificationService.VerifyCreditScoreAsync(ssn, bureau);
                        return new MCPToolResult
                        {
                            Success = true,
                            Data = result,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex)
                    {
                        return new MCPToolResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                }
            });
            _serverContext.RegisterTool("verify_credit_score");

            // 3. Check Fraud Indicators Tool
            RegisterTool(new MCPTool
            {
                Name = "check_fraud_indicators",
                Description = "Analyze application for fraud indicators",
                InputSchema = new Dictionary<string, object>
                {
                    { "applicationId", "string" },
                    { "deepAnalysis", "boolean" }
                },
                Handler = async (parameters) =>
                {
                    var applicationId = parameters["applicationId"].ToString();
                    var deepAnalysis = (bool)parameters.GetValueOrDefault("deepAnalysis", true);

                    try
                    {
                        var result = await _fraudDetectionService.CheckFraudIndicatorsAsync(
                            applicationId, deepAnalysis);
                        return new MCPToolResult
                        {
                            Success = true,
                            Data = result,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex)
                    {
                        return new MCPToolResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                }
            });
            _serverContext.RegisterTool("check_fraud_indicators");

            // 4. Verify Documents Tool
            RegisterTool(new MCPTool
            {
                Name = "verify_documents",
                Description = "Verify authenticity and validity of submitted documents",
                InputSchema = new Dictionary<string, object>
                {
                    { "applicationId", "string" },
                    { "documentTypes", "array" }
                },
                Handler = async (parameters) =>
                {
                    var applicationId = parameters["applicationId"].ToString();
                    var documentTypes = parameters.GetValueOrDefault("documentTypes") as List<object> ?? new();

                    try
                    {
                        var result = await _documentVerificationService.VerifyDocumentsAsync(
                            applicationId, documentTypes.Cast<string>().ToList());
                        return new MCPToolResult
                        {
                            Success = true,
                            Data = result,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex)
                    {
                        return new MCPToolResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                }
            });
            _serverContext.RegisterTool("verify_documents");

            // 5. Analyze Market Rates Tool
            RegisterTool(new MCPTool
            {
                Name = "analyze_market_rates",
                Description = "Analyze current market rates and competitive positioning",
                InputSchema = new Dictionary<string, object>
                {
                    { "loanType", "string" },
                    { "amount", "number" },
                    { "term", "number" },
                    { "creditScore", "number" }
                },
                Handler = async (parameters) =>
                {
                    var loanType = parameters["loanType"].ToString();
                    var amount = decimal.Parse(parameters["amount"].ToString());
                    var term = int.Parse(parameters["term"].ToString());
                    var creditScore = int.Parse(parameters["creditScore"].ToString());

                    try
                    {
                        var result = await _marketAnalysisService.AnalyzeMarketRatesAsync(
                            loanType, amount, term, creditScore);
                        return new MCPToolResult
                        {
                            Success = true,
                            Data = result,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex)
                    {
                        return new MCPToolResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                }
            });
            _serverContext.RegisterTool("analyze_market_rates");

            // 6. Check Compliance Tool
            RegisterTool(new MCPTool
            {
                Name = "check_compliance",
                Description = "Verify compliance with lending regulations",
                InputSchema = new Dictionary<string, object>
                {
                    { "applicationId", "string" },
                    { "regulations", "array" }
                },
                Handler = async (parameters) =>
                {
                    var applicationId = parameters["applicationId"].ToString();
                    var regulations = parameters.GetValueOrDefault("regulations") as List<object> ?? new();

                    try
                    {
                        var result = await _complianceCheckService.CheckComplianceAsync(
                            applicationId, regulations.Cast<string>().ToList());
                        return new MCPToolResult
                        {
                            Success = true,
                            Data = result,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex)
                    {
                        return new MCPToolResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        };
                    }
                }
            });
            _serverContext.RegisterTool("check_compliance");
        });
    }

    public MCPTool GetTool(string toolName)
    {
        if (_tools.TryGetValue(toolName, out var tool))
        {
            return tool;
        }
        throw new KeyNotFoundException($"Tool '{toolName}' not found");
    }

    public List<MCPTool> GetAllTools()
    {
        return _tools.Values.ToList();
    }

    public async Task<MCPToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Executing tool: {ToolName} with parameters: {@Parameters}", toolName, parameters);

        try
        {
            var tool = GetTool(toolName);
            var result = await tool.Handler(parameters);
            result.Metadata = new Dictionary<string, object>
            {
                { "toolName", toolName },
                { "executedBy", "MCP Server" }
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolName);
            return new MCPToolResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    public void RegisterTool(MCPTool tool)
    {
        _tools[tool.Name] = tool;
        _logger.LogInformation("Tool registered: {ToolName}", tool.Name);
    }
}