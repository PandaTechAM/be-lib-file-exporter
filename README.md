# PandaTech.FileExporter

**Blazing‑fast, flexible, convention‑first CSV/XLSX exporter for .NET 9+**\
Powered internally by two battle‑tested libraries:\

- **CsvHelper** --- industry‑standard CSV writer\
- **SpreadCheetah** --- extremely fast, fully streaming XLSX generator

`PandaTech.FileExporter` focuses on **performance**, **stability**, and
**developer happiness**.\
Version **5.x is a complete redesign** of 4.x --- simpler, safer,
faster. API is fully modernized (minimal APIs, DI‑friendly,
async‑ready).

> ⚠️ **Breaking Change Notice**\
> v5 breaks compatibility with v4. Migration is straightforward: - enums
> changed values --- **do not reuse v4 enum values** - PDF support
> removed (too slow, not worth maintaining) - All exporters rebuilt
> around a simpler, more predictable format system

------------------------------------------------------------------------

## ✨ Features

- **Convention‑based export** (zero config)\
- **Fluent export rules** via `ExportRule<T>`\
- **Super‑fast XLSX via SpreadCheetah** (safe for millions of rows)\
- **CSV export** using CsvHelper\
- **Automatic ZIP compression** when file size is large\
- **Multi‑sheet XLSX** for datasets \> 1M rows\
- **Async streaming support** (`IAsyncEnumerable<T>`)\
- **Custom formatting & transforms**\
- **Minimal API ready**\
- **Simple DI registration**\
- **No controllers required**

------------------------------------------------------------------------

## 🚀 Installation

``` bash
dotnet add package PandaTech.FileExporter
```

------------------------------------------------------------------------

## ⚙️ Quick Start

### 1. Configure in `Program.cs`

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddFileExporter(); // scans entry assembly for ExportRule<T>
```

### 2. Export from your endpoint

``` csharp
app.MapGet("/export", async () =>
{
    var data = new List<MyRow>
    {
        new() { Id = 1, Name = "Alice" },
        new() { Id = 2, Name = "Bob" }
    };

    var file = await data.ToFileFormatAsync(ExportFormat.Xlsx);
    return file.ToFileResult();
});
```

Done. Zero configuration required.

------------------------------------------------------------------------

## 📄 Defining Custom Rules (Optional)

``` csharp
using FileExporter.Rules;

public sealed class MyRowExportRule : ExportRule<MyRow>
{
    public MyRowExportRule()
    {
        WithName("My Custom Export");

        RuleFor(x => x.Id)
            .HasOrder(0)
            .HasFormat(ColumnFormatType.Integer);

        RuleFor(x => x.Name)
            .HasOrder(1)
            .WriteToColumn("Full Name");
    }
}
```

Just add this class; it will be auto‑discovered.

------------------------------------------------------------------------

## 🔍 Supported Formats

ExportFormat Description
  -------------- -------------------------------------------------
**Csv**        UTF‑8 CSV, auto‑quote, stable for huge datasets
**Xlsx**       Multi‑sheet, streaming, low memory usage

------------------------------------------------------------------------

## 📦 File Naming

Every file receives a timestamp:

    Orders 2025-01-01 10:33:00.xlsx

Automatically sanitized for safety.

------------------------------------------------------------------------

## 🔧 Property Rule Options

  -----------------------------------------------------------------------
Option Description
  --------------------------- -------------------------------------------
`WriteToColumn("Name")`     Override header text

`HasOrder(1)`               Column ordering

`Ignore()`                  Exclude property

`HasFormat(...)`            Text, Integer, Decimal, Currency,
Percentage, Boolean, Date, DateTime

`HasPrecision(2)`           Decimal digit precision (not rounding, for rounding refer `.Transform()`)

`HasWidth(20)`              XLSX column width override

`WithDefaultValue("N/A")`   Replaces null values

`Transform(v => ...)`       Custom transformation


------------------------------------------------------------------------

## 🧬 Enum Formatting

Rules support:

- `MixedIntAndName` *(default)* → `"1 - Active"`
- `Int` → `"1"`
- `Name` → `"Active"`

------------------------------------------------------------------------

## 🔥 Performance Notes

- Handles **millions of rows** with **constant memory** usage\
- XLSX splits automatically into multiple sheets\
- ZIP is only applied when final file exceeds threshold\
- Async pipelines (`IAsyncEnumerable<T>`) supported

------------------------------------------------------------------------

## 🪄 Tips

- Keep DTOs simple; exporters use reflection only once\
- Add custom rules **only for overrides** --- conventions already
  cover:
    - ordering\
    - decimal precision\
    - date formatting\
    - booleans\
- For extremely large exports: prefer `IAsyncEnumerable<T>`

------------------------------------------------------------------------

## 🧭 Demo

``` csharp
app.MapGet("/export/orders", async (ExportFormat format) =>
{
    var orders = Enumerable.Range(1, 2000000)
        .Select(i => new OrderDto { Id = i, Total = i * 1.25m });

    var file = await orders.ToFileFormatAsync(format);
    return file.ToFileResult();
});
```

Exports 2M rows in under a few seconds (depending on disk/CPU).

------------------------------------------------------------------------

## 🏁 Conclusion

`PandaTech.FileExporter` is built to stay small, clean, and blazing
fast.\
If you need CSV/XLSX export without pain --- you're covered.