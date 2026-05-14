using LendingAI.MCP.Models;

namespace LendingAI.MCP.Services;

public interface ICreditVerificationService
{
    Task<object> VerifyCreditScoreAsync(string ssn, string bureau);
}

public class CreditVerificationService : ICreditVerificationService
{
    private readonly ILogger<CreditVerificationService> _logger;

    public CreditVerificationService(ILogger<CreditVerificationService> logger)
    {
        _logger = logger;
    }

    public async Task<object> VerifyCreditScoreAsync(string ssn, string bureau)
    {
        _logger.LogInformation("Verifying credit score for SSN: {SSN} from bureau: {Bureau}", MaskSSN(ssn), bureau);

        try
        {
            // Simulate credit bureau integration
            var bureauScores = new Dictionary<string, int>();
            var random = new Random(ssn.GetHashCode());

            if (bureau == "all" || bureau == "equifax")
                bureauScores["equifax"] = 680 + random.Next(-50, 100);

            if (bureau == "all" || bureau == "experian")
                bureauScores["experian"] = 690 + random.Next(-50, 100);

            if (bureau == "all" || bureau == "transunion")
                bureauScores["transunion"] = 700 + random.Next(-50, 100);

            var averageScore = bureauScores.Values.Any() ? bureauScores.Values.Average() : 0;

            var result = new CreditAnalysis
            {
                BureauScores = bureauScores.Cast<KeyValuePair<string, int>>().ToDictionary(x => x.Key, x => x.Value),
                AverageScore = (int)averageScore,
                CreditHistory = "Good",
                Recommendation = averageScore >= 720 ? "Approved" : averageScore >= 650 ? "Conditional" : "Review Required"
            };

            _logger.LogInformation("Credit verification completed. Average score: {AverageScore}", averageScore);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying credit score");
            throw;
        }
    }

    private string MaskSSN(string ssn)
    {
        return ssn.Length >= 4 ? $"XXX-XX-{ssn.Substring(ssn.Length - 4)}" : "XXX-XX-XXXX";
    }
}