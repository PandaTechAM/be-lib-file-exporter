# FileExporter

FileExporter is a lightweight C# library that simplifies file export operations in .NET applications. With support for exporting data to CSV, Excel (XLSX), and PDF formats, FileExporter provides an intuitive interface for developers to quickly generate and download files.

## Features

- **Easy Exporting**: Simply call ToCsv(), ToXlsx(), or ToPdf() on your data collection to export to the desired format.
- **Automatic Splitting**: Handles large datasets by automatically splitting files if the maximum line count or file size is exceeded, then zipping them for easy download.
- **Flexible Configuration**: Customize export settings such as column headers, delimiter, and more to suit your needs.
- **Effortless Integration**: Seamlessly integrate FileExporter into your existing .NET projects with minimal setup.
- **Helper Extension Methods**: Use ToFileFormat(ExportType.Excel) as an alternative to directly calling ToCsv(), ToXlsx(), or ToPdf().

## Installation

You can install FileExporter via NuGet Package Manager by running the following command:

```bash
Install-Package FileExporter
```

## Usage

Here's a quick example of how to use FileExporter to export data to a CSV file with old way which is still supported:

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
Starting from release 3.3.0, FileExporter supports exporting data using fluent rules.

### Fluent Rules Example

First, create an ExportRule for your model. In the constructor, call GenerateRules() to automatically create default rules based on the model. To customize the setup, use the RuleFor() method to configure specific rules for your model's properties. Here's a quick demonstration:

### Model Example:

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
### Export Rule Example:

This sample includes two constructors, one with a default name and one with a custom name.

```csharp
namespace FileExporter.Tests.ExportRuleTests;

public class FileDataExportRule : ExportRule<FileData>
{
    public FileDataExportRule()
    {
        GenerateRules();

        // Custom rules
        RuleFor(x => x.Description)
            .WriteToColumn("Description")
            .WithDefaultValue("Default text here");

        RuleFor(x => x.CreatedAt)
            .WriteToColumn("Creation date")
            .WithDefaultValue("22/08/2024");
    }
    
    // OR with custom name
    public FileDataExportRule() : base("File Data")
    {
        GenerateRules();
        
        // Custom rules
        RuleFor(x => x.Description)
            .WriteToColumn("Description")
            .WithDefaultValue("Default text here");

        RuleFor(x => x.CreatedAt)
            .WriteToColumn("Creation date")
            .WithDefaultValue("22/08/2024");
    }
}
```
If a property is incorrectly set up, an InvalidPropertyNameException will be thrown with a relevant message.

### Controller Example:

Here is an example of how to integrate FileExporter into a web API controller:

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
            // OR alternative solution with extension method
            return rule.ToFileFormat(exportData, ExportType.Xlsx).ToFile();
        }
    }
}
```
You can also export data to Excel (XLSX) or PDF formats by calling ToXlsx() or ToPdf() respectively.

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## License

This project is licensed under the MIT License.