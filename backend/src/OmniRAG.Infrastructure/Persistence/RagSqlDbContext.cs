using Microsoft.EntityFrameworkCore;

namespace OmniRAG.Infrastructure.Persistence
{
    public class RagSqlDbContext : DbContext
    {
        public RagSqlDbContext(DbContextOptions<RagSqlDbContext> options) : base(options)
        {
        }

        public DbSet<DocumentMetadata> DocumentMetadata { get; set; } = default!;
    }
}
