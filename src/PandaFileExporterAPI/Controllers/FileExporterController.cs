using Microsoft.AspNetCore.Mvc;
using PandaFileExporter;
using PandaFileExporterAPI.Context;
using PdfSharpCore;

namespace PandaFileExporterAPI.Controllers
{
    [ApiController]
    [Route("api/")]
    public class FileExporterController : Controller
    {
        private readonly ApiDbContext _context;

        public FileExporterController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpPost("fill-database")]
        public IActionResult FillDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Dummies.AddRange(new List<DummyTable>
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
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("export-csv")]
        public IActionResult ExportCsv()
        {
            var exportData = _context.Dummies.AsQueryable().ToDataTable().ToCsv();

            if (exportData.Length > (10 * 1024 * 1024))
            {
                exportData = exportData.ToZip($"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.csv");
                return File(exportData, MimeTypes.ZIP, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.zip");
            }

            return File(exportData,MimeTypes.CSV, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.csv");
        }

        [HttpGet("export-xlsx")]
        public IActionResult ExportXlsx()
        {
            var exportData = _context.Dummies.ToDataTable().ToXlsx();

            if (exportData.Length > (10 * 1024 * 1024))
            {
                exportData = ZipExtensions.ToZip(exportData , $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.xlsx");
                return File(exportData, MimeTypes.ZIP, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.zip");
            }

            return File(exportData,MimeTypes.XLSX, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.xlsx");
        }

        [HttpGet("export-pdf")]
        public IActionResult ExportPdf(bool headersOnEachPage = false, PageSize pageSize = PageSize.A4, PageOrientation pageOrientation = PageOrientation.Portrait)
        {
            var exportData = _context.Dummies.ToDataTable().ToPdf(headersOnEachPage, pageSize,  pageOrientation);

            if (exportData.Length > (10 * 1024 * 1024))
            {
                exportData = exportData.ToZip($"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.pdf");
                return File(exportData, MimeTypes.ZIP, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.zip");
            }

            return File(exportData, MimeTypes.PDF, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.pdf");
        }
    }
}
