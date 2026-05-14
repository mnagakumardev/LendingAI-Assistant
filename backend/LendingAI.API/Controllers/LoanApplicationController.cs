using LendingAI.Core.Models;
using LendingAI.Core.Dtos;
using LendingAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LendingAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanApplicationController : ControllerBase
{
    private readonly ILoanApplicationService _loanService;
    private readonly IAIAgentService _aiAgentService;
    private readonly ILogger<LoanApplicationController> _logger;

    public LoanApplicationController(
        ILoanApplicationService loanService,
        IAIAgentService aiAgentService,
        ILogger<LoanApplicationController> logger)
    {
        _loanService = loanService;
        _aiAgentService = aiAgentService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<LoanApplicationDto>> CreateApplication([FromBody] CreateLoanApplicationRequest request)
    {
        _logger.LogInformation("Creating loan application for applicant: {ApplicantName}", request.ApplicantName);
        
        var application = await _loanService.CreateApplicationAsync(request);
        return Ok(application);
    }

    [HttpPost("{applicationId}/ai-assessment")]
    public async Task<ActionResult<AIAssessmentResult>> RunAIAssessment(string applicationId)
    {
        _logger.LogInformation("Running AI assessment for application: {ApplicationId}", applicationId);
        
        var application = await _loanService.GetApplicationAsync(applicationId);
        if (application == null)
            return NotFound("Application not found");

        var assessment = await _aiAgentService.AssessApplicationAsync(application);
        await _loanService.UpdateApplicationStatusAsync(applicationId, "AI_ASSESSED", assessment);
        
        return Ok(assessment);
    }

    [HttpPost("{applicationId}/documents")]
    public async Task<ActionResult<DocumentProcessingResult>> ProcessDocuments(
        string applicationId,
        IFormFileCollection files)
    {
        _logger.LogInformation("Processing {FileCount} documents for application: {ApplicationId}", files.Count, applicationId);
        
        var documents = await _loanService.ProcessDocumentsAsync(applicationId, files);
        return Ok(documents);
    }

    [HttpGet("{applicationId}")]
    public async Task<ActionResult<LoanApplicationDto>> GetApplication(string applicationId)
    {
        var application = await _loanService.GetApplicationAsync(applicationId);
        if (application == null)
            return NotFound();

        return Ok(application);
    }

    [HttpPost("{applicationId}/credit-score")]
    public async Task<ActionResult<CreditScoreResult>> CalculateCreditScore(string applicationId)
    {
        var application = await _loanService.GetApplicationAsync(applicationId);
        if (application == null)
            return NotFound();

        var creditScore = await _loanService.CalculateCreditScoreAsync(application);
        return Ok(creditScore);
    }

    [HttpPost("{applicationId}/submit")]
    public async Task<ActionResult<SubmissionResult>> SubmitApplication(string applicationId)
    {
        var result = await _loanService.SubmitApplicationAsync(applicationId);
        return Ok(result);
    }
}