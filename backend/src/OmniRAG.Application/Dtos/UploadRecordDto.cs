namespace OmniRAG.Application.Dtos;

public sealed record UploadRecordDto(
    Guid Id,
    string FileName,
    string ContentType,
    DateTime UploadedAt);
