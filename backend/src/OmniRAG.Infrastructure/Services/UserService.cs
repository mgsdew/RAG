using Microsoft.EntityFrameworkCore;
using OmniRAG.Application.Abstractions;
using OmniRAG.Application.Dtos;
using OmniRAG.Infrastructure.Persistence;

namespace OmniRAG.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly RagSqlDbContext _db;

    public UserService(RagSqlDbContext db)
    {
        _db = db;
    }

    public async Task<UserProfileDto> CreateAsync(string azureObjectId, string name, string email, CancellationToken cancellationToken)
    {
        var entity = new UserProfile
        {
            AzureObjectId = azureObjectId,
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            IsProfileCompleted = false
        };

        _db.UserProfiles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new UserProfileDto(entity.Id, entity.AzureObjectId, entity.Name, entity.Email, entity.CreatedAt, entity.LastLoginAt, entity.IsProfileCompleted);
    }

    public async Task<UserProfileDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var e = await _db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return e is null ? null : new UserProfileDto(e.Id, e.AzureObjectId, e.Name, e.Email, e.CreatedAt, e.LastLoginAt, e.IsProfileCompleted);
    }

    public async Task<UserProfileDto?> GetByAzureObjectIdAsync(string azureObjectId, CancellationToken cancellationToken)
    {
        var e = await _db.UserProfiles.FirstOrDefaultAsync(u => u.AzureObjectId == azureObjectId, cancellationToken);
        return e is null ? null : new UserProfileDto(e.Id, e.AzureObjectId, e.Name, e.Email, e.CreatedAt, e.LastLoginAt, e.IsProfileCompleted);
    }

    public async Task<(UserProfileDto Profile, bool IsFirstTime)> GetOrCreateByAzureObjectIdAsync(string azureObjectId, string name, string email, CancellationToken cancellationToken)
    {
        var e = await _db.UserProfiles.FirstOrDefaultAsync(u => u.AzureObjectId == azureObjectId, cancellationToken);
        var isFirstTime = false;

        if (e == null)
        {
            isFirstTime = true;
            e = new UserProfile
            {
                AzureObjectId = azureObjectId,
                Name = name,
                Email = email ?? string.Empty,
                CreatedAt = DateTime.UtcNow + TimeSpan.FromHours(12),
                LastLoginAt = DateTime.UtcNow + TimeSpan.FromHours(12),
                IsProfileCompleted = false
            };

            _db.UserProfiles.Add(e);
        }
        else
        {
            e.LastLoginAt = DateTime.UtcNow + TimeSpan.FromHours(12);
            _db.UserProfiles.Update(e);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new UserProfileDto(e.Id, e.AzureObjectId, e.Name, e.Email, e.CreatedAt, e.LastLoginAt, e.IsProfileCompleted);
        return (dto, isFirstTime);
    }

    public async Task<IReadOnlyCollection<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var list = await _db.UserProfiles.OrderByDescending(u => u.CreatedAt).ToListAsync(cancellationToken);
        return list.Select(e => new UserProfileDto(e.Id, e.AzureObjectId, e.Name, e.Email, e.CreatedAt, e.LastLoginAt, e.IsProfileCompleted)).ToArray();
    }
}
