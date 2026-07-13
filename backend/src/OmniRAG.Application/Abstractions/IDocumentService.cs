namespace OmniRAG.Application.Abstractions;

public interface IDocumentService
{
    /// <summary>
    /// Stores raw document content in Cosmos DB and structured metadata in SQL, returning the created document id.
    /// </summary>
    Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string uploadedBy, CancellationToken cancellationToken);
}
