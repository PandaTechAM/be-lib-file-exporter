# PandaTech.FileExporter

High-performance CSV and XLSX exporting library for .NET 8+ with convention-based defaults, fluent configuration, async
streaming, multi-sheet support, and automatic compression.

## Installation

```bash
dotnet add package PandaTech.FileExporter
```

## Quick Start

### 1. Register in Program.cs

```csharp
using FileExporter.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Scan for ExportRule<T> configurations
builder.AddFileExporter(typeof(Program).Assembly);
```

### 2. Export Data (Zero Configuration)

```csharp
using FileExporter.Extensions;

var products = await db.Products.ToListAsync();

// CSV export with automatic file naming and formatting
var csvFile = await products.ToFileFormatAsync(ExportFormat.Csv);

// XLSX export with multi-sheet support for large datasets
var excelFile = await products.ToFileFormatAsync(ExportFormat.Xlsx);

// Return from minimal API
return csvFile.ToFileResult();
```

**That's it!** The library uses conventions to:

- Auto-detect property types and apply formatting
- Generate column headers from property names (e.g., `CreatedDate` → "Created Date")
- Apply sensible column widths
- Handle nulls, enums, dates, decimals automatically

## Custom Configuration

### Define Export Rules

```csharp
using FileExporter.Rules;
using FileExporter.Enums;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public ProductStatus Status { get; set; }
}

public class ProductExportRule : ExportRule<Product>
{
    public ProductExportRule()
    {
        // Custom file name (supports {DateTime} placeholder)
        WithName("Product Report {DateTime}");
        
        // Configure columns
        RuleFor(x => x.Id)
            .WriteToColumn("Product ID")
            .HasOrder(1);
        
        RuleFor(x => x.Name)
            .WriteToColumn("Product Name")
            .HasOrder(2)
            .HasWidth(30);
        
        RuleFor(x => x.Price)
            .WriteToColumn("Price (USD)")
            .HasFormat(ColumnFormatType.Currency)
            .HasPrecision(2)
            .HasOrder(3);
        
        RuleFor(x => x.CreatedDate)
            .WriteToColumn("Created")
            .HasFormat(ColumnFormatType.DateTime)
            .HasOrder(4);
        
        RuleFor(x => x.Status)
            .WriteToColumn("Status")
            .WithEnumFormat(EnumFormatMode.Name) // Int, Name, or MixedIntAndName
            .HasOrder(5);
    }
}
```

## Features

### Convention-Based Defaults

Without configuration, the library automatically:

| Property Type       | Format Applied     | Column Width    | Example Output      |
|---------------------|--------------------|-----------------|---------------------|
| `string`            | Text               | Based on header | "Product Name"      |
| `int`, `long`       | Integer            | 12              | 1234                |
| `decimal`, `double` | Decimal (2 places) | 12              | 99.99               |
| `DateTime`          | DateTime           | 19              | 2024-01-15 14:30:00 |
| `DateOnly`          | Date               | 12              | 2024-01-15          |
| `bool`              | Yes/No             | 8               | Yes                 |
| `enum`              | Mixed int + name   | Based on header | 1 - Active          |

### Column Configuration API

| Method                           | Description              | Example                                 |
|----------------------------------|--------------------------|-----------------------------------------|
| `WriteToColumn(string)`          | Set column header        | `.WriteToColumn("Full Name")`           |
| `HasOrder(int)`                  | Set column position      | `.HasOrder(1)`                          |
| `HasWidth(int)`                  | Set column width (chars) | `.HasWidth(25)`                         |
| `HasFormat(ColumnFormatType)`    | Set format type          | `.HasFormat(ColumnFormatType.Currency)` |
| `HasPrecision(int)`              | Set decimal places       | `.HasPrecision(4)`                      |
| `WithEnumFormat(EnumFormatMode)` | Enum display mode        | `.WithEnumFormat(EnumFormatMode.Name)`  |
| `WithDefaultValue(string)`       | Default for nulls        | `.WithDefaultValue("N/A")`              |
| `Transform(Func)`                | Custom transformation    | `.Transform(x => x?.ToUpper())`         |
| `Ignore()`                       | Exclude from export      | `.Ignore()`                             |

### Format Types

```csharp
public enum ColumnFormatType
{
    Default,      // Auto-detect from property type
    Text,         // Force as text
    Integer,      // Whole numbers
    Decimal,      // Fixed decimal places
    Currency,     // Currency formatting with symbol
    Percentage,   // Percentage with % symbol
    Date,         // Date only (yyyy-MM-dd)
    DateTime,     // Date and time
    Boolean       // Yes/No
}
```

### Enum Formatting

```csharp
public enum EnumFormatMode
{
    MixedIntAndName, // "1 - Active" (default)
    Int,             // "1"
    Name             // "Active"
}
```

## Advanced Features

### Multi-Sheet XLSX

Files with >1,048,575 rows automatically split into multiple sheets:

```csharp
var hugeDataset = await db.Orders.ToListAsync(); // 3 million rows

var file = await hugeDataset.ToFileFormatAsync(ExportFormat.Xlsx);
// Creates single .xlsx with 3 sheets: "Orders", "Orders_2", "Orders_3"
```

### Auto-Compression

Files >10MB automatically compress to ZIP:

```csharp
var largeExport = await data.ToFileFormatAsync(ExportFormat.Csv);
// If >10MB: returns "Report 2024-01-15.zip" containing "Report 2024-01-15.csv"
// If <10MB: returns "Report 2024-01-15.csv" directly
```

### Async Streaming

```csharp
IAsyncEnumerable<Product> GetProductsAsync()
{
    await foreach (var product in db.Products.AsAsyncEnumerable())
    {
        yield return product;
    }
}

var stream = GetProductsAsync();
var file = await stream.ToCsvAsync();
```

### Custom Transformations

```csharp
RuleFor(x => x.Email)
    .Transform(email => email?.Contains("@") == true 
        ? MaskEmail(email) 
        : "N/A");

RuleFor(x => x.Price)
    .Transform(price => price * 1.20m) // Add 20% markup
    .HasFormat(ColumnFormatType.Currency);

RuleFor(x => x.Tags)
    .Transform(tags => string.Join(", ", tags)); // List<string> → "tag1, tag2"
```

### Minimal API Integration

```csharp
app.MapGet("/export/products/csv", async (AppDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    var file = await products.ToFileFormatAsync(ExportFormat.Csv);
    return file.ToFileResult();
});

app.MapGet("/export/products/xlsx", async (AppDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    var file = await products.ToFileFormatAsync(ExportFormat.Xlsx);
    return file.ToFileResult();
});
```

### File Naming

**Default naming:**

```csharp
typeof(Product) → "Product 2024-01-15 14:30:00"
```

**Custom naming with placeholder:**

```csharp
WithName("Sales Report {DateTime}")
// Output: "Sales Report 2024-01-15 14:30:00"
```

**Fixed name:**

```csharp
WithName("Monthly_Export")
// Output: "Monthly Export 2024-01-15 14:30:00" (DateTime still appended)
```

## Extension Methods

### IEnumerable<T>

```csharp
var file = await data.ToFileFormatAsync(ExportFormat.Csv);
var file = await data.ToFileFormatAsync(ExportFormat.Xlsx);
```

### IAsyncEnumerable<T>

```csharp
var file = await asyncData.ToCsvAsync();
var file = await asyncData.ToXlsxAsync();
var file = await asyncData.ToFileFormatAsync(ExportFormat.Csv);
```

### ExportFile

```csharp
var file = await data.ToFileFormatAsync(ExportFormat.Xlsx);

// Get file properties
string name = file.Name;         // "Products 2024-01-15 14:30:00.xlsx"
string mimeType = file.MimeType; // "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
byte[] content = file.Content;

// Return from API
return file.ToFileResult();
```

## Performance

Built on industry-standard libraries:

- **CsvHelper** (33.1.0) - Fast, reliable CSV parsing/writing
- **SpreadCheetah** (1.25.0) - High-performance XLSX generation

**Benchmarks** (1M rows, 10 columns):

- CSV export: ~3 seconds, ~100MB file
- XLSX export: ~8 seconds, ~40MB file
- Memory: Streaming-based, low memory footprint

## Limits

| Feature              | Limit         | Behavior                       |
|----------------------|---------------|--------------------------------|
| XLSX rows per sheet  | 1,048,575     | Auto-creates additional sheets |
| XLSX sheet name      | 31 characters | Auto-truncates                 |
| File size before zip | 10 MB         | Auto-compresses to ZIP         |

## Complete Example

```csharp
// Model
public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

// Export configuration
public class OrderExportRule : ExportRule<Order>
{
    public OrderExportRule()
    {
        WithName("Order Export {DateTime}");
        
        RuleFor(x => x.OrderId)
            .WriteToColumn("Order #")
            .HasOrder(1);
        
        RuleFor(x => x.CustomerName)
            .WriteToColumn("Customer")
            .HasWidth(30)
            .HasOrder(2);
        
        RuleFor(x => x.TotalAmount)
            .WriteToColumn("Total")
            .HasFormat(ColumnFormatType.Currency)
            .HasPrecision(2)
            .HasOrder(3);
        
        RuleFor(x => x.OrderDate)
            .WriteToColumn("Date")
            .HasFormat(ColumnFormatType.DateTime)
            .HasOrder(4);
        
        RuleFor(x => x.Status)
            .WriteToColumn("Status")
            .WithEnumFormat(EnumFormatMode.Name)
            .HasOrder(5);
        
        RuleFor(x => x.Notes)
            .WriteToColumn("Notes")
            .WithDefaultValue("No notes")
            .HasOrder(6);
    }
}

// Usage
var orders = await db.Orders.ToListAsync();
var file = await orders.ToFileFormatAsync(ExportFormat.Xlsx);
return file.ToFileResult();
```

## License

MIT