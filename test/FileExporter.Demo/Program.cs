using FileExporter.Demo.ExportRules;
using FileExporter.Demo.Models;
using FileExporter.Enums;
using FileExporter.Rules;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (!app.Environment.IsProduction())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/export-dummy-csv",
   ([FromQuery] ExportType exportType) =>
   {
      // some demo data
      var data = new List<DummyTable>
      {
         new()
         {
            Id = 1,
            RelatedId = 10,
            Name = "First",
            Comment = "Hello"
         },
         new()
         {
            Id = 2,
            RelatedId = 20,
            Name = "Second",
            Comment = "World"
         }
      };

      var rule = new DummyExportRule();
      var exportFile = rule.ToFileFormat(data, exportType);

      // Minimal API: return FileResult with correct headers
      return TypedResults.File(exportFile.Data, exportFile.Type, exportFile.Name);
   });

app.Run();