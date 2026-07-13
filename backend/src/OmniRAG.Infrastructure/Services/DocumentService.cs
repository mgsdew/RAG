using Microsoft.Azure.Cosmos;
using OmniRAG.Application.Abstractions;
using OmniRAG.Infrastructure.Persistence;

namespace OmniRAG.Infrastructure.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly RagSqlDbContext _dbContext;
    private readonly Container _cosmosContainer;

    public DocumentService(RagSqlDbContext dbContext, Container cosmosContainer)
    {
        _dbContext = dbContext;
        _cosmosContainer = cosmosContainer;
    }

    public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string uploadedBy, CancellationToken cancellationToken)
    {
        if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName is required", nameof(fileName));

        var documentId = Guid.NewGuid().ToString();

        string rawTextContent;
        using (var reader = new StreamReader(fileStream))
        {
            rawTextContent = await reader.ReadToEndAsync();
        }

        var cosmosItem = new
        {
            id = documentId,
            documentId = documentId,
            fileName = fileName,
            rawContent = rawTextContent,
            chunked = false
        };

        await _cosmosContainer.CreateItemAsync(cosmosItem, new PartitionKey(documentId));

        var metadata = new DocumentMetadata
        {
            DocumentId = documentId,
            FileName = fileName,
            FileSizeInBytes = fileStream.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy ?? "Unknown"
        };

        _dbContext.DocumentMetadata.Add(metadata);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return documentId;
    }
}
