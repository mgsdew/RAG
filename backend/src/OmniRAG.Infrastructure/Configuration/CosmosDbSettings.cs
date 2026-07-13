namespace OmniRAG.Infrastructure.Configuration
{
    public class CosmosDbSettings
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}
