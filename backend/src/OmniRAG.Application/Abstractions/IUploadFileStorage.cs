namespace OmniRAG.Application.Abstractions;

public interface IUploadFileStorage
{
    Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);

    Task ReplaceAsync(string storageKey, Stream fileStream, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
