using System.Collections.Concurrent;
using OmniRAG.Application.Abstractions;
using OmniRAG.Domain.Entities;

namespace OmniRAG.Infrastructure.Persistence;

public sealed class InMemoryUploadRepository : IUploadRepository
{
    private readonly ConcurrentDictionary<Guid, UploadRecord> _records = new();

    public Task AddAsync(UploadRecord record, CancellationToken cancellationToken)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _records.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<UploadRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<UploadRecord> records = _records.Values.OrderByDescending(record => record.UploadedAt).ToArray();
        return Task.FromResult(records);
    }

    public Task<UploadRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _records.TryGetValue(id, out var record);
        return Task.FromResult(record);
    }

    public Task UpdateAsync(UploadRecord record, CancellationToken cancellationToken)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }
}
