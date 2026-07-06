using OmniRAG.Application.Abstractions;
using OmniRAG.Domain.Entities;

namespace OmniRAG.Infrastructure.Storage;

public sealed class LocalUploadFileStorage : IUploadFileStorage
{
    private readonly string _rootPath;

    public LocalUploadFileStorage(string rootPath)
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken)
    {
        ValidateFileName(fileName);

        var storageKey = CreateStorageKey(fileName);
        var fullPath = Path.Combine(_rootPath, storageKey);

        await using var targetStream = File.Create(fullPath);
        await fileStream.CopyToAsync(targetStream, cancellationToken);
        return storageKey;
    }

    public async Task ReplaceAsync(string storageKey, Stream fileStream, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }

        var fullPath = Path.Combine(_rootPath, storageKey);
        await using var targetStream = File.Create(fullPath);
        await fileStream.CopyToAsync(targetStream, cancellationToken);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }

        var fullPath = Path.Combine(_rootPath, storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string CreateStorageKey(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return $"{Guid.NewGuid():N}{extension}";
    }

    private static void ValidateFileName(string fileName)
    {
        if (!UploadRecord.IsSupportedFileName(fileName))
        {
            throw new ArgumentException("Only CSV, XLS, and XLSX files are supported.", nameof(fileName));
        }
    }
}
