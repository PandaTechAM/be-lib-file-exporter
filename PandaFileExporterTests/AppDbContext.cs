using Microsoft.EntityFrameworkCore;

namespace PandaFileExporterTests
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Model> Models { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
