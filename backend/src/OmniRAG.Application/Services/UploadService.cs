using OmniRAG.Application.Abstractions;
using OmniRAG.Application.Dtos;
using OmniRAG.Domain.Entities;

namespace OmniRAG.Application.Services;

public sealed class UploadService : IUploadService
{
    private readonly IUploadFileStorage _fileStorage;
    private readonly IUploadRepository _repository;

    public UploadService(IUploadFileStorage fileStorage, IUploadRepository repository)
    {
        _fileStorage = fileStorage;
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<UploadRecordDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllAsync(cancellationToken);
        return records.Select(Map).ToArray();
    }

    public async Task<UploadRecordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken);
        return record is null ? null : Map(record);
    }

    public async Task<UploadRecordDto> CreateAsync(Stream fileStream, string fileName, string contentType, DateTime? uploadedAt, CancellationToken cancellationToken)
    {
        ValidateFileName(fileName);
        EnsureContentType(contentType);

        var storageKey = await _fileStorage.SaveAsync(fileStream, fileName, cancellationToken);
        var record = UploadRecord.Create(fileName, contentType, storageKey, uploadedAt);

        await _repository.AddAsync(record, cancellationToken);
        return Map(record);
    }

    public async Task<UploadRecordDto?> UpdateAsync(Guid id, Stream? fileStream, string? fileName, string? contentType, DateTime? uploadedAt, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var nextFileName = string.IsNullOrWhiteSpace(fileName) ? record.FileName : fileName.Trim();
        var nextContentType = string.IsNullOrWhiteSpace(contentType) ? record.ContentType : contentType.Trim();

        ValidateFileName(nextFileName);
        EnsureContentType(nextContentType);

        if (fileStream is not null)
        {
            await _fileStorage.ReplaceAsync(record.StorageKey, fileStream, cancellationToken);
        }

        record.Update(nextFileName, nextContentType, uploadedAt);
        await _repository.UpdateAsync(record, cancellationToken);

        return Map(record);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return false;
        }

        await _fileStorage.DeleteAsync(record.StorageKey, cancellationToken);
        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static void ValidateFileName(string fileName)
    {
        if (!UploadRecord.IsSupportedFileName(fileName))
        {
            throw new ArgumentException("Only CSV, XLS, and XLSX files are supported.", nameof(fileName));
        }
    }

    private static void EnsureContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }
    }

    private static UploadRecordDto Map(UploadRecord record) => new(record.Id, record.FileName, record.ContentType, record.UploadedAt);
}
