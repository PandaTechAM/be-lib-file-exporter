using DocumentFormat.OpenXml.Office.MetaAttributes;
using Microsoft.EntityFrameworkCore;
using PandaFileExporterAPI.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("api_connection")));

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
context.Database.EnsureDeleted();
context.Database.EnsureCreated();

context.Dummies.AddRange(new List<DummyTable>
{
    new DummyTable { Id = 1, Name = "Test 1" },
    new DummyTable { Id = 2, Name = "Test 2" },
    new DummyTable { Id = 3, Name = "Test 3" },
    new DummyTable { Id = 4, Name = "Test 4" },
    new DummyTable { Id = 5, Name = "Test 5" },
    new DummyTable { Id = 6, Name = "Test 6" },
    new DummyTable { Id = 7, Name = "Test 7" },
    new DummyTable { Id = 8, Name = "Test 8" },
    new DummyTable { Id = 9, Name = "Test 9" },
});
context.SaveChanges();

app.Run();