namespace OmniRAG.Application.Dtos;

public sealed record UserProfileDto(int Id, string AzureObjectId, string Name, string Email, DateTime CreatedAt, DateTime LastLoginAt, bool IsProfileCompleted);
