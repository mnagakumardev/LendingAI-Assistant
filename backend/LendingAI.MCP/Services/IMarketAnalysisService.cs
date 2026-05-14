using LendingAI.MCP.Models;

namespace LendingAI.MCP.Services;

public interface IMarketAnalysisService
{
    Task<MarketAnalysis> AnalyzeMarketRatesAsync(string loanType, decimal amount, int term, int creditScore);
}

public class MarketAnalysisService : IMarketAnalysisService
{
    private readonly ILogger<MarketAnalysisService> _logger;

    public MarketAnalysisService(ILogger<MarketAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<MarketAnalysis> AnalyzeMarketRatesAsync(
        string loanType, decimal amount, int term, int creditScore)
    {
        _logger.LogInformation(
            "Analyzing market rates for {LoanType}: ${Amount} over {Term} months, Credit Score: {CreditScore}",
            loanType, amount, term, creditScore);

        try
        {
            // Base rates by loan type
            var (baseRate, marketAvg) = loanType.ToLower() switch
            {
                "personal" => (6.5, 7.2),
                "home" => (4.5, 5.2),
                "auto" => (5.5, 6.2),
                "business" => (7.5, 8.2),
                _ => (6.5, 7.2)
            };

            // Adjust for credit score
            var creditAdjustment = creditScore switch
            {
                >= 800 => -2.0,
                >= 750 => -1.5,
                >= 700 => -1.0,
                >= 650 => -0.5,
                >= 600 => 0.0,
                >= 550 => 1.0,
                _ => 2.0
            };

            var currentRate = baseRate + creditAdjustment;

            var result = new MarketAnalysis
            {
                CurrentRate = currentRate,
                MarketAverage = marketAvg,
                CompetitivePosition = currentRate < marketAvg ? "Below Market" :
                                     currentRate > marketAvg ? "Above Market" : "At Market",
                Recommendation = currentRate < marketAvg ? "Favorable Terms" : "Review Terms"
            };

            _logger.LogInformation(
                "Market analysis completed. Current Rate: {CurrentRate}%, Market Average: {MarketAverage}%, Position: {Position}",
                currentRate, marketAvg, result.CompetitivePosition);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing market rates");
            throw;
        }
    }
}