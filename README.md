# PandaTech.FileExporter

High-performance CSV, XLSX and PDF exporting library for .NET 8+ with convention-based defaults, fluent configuration,
async streaming, multi-sheet support, paginated PDF tables, and automatic compression.

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

// PDF export: a paginated table with automatic column widths and page size
var pdfFile = await products.ToFileFormatAsync(ExportFormat.Pdf);

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

An explicit `HasFormat(...)` always wins over the inferred format, so `HasFormat(Date)` on a `DateTime` drops the
time part and `HasFormat(Integer)` on a `decimal` drops the decimal places. Booleans are written as the text
`Yes`/`No` in every format: a logical cell renders in the viewer's own Excel UI language, which no number format
can override.

### Column Configuration API

| Method                           | Description              | Example                                 |
|----------------------------------|--------------------------|-----------------------------------------|
| `WriteToColumn(string)`          | Set column header        | `.WriteToColumn("Full Name")`           |
| `HasOrder(int)`                  | Set column position      | `.HasOrder(1)`                          |
| `HasWidth(int)`                  | Set column width (chars; preferred width in PDF) | `.HasWidth(25)`                         |
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

A PDF writes each value the way Excel displays it under the XLSX number format: `Currency` as `12,500.00`,
`Percentage` as `55.32%`, `Date` as `2024-01-15`, `DateTime` as `2024-01-15 14:30:00`. CSV keeps its own conventions.

### Enum Formatting

```csharp
public enum EnumFormatMode
{
    MixedIntAndName, // "1 - Active" (default)
    Int,             // "1"
    Name             // "Active"
}
```

A value that is not a declared member has no name, so `MixedIntAndName` and `Name` both render the bare number
(`7`). Supply an `EnumLabelResolver` to render something other than the C# member name — see below.

## Per-Request Options

An `ExportRule` is discovered once at `AddFileExporter` boot and cached for the process lifetime, so nothing on it
can vary per request. `ExportOptions` is the hook that can. Pass it to any export extension method; every property is
optional and falls back to the rule.

```csharp
var options = new ExportOptions
{
    FileName = "Bestellungen {DateTime}", // used verbatim; {DateTime} is optional
    SheetName = "Bestellungen",           // XLSX sheet (31 characters max) and PDF title
    ColumnHeaders = new Dictionary<string, string>
    {
        [nameof(Order.Status)] = "Status",
        [nameof(Order.TotalAmount)] = "Gesamtbetrag"
    },
    EnumLabelResolver = value => labelByValue.TryGetValue(value, out var label) ? label : string.Empty
};

var file = await orders.ToFileFormatAsync(ExportFormat.Xlsx, options);
```

Headers, enum labels and names are plain strings, so any language or alphabet works — the file name is written to
`Content-Disposition` verbatim and the workbook is UTF-8 throughout.

| Property            | Effect                                                                                          |
|---------------------|-------------------------------------------------------------------------------------------------|
| `FileName`          | Base name, used verbatim. No extension — the format's is appended. `{DateTime}` is substituted if present, but never appended. |
| `SheetName`         | Worksheet name (XLSX, truncated to 31 characters) and PDF title (not truncated). Defaults to the rule's name without its timestamp. |
| `ColumnHeaders`     | Header text per column, keyed by **model property name**. Missing or blank entries keep the rule's header. Column selection and order stay the rule's job. |
| `EnumLabelResolver` | Renders an enum as text, honouring `EnumFormatMode`: `MixedIntAndName` still emits `"1 - {label}"` and `Int` is still a bare number. Returning null or whitespace falls back to the member name. Called once per enum cell, so close over a resolved lookup rather than querying per value. |

Nothing here mutates the shared rule, so concurrent requests can use different languages safely.

## PDF

`ExportFormat.Pdf` renders the same columns, headers and values as CSV and XLSX into a paginated table, through the
same entry points. There are no PDF-only options: everything comes from the export rule and `ExportOptions`.

- **Layout**: column widths follow the content (sampled from up to 5,000 rows) and text wraps inside its column; a word
  longer than its column breaks by character, and explicit line breaks are kept. `HasWidth(n)` sets a column's
  preferred width to n times the width of a `0`. Numeric columns are right-aligned, header included.
- **Page size**: the first of A4 portrait, A4 landscape, A3 landscape and A2 landscape that holds the table
  comfortably, with 8 pt text on A4 and 7 pt on A3 and A2. A table no page holds comfortably goes on A2 landscape and
  wraps harder there.
- **Every page** repeats the title and the header row, and carries an `x / y` page number.
- **Title**: `ExportOptions.SheetName`, else the rule's name (`WithName`) without its timestamp. It is drawn on one
  line and cut with an ellipsis when wider than the page; the document's title metadata keeps the whole text.
- **Fonts**: DejaVu Sans Condensed and Noto Sans Armenian are embedded in the library assembly and loaded from memory.
  The consuming application ships no font files and needs no system fonts, so a Linux container renders exactly like
  a developer machine. They cover Latin, Cyrillic and the full Armenian block; a character neither font has (CJK, for
  example) is drawn as an empty box and never fails the export. Internally they are renamed to FileExporter Sans
  Condensed and FileExporter Sans Armenian: PDFsharp caches fonts process-wide by name, and the original names would
  clash with your application's own copy of either font.
- **PDFsharp's global font resolver is not set.** If your application also uses PDFsharp and sets
  `GlobalFontSettings.FontResolver`, set it at startup, before the first export: PDFsharp refuses a new resolver once
  any font was loaded.
- **Row limit**: at most 100,000 rows per PDF. A larger export throws `ExportRowLimitExceededException` (with `Format`
  and `Limit`) before any value is formatted, and a lazy source is read only one row past the limit. Use CSV or XLSX
  for more rows.
- **Size**: as for CSV and XLSX, a PDF of 10 MB or more is returned as a ZIP holding it.
- **Concurrency**: a large PDF keeps a core busy for seconds and stays in memory until saved, so at most half the
  processor count render at once per process. Other exports wait their turn, honouring their `CancellationToken`.

```csharp
app.MapGet("/export/orders/pdf", async (AppDbContext db, CancellationToken ct) =>
{
    var orders = await db.Orders.ToListAsync(ct);

    try
    {
        var file = await orders.ToFileFormatAsync(ExportFormat.Pdf, new ExportOptions { SheetName = "Orders Q1" }, ct);
        return file.ToFileResult();
    }
    catch (ExportRowLimitExceededException ex)
    {
        return Results.BadRequest($"PDF exports are limited to {ex.Limit} rows.");
    }
});
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

Files of 10 MB or more, in any format, are returned as a ZIP:

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

WithName("Sales Report {DateTime} Final")
// Output: "Sales Report 2024-01-15 14:30:00 Final"
```

A name containing `{DateTime}` is taken exactly as written — no title-casing is applied, since you have already
spelled it the way you want it.

**Fixed name:**

```csharp
WithName("Monthly_Export")
// Output: "Monthly_Export 2024-01-15 14:30:00" (DateTime still appended)
```

The timestamp is resolved **per export**, not when the rule is constructed, so every download carries the time it was
actually requested. Names are capped at 100 characters.

**Per-request name:**

```csharp
await data.ToFileFormatAsync(ExportFormat.Xlsx, new ExportOptions { FileName = "Q1 Orders" });
// Output: "Q1 Orders.xlsx" — used verbatim, no timestamp appended
```

## Extension Methods

### IEnumerable<T>

```csharp
var file = await data.ToFileFormatAsync(ExportFormat.Csv);
var file = await data.ToFileFormatAsync(ExportFormat.Xlsx);
var file = await data.ToFileFormatAsync(ExportFormat.Pdf);
var file = await data.ToFileFormatAsync(ExportFormat.Xlsx, options);
```

### IAsyncEnumerable<T>

```csharp
var file = await asyncData.ToCsvAsync();
var file = await asyncData.ToXlsxAsync();
var file = await asyncData.ToPdfAsync();
var file = await asyncData.ToFileFormatAsync(ExportFormat.Csv);
var file = await asyncData.ToCsvAsync(options);
var file = await asyncData.ToXlsxAsync(options);
var file = await asyncData.ToPdfAsync(options);
var file = await asyncData.ToFileFormatAsync(ExportFormat.Csv, options);
```

An undefined `ExportFormat` — including the `0` an omitted query-string value binds to — throws
`ArgumentOutOfRangeException` before any work is done.

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
- **SpreadCheetah** (1.28.0) - High-performance XLSX generation
- **PDFsharp** (6.2.4) - PDF writing; the table layout is this library's own

**Benchmarks** (1M rows, 10 columns):

- CSV export: ~3 seconds, ~100MB file
- XLSX export: ~8 seconds, ~40MB file
- Memory: Streaming-based, low memory footprint

**PDF** (100,000 rows, measured on a developer laptop): 10 columns on A4 in ~3 seconds (6.6 MB); 27 columns on A2
landscape in ~7 seconds (returned zipped, 11 MB), at about 0.6 GB peak memory including the input rows.

## Limits

| Feature              | Limit         | Behavior                                         |
|----------------------|---------------|--------------------------------------------------|
| XLSX rows per sheet  | 1,048,575     | Auto-creates additional sheets                   |
| XLSX sheet name      | 31 characters | Auto-truncates                                   |
| PDF rows per file    | 100,000       | Rejected with `ExportRowLimitExceededException`  |
| PDF page size        | A4 to A2      | Chosen automatically from the table's width      |
| File name            | 100 characters| Auto-truncates                                   |
| File size before zip | 10 MB         | Auto-compresses to ZIP                           |

## Upgrading to 8.2.0

Additive: no existing call changes behaviour.

- New: `ExportFormat.Pdf` (3), `MimeTypes.Pdf`, `ToPdfAsync` on `IAsyncEnumerable<T>`, and
  `FileExporter.Exceptions.ExportRowLimitExceededException`.
- `ToFileFormatAsync` accepts `ExportFormat.Pdf`. An endpoint that binds `ExportFormat` from the request and validates
  it as a defined enum value now accepts 3 as well.
- New dependency: PDFsharp 6.2.4 (MIT), which adds nine `PdfSharp*.dll` assemblies (about 1.6 MB) to your output.
  `FileExporter.dll` grows by about 1.4 MB of embedded fonts; no font file is copied.
- The embedded fonts keep their own licences, shipped in the package's `licenses/` folder: DejaVu (Bitstream Vera and
  Arev font licence) and Noto Sans Armenian (SIL Open Font License 1.1).

## Upgrading to 8.0.0

Public API is source-compatible — every 7.x call still compiles. Four behaviours changed:

| Change                                                                                            | Why |
|---------------------------------------------------------------------------------------------------|-----|
| The file name's timestamp is stamped at export time, not at rule construction                       | A rule is a singleton, so every download shared the process's start time |
| The worksheet name is the rule's name **without** its timestamp                                    | It used to be the stamped name truncated to 30 characters |
| `MixedIntAndName` / `Name` render an undefined enum value as the bare number                       | XLSX wrote `"7 - "` and CSV wrote `"7 - 7"` |
| `ColumnFormatType.Percentage` treats the value as a fraction in CSV, matching XLSX                  | `0.5532` read `0.55%` in CSV and `55.32%` in XLSX |
| `WithName("... {DateTime} ...")` actually substitutes the placeholder                               | Title-casing rewrote it to `{Date Time}` first, so the literal token shipped in the file name |
| Name cap raised from 30 to 100 characters                                                          | 30 minus a 19-character timestamp truncated almost every real name |

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