using ExcelExporter;
using Microsoft.AspNetCore.Mvc;
using PandaFileExporter;
using PandaFileExporterAPI.Context;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;

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
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("export-csv")]
        public IActionResult ExportCsv()
        {
            //var exportData = FileExporter.ExportToXlsx(_context.Dummies);
            var exportData = FileExporter.ToCsvArray(_context.Dummies.ToList());

            if (exportData.Length > (10 * 1024 * 1024))
            {
                exportData = FileExporter.ToZipArray(_context.Dummies);
                return File(exportData, MimeTypes.ZIP, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.zip");

            }

            return File(exportData, MimeTypes.CSV, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.csv");
        }

        [HttpGet("export-xlsx")]
        public IActionResult ExportXlsx()
        {
            //var exportData = FileExporter.ExportToXlsx(_context.Dummies);
            var exportData = FileExporter.ToExcelArray(_context.Dummies.ToList());

            if (exportData.Length > (10 * 1024 * 1024))
            {
                exportData = FileExporter.ToZipArray(_context.Dummies);
                return File(exportData, MimeTypes.ZIP, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.zip");

            }

            return File(exportData, MimeTypes.XLSX, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.xlsx");
        }

        [HttpGet("export-pdf")]
        public IActionResult ExportPdf()
        {
            //var exportData = FileExporter.ExportToXlsx(_context.Dummies);
            var exportData = FileExporter.ToPdfArray(_context.Dummies);

            if (exportData.Length > (10 * 1024 * 1024))
            {
                exportData = FileExporter.ToZipArray(_context.Dummies);
                return File(exportData, MimeTypes.ZIP, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.zip");

            }

            return File(exportData, MimeTypes.PDF, $"Export_{_context.Dummies.FirstOrDefault()?.GetType().Name}.pdf");
        }
    }
}
