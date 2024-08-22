# FileExporter

FileExporter is a lightweight C# library designed to simplify file export operations in your .NET applications. With support for exporting data to CSV, Excel (XLSX), and PDF formats, FileExporter provides an intuitive interface for developers to quickly generate and download files.

## Features

- **Easy Exporting**: Simply call `ToCsv()`, `ToXlsx()`, or `ToPdf()` on your data collection to export to the desired format.
- **Automatic Splitting**: Handles large datasets gracefully by automatically splitting files if the maximum line count or file size is exceeded, then zipping them for easy download.
- **Flexible Configuration**: Customize export settings such as column headers, delimiter, and more to suit your needs.
- **Effortless Integration**: Seamlessly integrate FileExporter into your existing .NET projects with minimal setup.
- **Helper Extension Methods**: Instead of using `ToCsv()`, `ToXlsx()`, or `ToPdf()` directly, you can use `ToFileFormat(ExportType.Excel)` to export data to the desired format.

## Installation

You can install FileExporter via NuGet Package Manager:

```bash
Install-Package FileExporter
```

## Usage
Here's a quick example of how to use FileExporter to export data to a CSV file:

```csharp
using FileExporter;

// Define your data
var data = new List<MyDataClass>
{
    new MyDataClass { Name = "John Doe", Age = 30, Email = "john@example.com" },
    new MyDataClass { Name = "Jane Smith", Age = 25, Email = "jane@example.com" }
};

// Export data to CSV
var exportedFile = data.ToCsv().ToFile();

// Return the exported file to the caller
return exportedFile;
```
Started from release 3.3.0 it's possible to export data by fluent rules.

First of all you need to create `ExportRule` for your model.
Then from constructor call `GenerateRules()` which will automatically create default rules for you related to given model.
If you need custom setup, then call `RuleFor()` method and setup custom rules for your model properties. 
Here is a quick demo to show how.

This is a model sample:
```csharp
public class FileData
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Comment { get; set; }
}
```

This is an export rule sample:
```csharp
namespace FileExporter.Tests.ExportRuleTests;

public class FileDataExportRule : ExportRule<FileData>
{
    public FileDataExportRule() : base("File Data")
    {
        GenerateRules();
        
        RuleFor(x => x.Id);
        RuleFor(x => x.Name);
        RuleFor(x => x.Description).WithDefaultValue("Default text here");
        RuleFor(x => x.CreatedAt).WriteToColumn("Creation date")
            .WithDefaultValue("22/02/2022");
        RuleFor(x => x.Comment);
    }
}
```
in case of wrong property setup you will get `InvalidPropertyNameException` with message.

Here is a sample of controller:
```csharp
namespace FileExporter.Demo.Controllers
{
    [ApiController]
    [Route("api/")]
    public class FileDataExportController(ApiDbContext context) : Controller
    {
        [HttpGet("export-xlsx-via-rules")]
        public IActionResult ExportXlsxViaRules()
        {
            var exportData = context.FileData.ToList();

            var rule = new FileDataExportRule();
            
            return rule.ToCsv(exportData).ToFile();
        }
    }
}
```

You can also export data to Excel (XLSX) or PDF formats by calling ToXlsx() or ToPdf() respectively.

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## License

This project is licensed under the MIT License.