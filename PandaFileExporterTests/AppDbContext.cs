using ExcelExporter;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace PandaFileExporterTests
{
    [DisplayName("DB Model")]
    [CustomDisplayName("Model Custom Name")]
    public class DbModel
    {
        [DisplayName("Model Id")]
        public int Id { get; set; }
        [DisplayName("Model Name")]
        public string Name { get; set; } = null!;
    }

    [CustomDisplayName("DTO Custom Name")]
    public class DtoModel
    {
        [CustomDisplayName("DTO Id")]
        public int Id { get; set; }
        [CustomDisplayName("DTO DbModels")]
        public List<DbModel> DbModels { get; set; } = null!;
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
