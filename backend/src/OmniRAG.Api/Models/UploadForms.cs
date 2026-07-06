using System.ComponentModel.DataAnnotations;

namespace OmniRAG.Api.Models;

public sealed class UploadCreateForm
{
    [Required]
    public IFormFile File { get; set; } = default!;

    public string? FileName { get; set; }

    public DateTime? UploadedAt { get; set; }
}

public sealed class UploadUpdateForm
{
    public IFormFile? File { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public DateTime? UploadedAt { get; set; }
}
