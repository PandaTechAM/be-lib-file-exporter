using FileExporter.Demo.Models;
using FileExporter.Dtos;
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
            async ([FromQuery] ExportFormat format) =>
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

                var exportFile = await data.ToFileFormatAsync(format);

                return exportFile.ToFileResult();
            });

// 2) >1M rows: request Xlsx to check fallback to CSV
        app.MapGet("/export/over-million",
            async ([FromQuery] ExportFormat format) =>
            {
                const int rowCount = 1_100_000;

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

                var exportFile = await data.ToFileFormatAsync(format);

                return exportFile.ToFileResult();
            });

// 3) Wide / many columns, varying text lengths
        app.MapGet("/export/wide",
            async ([FromQuery] ExportFormat format) =>
            {
                var data = new List<WideRow>();

                for (var i = 1; i <= 100; i++)
                {
                    data.Add(new WideRow
                    {
                        Id = i,
                        ShortText = $"Short {i}",
                        MediumText = new string('M', 20 + i % 10),
                        LongText = new string('L', 40 + i % 15),
                        VeryLongText = new string('V', 80 + i % 20),
                        HugeText = new string('H', 200 + i % 30),
                        Amount = i * 1.23m,
                        LargeAmount = i * 12345.6789m,
                        CreatedAt = DateTime.UtcNow.AddDays(-i),
                        UpdatedAt = i % 2 == 0 ? DateTime.UtcNow.AddDays(-i / 2) : null
                    });
                }

                var exportFile = await data.ToFileFormatAsync(format);


                return exportFile.ToFileResult();
            });

// 4) Per-request overrides: localized headers, localized enum labels, explicit file and sheet names
        app.MapGet("/export/localized",
            async ([FromQuery] ExportFormat format) =>
            {
                var data = new List<DummyTable>
                {
                    new()
                    {
                        Id = 1,
                        RelatedId = 10,
                        Name = "First",
                        Comment = "Hello",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10),
                        DefaultEnum = DefaultEnum.Vazgen
                    },
                    new()
                    {
                        Id = 2,
                        RelatedId = 20,
                        Name = "Second",
                        Comment = "World",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(5),
                        DefaultEnum = (DefaultEnum)7 // undefined member: renders as "7", not "7 - "
                    }
                };

                var labels = new Dictionary<DefaultEnum, string>
                {
                    [DefaultEnum.Vardan] = "Erste Option",
                    [DefaultEnum.Vazgen] = "Zweite Option"
                };

                var options = new ExportOptions
                {
                    FileName = "Bestellungen {DateTime}",
                    SheetName = "Bestellungen",
                    ColumnHeaders = new Dictionary<string, string>
                    {
                        [nameof(DummyTable.Id)] = "Nummer",
                        [nameof(DummyTable.Name)] = "Bezeichnung",
                        [nameof(DummyTable.DefaultEnum)] = "Status"
                    },
                    EnumLabelResolver = value =>
                        value is DefaultEnum e && labels.TryGetValue(e, out var label) ? label : string.Empty
                };

                var exportFile = await data.ToFileFormatAsync(format, options);

                return exportFile.ToFileResult();
            });

// 5) Latin, Cyrillic and Armenian with the dram sign, glyphs no embedded font has, line breaks and an unbreakable token
        app.MapGet("/export/multilingual",
            async ([FromQuery] ExportFormat format) =>
            {
                var data = new List<DummyTable>
                {
                    new()
                    {
                        Id = 1,
                        Name = "Latin",
                        Comment = "The quick brown fox jumps over the lazy dog",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Cyrillic",
                        Comment = "Съешь же ещё этих мягких французских булок",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        Id = 3,
                        Name = "Armenian",
                        Comment = "Բարեւ աշխարհ, 12 500 ֏ (and the rare ՠ ֈ ֍ ֎)",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        Id = 4,
                        Name = "Missing glyphs",
                        Comment = "Lari ₾ and CJK 中文 draw as empty boxes; the export never fails",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        Id = 5,
                        Name = "Line breaks",
                        Comment = "First line\r\nSecond line\nThird line",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        Id = 6,
                        Name = "Long token",
                        Comment = new string('W', 300),
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(10)
                    }
                };

                var exportFile = await data.ToFileFormatAsync(format);

                return exportFile.ToFileResult();
            });

// 6) One row past the PDF limit: Pdf throws ExportRowLimitExceededException, Csv and Xlsx export every row
        app.MapGet("/export/over-pdf-limit",
            async ([FromQuery] ExportFormat format) =>
            {
                var data = Enumerable
                    .Range(1, 100_001)
                    .Select(i => new DummyTable
                    {
                        Id = i,
                        Name = $"Row {i}",
                        CreationDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow
                    });

                var exportFile = await data.ToFileFormatAsync(format);

                return exportFile.ToFileResult();
            });

        return app;
    }
}
