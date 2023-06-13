using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace PandaFileExporterTests
{
    [DisplayName("DB Model")]
    public class DbModel
    {
        [DisplayName("Model Id")]
        public int Id { get; set; }
        [DisplayName("Model Name")]
        public string Name { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<DbModel> Models { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
