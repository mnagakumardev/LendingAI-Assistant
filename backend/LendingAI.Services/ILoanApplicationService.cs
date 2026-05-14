using LendingAI.Core.Models;
using LendingAI.Core.Dtos;

namespace LendingAI.Services;

public interface ILoanApplicationService
{
    Task<LoanApplicationDto> CreateApplicationAsync(CreateLoanApplicationRequest request);
    Task<LoanApplication?> GetApplicationAsync(string applicationId);
    Task UpdateApplicationStatusAsync(string applicationId, string status, object? data = null);
    Task<List<DocumentProcessingResult>> ProcessDocumentsAsync(string applicationId, IFormFileCollection files);
    Task<CreditScoreResult> CalculateCreditScoreAsync(LoanApplication application);
    Task<SubmissionResult> SubmitApplicationAsync(string applicationId);
}

public class LoanApplicationService : ILoanApplicationService
{
    private readonly Microsoft.Azure.Cosmos.Database _cosmosDb;
    private readonly IDocumentProcessingService _documentService;
    private readonly ICreditScoringService _creditScoringService;
    private readonly ILogger<LoanApplicationService> _logger;
    private const string ContainerId = "LoanApplications";

    public LoanApplicationService(
        Microsoft.Azure.Cosmos.Database cosmosDb,
        IDocumentProcessingService documentService,
        ICreditScoringService creditScoringService,
        ILogger<LoanApplicationService> logger)
    {
        _cosmosDb = cosmosDb;
        _documentService = documentService;
        _creditScoringService = creditScoringService;
        _logger = logger;
    }

    public async Task<LoanApplicationDto> CreateApplicationAsync(CreateLoanApplicationRequest request)
    {
        var application = new LoanApplication
        {
            ApplicantInfo = new ApplicantInfo
            {
                FirstName = request.ApplicantName.Split(' ')[0],
                LastName = request.ApplicantName.Split(' ').Length > 1 ? request.ApplicantName.Split(' ')[1] : "",
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            },
            LoanDetails = new LoanDetails
            {
                LoanAmount = request.LoanAmount,
                LoanTermMonths = request.LoanTermMonths,
                LoanType = request.LoanType,
                Purpose = request.Purpose,
                AnnualIncome = request.AnnualIncome
            }
        };

        var container = _cosmosDb.GetContainer(ContainerId);
        await container.CreateItemAsync(application, new Microsoft.Azure.Cosmos.PartitionKey(application.Id));

        _logger.LogInformation("Loan application created: {ApplicationId}", application.Id);

        return MapToDto(application);
    }

    public async Task<LoanApplication?> GetApplicationAsync(string applicationId)
    {
        try
        {
            var container = _cosmosDb.GetContainer(ContainerId);
            var response = await container.ReadItemAsync<LoanApplication>(applicationId, new Microsoft.Azure.Cosmos.PartitionKey(applicationId));
            return response.Resource;
        }
        catch (Microsoft.Azure.Cosmos.CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpdateApplicationStatusAsync(string applicationId, string status, object? data = null)
    {
        var application = await GetApplicationAsync(applicationId);
        if (application == null) return;

        application.Status = status;
        application.UpdatedAt = DateTime.UtcNow;

        if (data is AIAssessmentResult assessment)
            application.AIAssessment = assessment;

        var container = _cosmosDb.GetContainer(ContainerId);
        await container.UpsertItemAsync(application, new Microsoft.Azure.Cosmos.PartitionKey(applicationId));
    }

    public async Task<List<DocumentProcessingResult>> ProcessDocumentsAsync(string applicationId, IFormFileCollection files)
    {
        var results = new List<DocumentProcessingResult>();
        var application = await GetApplicationAsync(applicationId);
        
        if (application == null) return results;

        foreach (var file in files)
        {
            var result = await _documentService.ProcessDocumentAsync(file, applicationId);
            results.Add(result);
            
            var document = new Document
            {
                DocumentType = file.ContentType,
                FileName = file.FileName,
                StoragePath = result.StoragePath
            };
            application.Documents.Add(document);
        }

        var container = _cosmosDb.GetContainer(ContainerId);
        await container.UpsertItemAsync(application, new Microsoft.Azure.Cosmos.PartitionKey(applicationId));

        return results;
    }

    public async Task<CreditScoreResult> CalculateCreditScoreAsync(LoanApplication application)
    {
        return await _creditScoringService.CalculateScoreAsync(application);
    }

    public async Task<SubmissionResult> SubmitApplicationAsync(string applicationId)
    {
        var application = await GetApplicationAsync(applicationId);
        if (application == null)
            throw new InvalidOperationException("Application not found");

        application.Status = "SUBMITTED";
        var container = _cosmosDb.GetContainer(ContainerId);
        await container.UpsertItemAsync(application, new Microsoft.Azure.Cosmos.PartitionKey(applicationId));

        return new SubmissionResult
        {
            ApplicationId = applicationId,
            SubmittedAt = DateTime.UtcNow,
            Status = "SUBMITTED"
        };
    }

    private LoanApplicationDto MapToDto(LoanApplication app)
    {
        return new LoanApplicationDto
        {
            Id = app.Id,
            ApplicantInfo = new ApplicantInfoDto
            {
                FirstName = app.ApplicantInfo.FirstName,
                LastName = app.ApplicantInfo.LastName,
                Email = app.ApplicantInfo.Email,
                PhoneNumber = app.ApplicantInfo.PhoneNumber
            },
            LoanDetails = new LoanDetailsDto
            {
                LoanAmount = app.LoanDetails.LoanAmount,
                LoanTermMonths = app.LoanDetails.LoanTermMonths,
                LoanType = app.LoanDetails.LoanType,
                AnnualIncome = app.LoanDetails.AnnualIncome
            },
            Status = app.Status,
            CreatedAt = app.CreatedAt
        };
    }
}

public class DocumentProcessingResult
{
    public string DocumentId { get; set; }
    public string StoragePath { get; set; }
    public bool ProcessedSuccessfully { get; set; }
}

public class SubmissionResult
{
    public string ApplicationId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; }
}