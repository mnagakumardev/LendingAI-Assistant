using Azure.Storage.Blobs;

namespace LendingAI.Services;

public interface IDocumentProcessingService
{
    Task<DocumentProcessingResult> ProcessDocumentAsync(IFormFile file, string applicationId);
}

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(BlobContainerClient containerClient, ILogger<DocumentProcessingService> logger)
    {
        _containerClient = containerClient;
        _logger = logger;
    }

    public async Task<DocumentProcessingResult> ProcessDocumentAsync(IFormFile file, string applicationId)
    {
        try
        {
            var fileName = $"{applicationId}/{Guid.NewGuid()}-{file.FileName}";
            
            using var stream = file.OpenReadStream();
            await _containerClient.UploadBlobAsync(fileName, stream, overwrite: true);

            _logger.LogInformation("Document processed successfully: {FileName}", fileName);

            return new DocumentProcessingResult
            {
                DocumentId = Guid.NewGuid().ToString(),
                StoragePath = fileName,
                ProcessedSuccessfully = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {FileName}", file.FileName);
            return new DocumentProcessingResult
            {
                ProcessedSuccessfully = false
            };
        }
    }
}

public class DocumentProcessingResult
{
    public string DocumentId { get; set; }
    public string StoragePath { get; set; }
    public bool ProcessedSuccessfully { get; set; }
}