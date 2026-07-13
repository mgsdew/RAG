namespace OmniRAG.Infrastructure.Persistence
{
    public class DocumentMetadata
    {
        public int Id { get; set; }
        public string DocumentId { get; set; } = string.Empty; // Maps exactly to Cosmos DB Partition/ID
        public string FileName { get; set; } = string.Empty;
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }
}
