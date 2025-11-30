using FileExporter.Demo.Models;
using FileExporter.Enums;
using FileExporter.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FileExporter.Demo;

public static class Endpoints
{
   public static WebApplication MapDemoEndpoints(this WebApplication app)
   {
      // 1) Small dataset
      app.MapGet("/export/dummy",
         ([FromQuery] ExportFormat format) =>
         {
            var data = new List<DummyTable>
            {
               new()
               {
                  Id = 1,
                  RelatedId = 10,
                  Name = "First",
                  Comment = "Hello",
                  CreationDate = DateTime.UtcNow.AddDays(-1),
                  ExpirationDate = DateTime.UtcNow.AddDays(10),
                  Dto = null
               },
               new()
               {
                  Id = 2,
                  RelatedId = 20,
                  Name = "Second",
                  Comment = "World",
                  CreationDate = DateTime.UtcNow.AddDays(-2),
                  ExpirationDate = DateTime.UtcNow.AddDays(5),
                  Dto = "Custom DTO"
               }
            };

            var exportFile = data.ToFileFormat(format);

            return exportFile.ToFileResult();
         });

// 2) >1M rows: request Xlsx to check fallback to CSV
      app.MapGet("/export/over-million",
         ([FromQuery] ExportFormat format) =>
         {
            const int rowCount = 1_000_010;

            var data = Enumerable
                       .Range(1, rowCount)
                       .Select(i => new DummyTable
                       {
                          Id = i,
                          RelatedId = i % 100,
                          Name = $"Row {i}",
                          Comment = $"Row {i} demo.",
                          CreationDate = DateTime.UtcNow.AddMinutes(-i),
                          ExpirationDate = DateTime.UtcNow.AddDays(i % 365),
                          Dto = null
                       });

            var exportFile = data.ToFileFormat(format);

            return exportFile.ToFileResult();
         });

// 3) Wide / many columns, varying text lengths
      app.MapGet("/export/wide",
         ([FromQuery] ExportFormat format) =>
         {
            var data = new List<WideRow>();

            for (var i = 1; i <= 100; i++)
            {
               data.Add(new WideRow
               {
                  Id = i,
                  ShortText = $"Short {i}",
                  MediumText = new string('M', 20 + (i % 10)),
                  LongText = new string('L', 40 + (i % 15)),
                  VeryLongText = new string('V', 80 + (i % 20)),
                  HugeText = new string('H', 200 + (i % 30)),
                  Amount = i * 1.23m,
                  LargeAmount = i * 12345.6789m,
                  CreatedAt = DateTime.UtcNow.AddDays(-i),
                  UpdatedAt = i % 2 == 0 ? DateTime.UtcNow.AddDays(-i / 2) : null
               });
            }

            var exportFile = data.ToFileFormat(format);


            return exportFile.ToFileResult();
         });

      return app;
   }
}