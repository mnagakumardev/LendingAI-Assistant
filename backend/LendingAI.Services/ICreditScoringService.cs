using LendingAI.Core.Models;

namespace LendingAI.Services;

public interface ICreditScoringService
{
    Task<CreditScoreResult> CalculateScoreAsync(LoanApplication application);
}

public class CreditScoringService : ICreditScoringService
{
    private readonly ILogger<CreditScoringService> _logger;

    public CreditScoringService(ILogger<CreditScoringService> logger)
    {
        _logger = logger;
    }

    public async Task<CreditScoreResult> CalculateScoreAsync(LoanApplication application)
    {
        return await Task.FromResult(CalculateScore(application));
    }

    private CreditScoreResult CalculateScore(LoanApplication application)
    {
        var factors = new List<CreditFactor>();
        int score = 650; // Base score

        // DTI Factor
        var monthlyPayment = application.LoanDetails.LoanAmount / application.LoanDetails.LoanTermMonths;
        var monthlyIncome = application.LoanDetails.AnnualIncome / 12;
        var dti = monthlyIncome > 0 ? monthlyPayment / monthlyIncome : 1m;

        if (dti < 0.3m)
        {
            score += 80;
            factors.Add(new CreditFactor { Name = "Low Debt-to-Income", Weight = 0.35, Impact = 80 });
        }
        else if (dti < 0.5m)
        {
            score += 50;
            factors.Add(new CreditFactor { Name = "Moderate Debt-to-Income", Weight = 0.35, Impact = 50 });
        }
        else
        {
            score -= 30;
            factors.Add(new CreditFactor { Name = "High Debt-to-Income", Weight = 0.35, Impact = -30 });
        }

        // Loan Amount Factor
        var loanToIncomeRatio = application.LoanDetails.LoanAmount / application.LoanDetails.AnnualIncome;
        if (loanToIncomeRatio < 2)
            score += 40;
        factors.Add(new CreditFactor { Name = "Loan-to-Income Ratio", Weight = 0.25, Impact = 40 });

        // Employment Status
        if (application.LoanDetails.EmploymentStatus == "EMPLOYED")
            score += 30;
        factors.Add(new CreditFactor { Name = "Employment Status", Weight = 0.20, Impact = 30 });

        // Age Factor
        var age = CalculateAge(application.ApplicantInfo.DateOfBirth);
        if (age >= 25 && age <= 65)
            score += 20;
        factors.Add(new CreditFactor { Name = "Age", Weight = 0.10, Impact = 20 });

        score = Math.Min(850, Math.Max(300, score));

        return new CreditScoreResult
        {
            Score = score,
            Rating = GetRating(score),
            CalculatedAt = DateTime.UtcNow,
            Factors = factors
        };
    }

    private string GetRating(int score)
    {
        return score switch
        {
            >= 800 => "Excellent",
            >= 700 => "Good",
            >= 600 => "Fair",
            _ => "Poor"
        };
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}