using System;

namespace OmniRAG.Infrastructure.Persistence;

public class UserProfile
{
    public int Id { get; set; }
    public string AzureObjectId { get; set; } = string.Empty; // external AD user id (oid)
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public bool IsProfileCompleted { get; set; } = false;
}
