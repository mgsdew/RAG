using OmniRAG.Domain.Entities;

namespace OmniRAG.Application.Abstractions;

public interface IUploadRepository
{
    Task<IReadOnlyCollection<UploadRecord>> GetAllAsync(CancellationToken cancellationToken);

    Task<UploadRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(UploadRecord record, CancellationToken cancellationToken);

    Task UpdateAsync(UploadRecord record, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
