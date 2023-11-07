using Microsoft.EntityFrameworkCore;
using PandaFileExporterAPI.Context;

var builder = WebApplication.CreateBuilder(args);

// Change connection string to real database from server if you want
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseInMemoryDatabase("Test"));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetService<ApiDbContext>();
context?.Database.EnsureDeleted();
context?.Database.EnsureCreated();

// Remove this part if you want to work with real database from server
context?.Dummies.AddRange(new List<DummyTable>
{
    new DummyTable { Id = 1, Name = "Բարեւ բոլորին 1", Description = "Test this out, it's OK" },
    new DummyTable { Id = 2, Name = "Բարեւ բոլորին 2", Description = "Test this out, it's OK" },
    new DummyTable { Id = 3, Name = "Բարեւ բոլորին 3", Description = "Test this out, it's OK" },
    new DummyTable { Id = 4, Name = "Բարեւ բոլորին 4", Description = "Test this out, it's OK" },
    new DummyTable { Id = 5, Name = "Բարեւ բոլորին 5", Description = "Test this out, it's OK" },
    new DummyTable { Id = 6, Name = "Բարեւ բոլորին 6", Description = "Test this out, it's OK" },
    new DummyTable { Id = 7, Name = "Բարեւ բոլորին 7", Description = "Test this out, it's OK" },
    new DummyTable { Id = 8, Name = "Բարեւ բոլորին 8", Description = "Test this out, it's OK" },
    new DummyTable { Id = 9, Name = "Բարեւ բոլորին 9", Description = "Test this out, it's OK" },
    new DummyTable { Id = 10, Name = "Բարեւ բոլորին 10", Description = "Test this out, it's OK" },
    new DummyTable { Id = 11, Name = "Բարեւ բոլորին 11", Description = "Test this out, it's OK" },
    new DummyTable { Id = 12, Name = "Բարեւ բոլորին 12", Description = "Test this out, it's OK" },
    new DummyTable { Id = 13, Name = "Բարեւ բոլորին 13", Description = "Test this out, it's OK" },
    new DummyTable { Id = 14, Name = "Բարեւ բոլորին 14", Description = "Test this out, it's OK" },
    new DummyTable { Id = 15, Name = "Բարեւ բոլորին 15", Description = "Test this out, it's OK" },
    new DummyTable { Id = 16, Name = "Բարեւ բոլորին 16", Description = "Test this out, it's OK" },
    new DummyTable { Id = 17, Name = "Բարեւ բոլորին 17", Description = "Test this out, it's OK" },
    new DummyTable { Id = 18, Name = "Բարեւ բոլորին 18", Description = "Test this out, it's OK" },
    new DummyTable { Id = 19, Name = "Բարեւ բոլորին 19", Description = "Test this out, it's OK" },
});
context?.SaveChanges();

app.Run();