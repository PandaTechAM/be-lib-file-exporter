using FileExporter.Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace FileExporter.Demo.Context;

public class ApiDbContext : DbContext
{
   public DbSet<DummyTable> Dummies { get; set; }
   public DbSet<EmptyTable> EmptyTable { get; set; }

   public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
   {
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
   }
}