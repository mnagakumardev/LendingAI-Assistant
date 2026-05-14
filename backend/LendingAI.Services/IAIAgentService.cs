using LendingAI.Core.Models;
using Azure.AI.OpenAI;
using System.Text.Json;

namespace LendingAI.Services;

public interface IAIAgentService
{
    Task<AIAssessmentResult> AssessApplicationAsync(LoanApplication application);
}

public class AIAgentService : IAIAgentService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<AIAgentService> _logger;
    private const string DeploymentName = "gpt-4";

    public AIAgentService(OpenAIClient openAIClient, ILogger<AIAgentService> logger)
    {
        _openAIClient = openAIClient;
        _logger = logger;
    }

    public async Task<AIAssessmentResult> AssessApplicationAsync(LoanApplication application)
    {
        try
        {
            var prompt = BuildAssessmentPrompt(application);
            
            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = DeploymentName,
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are an expert loan officer AI assistant. Analyze loan applications and provide detailed risk assessments with approval probabilities."),
                    new ChatMessage(ChatRole.User, prompt)
                },
                Temperature = 0.7f,
                MaxTokens = 1000
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
            var responseText = response.Value.Choices[0].Message.Content;

            var assessment = ParseAIResponse(responseText, application);
            _logger.LogInformation("AI assessment completed for application: {ApplicationId}", application.Id);
            
            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI assessment for application: {ApplicationId}", application.Id);
            throw;
        }
    }

    private string BuildAssessmentPrompt(LoanApplication application)
    {
        return $@"
Analyze the following loan application and provide a risk assessment:

APPLICANT INFORMATION:
- Name: {application.ApplicantInfo.FirstName} {application.ApplicantInfo.LastName}
- Age: {CalculateAge(application.ApplicantInfo.DateOfBirth)}
- Email: {application.ApplicantInfo.Email}

LOAN DETAILS:
- Amount Requested: ${application.LoanDetails.LoanAmount:F2}
- Loan Term: {application.LoanDetails.LoanTermMonths} months
- Loan Type: {application.LoanDetails.LoanType}
- Purpose: {application.LoanDetails.Purpose}
- Annual Income: ${application.LoanDetails.AnnualIncome:F2}

DEBT-TO-INCOME RATIO: {CalculateDTI(application):P2}

Please provide:
1. Approval probability (0-100%)
2. Recommendation (APPROVE, CONDITIONAL_APPROVAL, DENY)
3. Key risk factors
4. Positive factors
5. Summary analysis

Format your response as JSON.";
    }

    private AIAssessmentResult ParseAIResponse(string responseText, LoanApplication application)
    {
        try
        {
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}') + 1;
            var jsonString = responseText.Substring(jsonStart, jsonEnd - jsonStart);
            
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            return new AIAssessmentResult
            {
                ApprovalProbability = root.TryGetProperty("approval_probability", out var ap) 
                    ? double.Parse(ap.GetString()?.Replace("%", "") ?? "0") / 100
                    : 0.5,
                Recommendation = root.TryGetProperty("recommendation", out var rec)
                    ? rec.GetString() ?? "CONDITIONAL_APPROVAL"
                    : "CONDITIONAL_APPROVAL",
                RiskFactors = root.TryGetProperty("risk_factors", out var rf)
                    ? rf.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : new(),
                PositiveFactors = root.TryGetProperty("positive_factors", out var pf)
                    ? pf.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : new(),
                AnalysisSummary = root.TryGetProperty("summary", out var sum)
                    ? sum.GetString() ?? ""
                    : ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing AI response, returning default assessment");
            return new AIAssessmentResult
            {
                ApprovalProbability = 0.5,
                Recommendation = "CONDITIONAL_APPROVAL",
                AnalysisSummary = "Manual review required"
            };
        }
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    private decimal CalculateDTI(LoanApplication application)
    {
        var monthlyLoanPayment = application.LoanDetails.LoanAmount / application.LoanDetails.LoanTermMonths;
        var monthlyIncome = application.LoanDetails.AnnualIncome / 12;
        return monthlyIncome > 0 ? monthlyLoanPayment / monthlyIncome : 0;
    }
}