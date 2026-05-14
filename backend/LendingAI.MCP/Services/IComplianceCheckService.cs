using LendingAI.MCP.Models;

namespace LendingAI.MCP.Services;

public interface IComplianceCheckService
{
    Task<ComplianceAnalysis> CheckComplianceAsync(string applicationId, List<string> regulations);
}

public class ComplianceCheckService : IComplianceCheckService
{
    private readonly ILogger<ComplianceCheckService> _logger;

    public ComplianceCheckService(ILogger<ComplianceCheckService> logger)
    {
        _logger = logger;
    }

    public async Task<ComplianceAnalysis> CheckComplianceAsync(string applicationId, List<string> regulations)
    {
        _logger.LogInformation(
            "Checking compliance for application: {ApplicationId} against {RegulationCount} regulations",
            applicationId, regulations.Count);

        try
        {
            var issues = new List<string>();
            var random = new Random(applicationId.GetHashCode());

            // Simulate compliance checks
            foreach (var regulation in regulations)
            {
                var passed = random.Next(100) > 5; // 95% pass rate
                if (!passed)
                    issues.Add($"{regulation} compliance check failed");
            }

            var result = new ComplianceAnalysis
            {
                IsCompliant = !issues.Any(),
                CheckedRegulations = regulations,
                Issues = issues
            };

            _logger.LogInformation(
                "Compliance check completed for application: {ApplicationId}. Compliant: {IsCompliant}",
                applicationId, result.IsCompliant);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compliance");
            throw;
        }
    }
}