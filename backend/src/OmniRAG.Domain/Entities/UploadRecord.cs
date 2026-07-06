namespace OmniRAG.Domain.Entities;

public sealed class UploadRecord
{
    private static readonly string[] AllowedExtensions = [".csv", ".xls", ".xlsx"];

    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }

    private UploadRecord()
    {
    }

    private UploadRecord(Guid id, string fileName, string contentType, string storageKey, DateTime uploadedAt)
    {
        Id = id;
        SetState(fileName, contentType, storageKey, uploadedAt);
    }

    public static UploadRecord Create(string fileName, string contentType, string storageKey, DateTime? uploadedAt = null)
    {
        Validate(fileName, contentType, storageKey);
        return new UploadRecord(Guid.NewGuid(), Normalize(fileName), Normalize(contentType), Normalize(storageKey), (uploadedAt ?? DateTime.UtcNow).Date);
    }

    public void Update(string fileName, string contentType, DateTime? uploadedAt = null)
    {
        Validate(fileName, contentType, StorageKey);
        SetState(fileName, contentType, StorageKey, uploadedAt ?? UploadedAt);
    }

    public static bool IsSupportedFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) && AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private void SetState(string fileName, string contentType, string storageKey, DateTime uploadedAt)
    {
        FileName = Normalize(fileName);
        ContentType = Normalize(contentType);
        StorageKey = Normalize(storageKey);
        UploadedAt = uploadedAt.Date;
    }

    private static void Validate(string fileName, string contentType, string storageKey)
    {
        if (!IsSupportedFileName(fileName))
        {
            throw new ArgumentException("Only CSV, XLS, and XLSX files are supported.", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }
    }

    private static string Normalize(string value) => value.Trim();
}
