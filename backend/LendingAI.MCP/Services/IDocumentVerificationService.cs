namespace LendingAI.MCP.Services;

public interface IDocumentVerificationService
{
    Task<object> VerifyDocumentsAsync(string applicationId, List<string> documentTypes);
}

public class DocumentVerificationService : IDocumentVerificationService
{
    private readonly ILogger<DocumentVerificationService> _logger;

    public DocumentVerificationService(ILogger<DocumentVerificationService> logger)
    {
        _logger = logger;
    }

    public async Task<object> VerifyDocumentsAsync(string applicationId, List<string> documentTypes)
    {
        _logger.LogInformation(
            "Verifying {DocumentCount} documents for application: {ApplicationId}",
            documentTypes.Count, applicationId);

        try
        {
            var verificationResults = new Dictionary<string, object>();
            var random = new Random(applicationId.GetHashCode());

            foreach (var docType in documentTypes)
            {
                verificationResults[docType] = new
                {
                    status = random.Next(100) > 10 ? "Verified" : "Failed",
                    confidence = (random.NextDouble() * 0.3) + 0.7, // 70-100% confidence
                    verifiedAt = DateTime.UtcNow
                };
            }

            var result = new
            {
                documentsVerified = documentTypes,
                verificationStatus = verificationResults.Values.Cast<dynamic>().All(x => x.status == "Verified") ? "Passed" : "Failed",
                details = verificationResults
            };

            _logger.LogInformation("Document verification completed for application: {ApplicationId}", applicationId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying documents");
            throw;
        }
    }
}