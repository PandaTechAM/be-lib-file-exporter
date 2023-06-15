## File Exporter
Exports given data into csv, xls, xlsx and pdf formats.

### Structure

Package consists of static `FileExporter` class which contains all needed calls to export given data into supported types defined inside `ExportType` enum.

> Supported formats are: CSV, XLS, XLSX and PDF.

### Usage

Install package `PandaTech.FileExporter` from Nexus.

##### Service
Implement your service which will use the `PandaTech.FileExporter` package.
```C#
public class Service
{
    public byte[] XlsXlsxArray(List<string> list)
    {
        return FileExporter.ToExcelArray(GetDtos(list));
    }

    public byte[] CsvArray(List<string> list)
    {
        return FileExporter.ToCsvArray(GetDtos(list));
    }

    public byte[] PdfArray(List<string> list)
    {
        return FileExporter.ToExcelArray(GetDtos(list));
    }
}
```

##### Controller
Use service calls to export returned byte[] data into given format from `ExportType` enum.
```C#
[HttpGet("export-to")]
public IActionResult ExportTo([FromQuery] ExportType exportType, [FromQuery] List<string> data)
{
    try
    {
        return exportType switch
        {
            ExportType.XLSX => File(_service.XlsXlsxArray(data), MimeTypes.XLSX, $"Export.xlsx"),
            ExportType.XLS => File(_service.XlsXlsxArray(data), MimeTypes.XLS, $"Export.xls"),
            ExportType.CSV => File(_service.CsvArray(data), MimeTypes.CSV, $"Export.csv"),
            ExportType.PDF => File(_service.PdfArray(data), MimeTypes.PDF, $"Export.pdf"),
            _ => NoContent() 
        };
    }
    catch (Exception)
    {
        return BadRequest();
    }
}
```