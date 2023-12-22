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
    new() { Id = 1, RelatedId = 18, Name = "Բարև բոլորին 1", Description = "Test this out, it's OK" , Max = 100, NullableList = new List<string> { "1", "2", "3" }},
    new() { Id = 2, RelatedId = 18, Name = "Բարև բոլորին 2", Description = "Test this out, it's OK" , EnumArray = new [] { MyEnum.A, MyEnum.B, MyEnum.C, MyEnum.D }},
    new() { Id = 3, Name = "Բարև բոլորին 3", Description = "Test this out, it's OK" },
    new() { Id = 4, Name = "Բարև բոլորին 4", Description = "Test this out, it's OK" },
    new() { Id = 5, RelatedId = 18, Name = "Բարև բոլորին 5", Description = "Test this out, it's OK" },
    new() { Id = 6, Name = "Բարև բոլորին 6", Description = "Test this out, it's OK" },
    new() { Id = 7, Name = "Բարև բոլորին 7", Description = "Test this out, it's OK" },
    new() { Id = 8, Name = "Բարև բոլորին 8", Description = "Test this out, it's OK" },
    new() { Id = 9, Name = "Բարև բոլորին 9", Description = "Test this out, it's OK" },
    new() { Id = 10, RelatedId = 18, Name = "Բարև բոլորին 10", Description = "Test this out, it's OK" },
    new() { Id = 11, Name = "Բարև բոլորին 11", Description = "Test this out, it's OK" },
    new() { Id = 12, Name = "Բարև բոլորին 12", Description = "Test this out, it's OK" },
    new() { Id = 13, Name = "Բարև բոլորին 13", Description = "Test this out, it's OK" },
    new() { Id = 14, RelatedId = 18, Name = "Բարև բոլորին 14", Description = "Test this out, it's OK" },
    new() { Id = 15, Name = "Բարև բոլորին 15", Description = "Test this out, it's OK" },
    new() { Id = 16, Name = "Բարև բոլորին 16", Description = "Test this out, it's OK" },
    new() { Id = 17, RelatedId = 18, Name = "Բարև բոլորին 17", Description = "Test this out, it's OK" },
    new() { Id = 18, Name = "Բարև բոլորին 18", Description = "Test this out, it's OK" },
    new() { Id = 19, Name = "Բարև բոլորին 19", Description = "Test this out, it's OK" },
});

for (int x = 20; x < 1000; x++)
    context?.Dummies.Add(new() { Id = x, Name = "Բարև բոլորին 19", Description = "Test this out, it's OK" });
context?.SaveChanges();

app.Run();