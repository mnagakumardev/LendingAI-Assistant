using LendingAI.MCP.Models;

namespace LendingAI.MCP.Services;

public interface IFraudDetectionService
{
    Task<object> CheckFraudIndicatorsAsync(string applicationId, bool deepAnalysis);
}

public class FraudDetectionService : IFraudDetectionService
{
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(ILogger<FraudDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<object> CheckFraudIndicatorsAsync(string applicationId, bool deepAnalysis)
    {
        _logger.LogInformation(
            "Checking fraud indicators for application: {ApplicationId}, DeepAnalysis: {DeepAnalysis}",
            applicationId, deepAnalysis);

        try
        {
            var fraudIndicators = new List<string>();
            var random = new Random(applicationId.GetHashCode());
            var mlScore = random.NextDouble() * 0.5; // 0-50% risk

            // Simulate fraud detection
            if (random.Next(100) > 70)
                fraudIndicators.Add("Multiple applications detected");

            if (random.Next(100) > 80)
                fraudIndicators.Add("Unusual income amount");

            var result = new FraudAnalysis
            {
                RiskLevel = mlScore < 0.15 ? "Low" : mlScore < 0.35 ? "Medium" : "High",
                Indicators = fraudIndicators,
                MLScore = mlScore,
                Details = deepAnalysis ? new Dictionary<string, object>
                {
                    { "documentAuthenticity", "Passed" },
                    { "identityVerification", "Verified" },
                    { "addressVerification", "Verified" }
                } : null
            };

            _logger.LogInformation("Fraud detection completed. Risk level: {RiskLevel}", result.RiskLevel);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking fraud indicators");
            throw;
        }
    }
}