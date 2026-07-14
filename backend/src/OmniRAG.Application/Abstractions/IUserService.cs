using OmniRAG.Application.Dtos;

namespace OmniRAG.Application.Abstractions;

public interface IUserService
{
    Task<UserProfileDto> CreateAsync(string azureObjectId, string name, string email, CancellationToken cancellationToken);

    Task<UserProfileDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserProfileDto>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<UserProfileDto?> GetByAzureObjectIdAsync(string azureObjectId, CancellationToken cancellationToken);

    Task<(UserProfileDto Profile, bool IsFirstTime)> GetOrCreateByAzureObjectIdAsync(string azureObjectId, string name, string email, CancellationToken cancellationToken);
}
