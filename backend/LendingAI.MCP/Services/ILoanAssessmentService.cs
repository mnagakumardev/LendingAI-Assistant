using LendingAI.MCP.Models;

namespace LendingAI.MCP.Services;

public interface ILoanAssessmentService
{
    Task<MCPLoanAssessment> AssessLoanApplicationAsync(
        string applicationId,
        bool includeFraudCheck = false,
        bool includeMarketAnalysis = false,
        bool includeComplianceCheck = false,
        bool deepAnalysis = false);
}

public class LoanAssessmentService : ILoanAssessmentService
{
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly ICreditVerificationService _creditVerificationService;
    private readonly IMarketAnalysisService _marketAnalysisService;
    private readonly IComplianceCheckService _complianceCheckService;
    private readonly ILogger<LoanAssessmentService> _logger;
    private readonly HttpClient _httpClient;

    public LoanAssessmentService(
        IFraudDetectionService fraudDetectionService,
        ICreditVerificationService creditVerificationService,
        IMarketAnalysisService marketAnalysisService,
        IComplianceCheckService complianceCheckService,
        ILogger<LoanAssessmentService> logger,
        HttpClient httpClient)
    {
        _fraudDetectionService = fraudDetectionService;
        _creditVerificationService = creditVerificationService;
        _marketAnalysisService = marketAnalysisService;
        _complianceCheckService = complianceCheckService;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<MCPLoanAssessment> AssessLoanApplicationAsync(
        string applicationId,
        bool includeFraudCheck = false,
        bool includeMarketAnalysis = false,
        bool includeComplianceCheck = false,
        bool deepAnalysis = false)
    {
        _logger.LogInformation(
            "Starting comprehensive loan assessment for application: {ApplicationId}",
            applicationId);

        try
        {
            // Fetch application data from main API
            var applicationData = await FetchApplicationDataAsync(applicationId);

            // Perform core analysis
            var factors = new LoanAssessmentFactors
            {
                Financial = AnalyzeFinancials(applicationData),
                Employment = AnalyzeEmployment(applicationData),
                Debt = new DebtAnalysis
                {
                    ExistingDebts = new(),
                    TotalMonthlyObligations = 0,
                    RemainingCapacity = 0
                }
            };

            // Optional: Credit verification
            try
            {
                factors.Credit = await _creditVerificationService.VerifyCreditScoreAsync(
                    applicationData.ApplicantInfo.SSN, "all") as CreditAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Credit verification failed for application: {ApplicationId}", applicationId);
                factors.Credit = new CreditAnalysis { AverageScore = 0, Recommendation = "Verification Failed" };
            }

            // Optional: Fraud detection
            if (includeFraudCheck)
            {
                try
                {
                    var fraudResult = await _fraudDetectionService.CheckFraudIndicatorsAsync(
                        applicationId, deepAnalysis);
                    factors.Fraud = fraudResult as FraudAnalysis;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fraud detection failed for application: {ApplicationId}", applicationId);
                }
            }

            // Optional: Market analysis
            if (includeMarketAnalysis)
            {
                try
                {
                    factors.Market = await _marketAnalysisService.AnalyzeMarketRatesAsync(
                        applicationData.LoanDetails.LoanType,
                        applicationData.LoanDetails.LoanAmount,
                        applicationData.LoanDetails.LoanTermMonths,
                        factors.Credit?.AverageScore ?? 650);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Market analysis failed for application: {ApplicationId}", applicationId);
                }
            }

            // Optional: Compliance check
            if (includeComplianceCheck)
            {
                try
                {
                    factors.Compliance = await _complianceCheckService.CheckComplianceAsync(
                        applicationId, new[] { "FCRA", "TILA", "ECOA", "GLBA" }.ToList());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Compliance check failed for application: {ApplicationId}", applicationId);
                }
            }

            // Calculate overall risk score and recommendation
            var riskScore = CalculateRiskScore(factors);
            var recommendation = GenerateRecommendation(riskScore, factors);

            var assessment = new MCPLoanAssessment
            {
                ApplicationId = applicationId,
                AssessedAt = DateTime.UtcNow,
                Factors = factors,
                Recommendation = recommendation,
                RiskScore = riskScore,
                DetailedAnalysis = new Dictionary<string, object>
                {
                    { "assessmentDate", DateTime.UtcNow },
                    { "loanType", applicationData.LoanDetails.LoanType },
                    { "loanAmount", applicationData.LoanDetails.LoanAmount },
                    { "loanTerm", applicationData.LoanDetails.LoanTermMonths },
                    { "dti", factors.Financial.DTI },
                    { "creditScore", factors.Credit?.AverageScore ?? 0 }
                }
            };

            _logger.LogInformation(
                "Loan assessment completed for application: {ApplicationId}, Recommendation: {Recommendation}",
                applicationId, recommendation);

            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing loan application: {ApplicationId}", applicationId);
            throw;
        }
    }

    private async Task<dynamic> FetchApplicationDataAsync(string applicationId)
    {
        try
        {
            // This would call the main LendingAI API
            var response = await _httpClient.GetAsync(
                $"http://localhost:5000/api/loanapplication/{applicationId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching application data for: {ApplicationId}", applicationId);
            throw;
        }
    }

    private FinancialAnalysis AnalyzeFinancials(dynamic appData)
    {
        decimal income = appData.GetProperty("loanDetails").GetProperty("annualIncome").GetDecimal();
        decimal loanAmount = appData.GetProperty("loanDetails").GetProperty("loanAmount").GetDecimal();
        int loanTerm = appData.GetProperty("loanDetails").GetProperty("loanTermMonths").GetInt32();

        var monthlyPayment = loanAmount / loanTerm;
        var monthlyIncome = income / 12;
        var dti = monthlyIncome > 0 ? (double)(monthlyPayment / monthlyIncome) : 1.0;

        return new FinancialAnalysis
        {
            AnnualIncome = income,
            MonthlyIncome = monthlyIncome,
            DTI = (decimal)dti,
            DTIRisk = dti < 0.28 ? "Low" : dti < 0.43 ? "Medium" : "High",
            Recommendation = dti < 0.28 ? "Strong" : dti < 0.43 ? "Conditional" : "Risky"
        };
    }

    private EmploymentAnalysis AnalyzeEmployment(dynamic appData)
    {
        var employmentStatus = appData.GetProperty("loanDetails").GetProperty("employmentStatus").GetString();
        return new EmploymentAnalysis
        {
            Status = employmentStatus,
            Risk = employmentStatus == "EMPLOYED" ? "Low" : "High",
            Recommendation = employmentStatus == "EMPLOYED"
                ? "Stable income source"
                : "Verify income stability"
        };
    }

    private double CalculateRiskScore(LoanAssessmentFactors factors)
    {
        double score = 50; // Base score

        // Credit score impact (0-40 points)
        if (factors.Credit?.AverageScore >= 750)
            score -= 30;
        else if (factors.Credit?.AverageScore >= 700)
            score -= 20;
        else if (factors.Credit?.AverageScore >= 650)
            score -= 10;

        // DTI impact (0-40 points)
        if (factors.Financial.DTI < 0.28m)
            score -= 25;
        else if (factors.Financial.DTI < 0.43m)
            score -= 15;
        else
            score += 15;

        // Employment impact (0-20 points)
        if (factors.Employment.Risk == "Low")
            score -= 15;

        // Fraud impact (0-30 points)
        if (factors.Fraud?.RiskLevel == "Low")
            score -= 10;
        else if (factors.Fraud?.RiskLevel == "High")
            score += 20;

        return Math.Max(0, Math.Min(100, score));
    }

    private string GenerateRecommendation(double riskScore, LoanAssessmentFactors factors)
    {
        if (riskScore < 30)
            return "APPROVE";
        if (riskScore < 65)
            return "CONDITIONAL_APPROVAL";
        return "DENY";
    }
}