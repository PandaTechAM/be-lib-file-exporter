using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaFileExporterAPI.Context
{
    [DisplayName("Dummy Table")]
    public class DummyTable
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = "Created";
        public int Price { get; set; } = 50000;
        public int Count { get; set; } = 100;
        public string Description { get; set; } = "Test, test";
        [DisplayName("Creation Date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [DisplayName("Expiration Date")]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(10);
        public string? Comment { get; set; }
        public DateTime Version { get; set; } = DateTime.UtcNow;
        [NotMapped]
        public List<DateTime> Dates { get; set; } = new()
        {
            DateTime.Now,
            DateTime.Today,
            DateTime.UtcNow
        };
        [NotMapped]
        public List<string>? NullableList { get; set; } = null;
    }

    public class ApiDbContext : DbContext
    {
        public DbSet<DummyTable> Dummies { get; set; }

        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
