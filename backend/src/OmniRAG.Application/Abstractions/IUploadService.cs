using OmniRAG.Application.Dtos;

namespace OmniRAG.Application.Abstractions;

public interface IUploadService
{
    Task<IReadOnlyCollection<UploadRecordDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<UploadRecordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UploadRecordDto> CreateAsync(Stream fileStream, string fileName, string contentType, DateTime? uploadedAt, CancellationToken cancellationToken);

    Task<UploadRecordDto?> UpdateAsync(Guid id, Stream? fileStream, string? fileName, string? contentType, DateTime? uploadedAt, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
