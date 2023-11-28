using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using PandaFileExporter;

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
        [DisplayName("DTO Not Nullable List")]
        public List<string> ListNotNullable { get; set; } = null!;
        [DisplayName("DTO Nullable List")]
        public List<string>? ListNullable { get; set; }
        [DisplayName("DTO Enum")]
        public MyEnum Enum { get; set; }
        [DisplayName("DTO Enum List")]
        public MyEnum[] EnumArray { get; set; } = new MyEnum[4] { MyEnum.A, MyEnum.B, MyEnum.C, MyEnum.D };
    }

    public enum MyEnum
    {
        A, B, C, D
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
