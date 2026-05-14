namespace LendingAI.Core.Dtos;

public class CreateLoanApplicationRequest
{
    public string ApplicantName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public string LoanType { get; set; }
    public string Purpose { get; set; }
    public decimal AnnualIncome { get; set; }
}

public class LoanApplicationDto
{
    public string Id { get; set; }
    public ApplicantInfoDto ApplicantInfo { get; set; }
    public LoanDetailsDto LoanDetails { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApplicantInfoDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}

public class LoanDetailsDto
{
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public string LoanType { get; set; }
    public decimal AnnualIncome { get; set; }
}