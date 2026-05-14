namespace LendingAI.Core.Models;

public class LoanApplication
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ApplicantInfo ApplicantInfo { get; set; } = new();
    public LoanDetails LoanDetails { get; set; } = new();
    public List<Document> Documents { get; set; } = new();
    public AIAssessmentResult? AIAssessment { get; set; }
    public CreditScoreResult? CreditScore { get; set; }
    public string Status { get; set; } = "DRAFT";
}

public class ApplicantInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string SSN { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

public class LoanDetails
{
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public string LoanType { get; set; } // Personal, Home, Auto, etc.
    public string Purpose { get; set; }
    public decimal AnnualIncome { get; set; }
    public string EmploymentStatus { get; set; }
}

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentType { get; set; } // PayStub, BankStatement, etc.
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DocumentExtractionResult? ExtractionResult { get; set; }
}

public class DocumentExtractionResult
{
    public Dictionary<string, string> ExtractedData { get; set; } = new();
    public double Confidence { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class AIAssessmentResult
{
    public string AssessmentId { get; set; } = Guid.NewGuid().ToString();
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    public double ApprovalProbability { get; set; }
    public string Recommendation { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> PositiveFactors { get; set; } = new();
    public string AnalysisSummary { get; set; }
    public Dictionary<string, double> ScoreBreakdown { get; set; } = new();
}

public class CreditScoreResult
{
    public int Score { get; set; }
    public string Rating { get; set; } // Excellent, Good, Fair, Poor
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public List<CreditFactor> Factors { get; set; } = new();
}

public class CreditFactor
{
    public string Name { get; set; }
    public double Weight { get; set; }
    public double Impact { get; set; }
}