using Microsoft.AspNetCore.Mvc;
using FileExporter;
using FileExporter.Demo.Context;
using PdfSharpCore;

namespace FileExporter.Demo.Controllers
{
    [ApiController]
    [Route("api/")]
    public class FileExporterController(ApiDbContext context) : Controller
    {
        [HttpPost("fill-database")]
        public IActionResult FillDatabase()
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Dummies.AddRange(new List<DummyTable>
                {
                    new() { Id = 1, RelatedId = 18, Name = "Բարև բոլորին 1", Description = "Test this out, it's OK" },
                    new() { Id = 2, RelatedId = 18, Name = "Բարև բոլորին 2", Description = "Test this out, it's OK" },
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

            context.SaveChanges();

            return Ok();
        }

        [HttpGet("export-csv")]
        public IActionResult ExportCsv()
        {
            var exportData = context.Dummies.AsEnumerable().ToCsv();

            return exportData.ToFile();
        }
        
        [HttpGet("export-csv-empty")]
        public IActionResult ExportEmptyCsv()
        {
            var exportData = context.EmptyTable.AsEnumerable().ToCsv();

            return exportData.ToFile();
        }

        [HttpGet("export-xlsx")]
        public IActionResult ExportXlsx()
        {
            var exportData = context.Dummies.AsEnumerable().ToXlsx();

            return exportData.ToFile();
        }
        
        [HttpGet("export-xlsx-empty")]
        public IActionResult ExportEmptyXlsx()
        {
            var exportData = context.EmptyTable.AsEnumerable().ToXlsx();

            return exportData.ToFile();
        }

        [HttpGet("export-pdf")]
        public IActionResult ExportPdf(bool headersOnEachPage = true, string fontName = "arial", int fontSize = 10, PageSize pageSize = PageSize.A4, PageOrientation pageOrientation = PageOrientation.Portrait)
        {
            var exportData = context.Dummies.AsEnumerable().ToPdf(headersOnEachPage, fontName, fontSize, pageSize, pageOrientation);

            return exportData.ToFile();
        }
        
        [HttpGet("export-pdf-empty")]
        public IActionResult ExportEmptyPdf(bool headersOnEachPage = true, string fontName = "arial", int fontSize = 10, PageSize pageSize = PageSize.A4, PageOrientation pageOrientation = PageOrientation.Portrait)
        {
            var exportData = context.EmptyTable.AsEnumerable().ToPdf(headersOnEachPage, fontName, fontSize, pageSize, pageOrientation);

            return exportData.ToFile();
        }
    }
}
