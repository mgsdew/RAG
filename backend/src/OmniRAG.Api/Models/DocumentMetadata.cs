namespace OmniRAG.Api.Models
{
    public class DocumentMetadata
    {
        public int Id { get; set; }
        public string DocumentId { get; set; } // Maps exactly to Cosmos DB Partition/ID
        public string FileName { get; set; }
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }
    }
}
