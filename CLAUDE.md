# PandaTech.FileExporter - Claude Guide

## What is this?

A .NET NuGet library that exports `IEnumerable<T>` and `IAsyncEnumerable<T>` to CSV, XLSX or PDF from a
convention-based, fluent `ExportRule<T>`. PandaTech backends get it through `Pandatech.SharedKernel`.

## Architecture

- One library project, `src/FileExporter/` (net8.0, net9.0, net10.0; C# 12 on net8.0, so no .NET 9+ APIs such as
  `Lock`), a demo app in `test/FileExporter.Demo/` and unit tests in `test/FileExporter.Tests/`.
- Rules are discovered once by `AddFileExporter` and cached for the process (`ExportRuleRegistry`,
  `FileExporterRuntime`). Nothing on a rule can vary per request; `ExportOptions` is the per-request hook.
- Three exporters, `CsvExporter`, `XlsxExporter` and `PdfExporter`, share `ExportColumnBuilder` (columns, headers,
  order), `ValueFormatter` (values), `NamingHelper` (names) and `ExportFileFactory` (the file-or-zip rule). A new format
  implements the same `ExportAsync(data, rule, options, ct)` shape and is dispatched in `EnumerableExportExtensions`.
  Never add format-specific overloads or options: the PdfSharpCore exporter removed in `37b9675` had them.
- PDF lives in `Pdf/`: `TextMetrics` (widths and wrapping from per-character advances; pure), `PdfLayout` (sample, then
  column measures, then page, font size and widths; pure), `PdfFonts` (embedded fonts, fallback set, run splitting),
  `PdfTableRenderer` (PDFsharp drawing only), and the records `PdfColumn` and `PdfTablePlan`.

## Key Files

- `Extensions/EnumerableExportExtensions.cs` - every public entry point, format dispatch, `EnsureSupported`
- `Rules/ExportRule.cs`, `Rules/PropertyRule.cs` - the fluent rule API and convention defaults
- `Helpers/ValueFormatter.cs` - `FormatForCsv`, `FormatForXlsx`, `FormatForPdf`, PDF alignment
- `Helpers/ExportLimits.cs` - zip threshold, XLSX rows per sheet, PDF row cap
- `Exporters/PdfExporter.cs` - row cap, columns, render gate, sample, plan, render

## Limits

- Every format: 10 MB or more is returned as a zip (`ExportFileFactory`).
- XLSX: 1,048,575 rows per sheet, then extra sheets in the same workbook.
- PDF: 100,000 rows. Above that, `ExportRowLimitExceededException` before any value is formatted; a lazy source is read
  one row past the limit and no further. Renders are gated to `ProcessorCount / 2` per process.

## PDF Rules

- Fonts are `EmbeddedResource` only (`FileExporter.Fonts.*`) and load through `XFontSource.GetOrCreateFrom` and
  `XGlyphTypeface`. Never `CopyToOutputDirectory` or `PackageCopyToOutput`: the old exporter shipped Microsoft's Arial
  into every consuming app that way. Licence texts are packed under `licenses/`.
- The TTFs are DejaVu 2.37 and Noto Sans Armenian 2.008 with their name tables rewritten to FileExporter Sans
  Condensed and FileExporter Sans Armenian (fontTools; glyphs untouched). PDFsharp registers every font source in a
  process-global dictionary keyed by full font name and throws on a duplicate, so the public names would break either
  our PDFs or the host's own PDFsharp use when the host loads another build of the same font. Keep private names when
  replacing a font; both licences allow it, and DejaVu's requires it for any modified copy.
- Never set `GlobalFontSettings.FontResolver`. PDFsharp 6.2.4 ignores a second resolver of the same type, replaces it
  with one of another type only while no font is loaded, and throws after that. Any resolver installed here would
  break the application's own or be broken by it.
- DejaVu Sans Condensed lacks exactly U+0560, U+0588, U+058D, U+058E and U+058F of the Armenian block (U+058F is the
  dram sign). Those five are drawn in Noto Sans Armenian as separate runs on the same baseline. A character in neither
  font draws as an empty box and never throws.
- PDFsharp does not kern, so summed per-character advances equal the drawn width; `TextMetrics` relies on it. The
  exception is a glyph the font lacks: PDFsharp measures .notdef (0.54 em) but viewers draw it at the PDF default
  width of 1 em, so `PdfTableRenderer.Advance` counts it as 1 em (and a surrogate pair as one 1 em glyph).
- Control characters print as boxes: `TextMetrics.Normalize` turns line breaks into `\n`, which wraps as a hard break,
  and other control characters into spaces, for every cell, header and the title.
- `XGraphics.DrawString` allocates about 2 KB per call inside PDFsharp, so a 100,000 x 27 export churns about 7 GB of
  short-lived garbage in about 7 s. Our per-cell path allocates only the formatted string and any wrapped lines;
  `XBrushes.*` and `XStringFormats.*` are getters that allocate on every access, so keep them in static fields.
- Page choice: the first of A4 portrait, A4 landscape, A3 landscape, A2 landscape that holds max(sum of minimums,
  0.8 x sum of preferred widths), else A2. Never "the smallest page that holds the minimums": a wider table then lands
  on a smaller page.

## Gotchas

- `ExportRule.InitializeDefaultRules` seeds rules through `dynamic`, which binds with this library's accessibility: a
  model with an internal property type fails at rule construction. Test models and rules must be public.
- A declared `HasFormat` wins over the CLR type, in XLSX and in PDF.
- Booleans are the text `Yes`/`No` in every format.
- PDF values follow what Excel displays for the XLSX number formats, not what CSV writes.
- `README.md` is packed into the nupkg and serves every consumer: keep it free of app-specific content.

## Build & Test

```bash
dotnet build FileExporter.slnx
dotnet test FileExporter.slnx
```

Tests are xUnit v3 on Microsoft.Testing.Platform. Check the summary reports a non-zero count: without the three MTP
properties in the test csproj, `dotnet test` silently runs nothing. CI (`.github/workflows/main.yml`) restores,
builds, tests, packs and publishes to nuget.org on every push to `main`.

## Release Train

FileExporter, then `Pandatech.SharedKernel` (which repins it), then the consuming apps (be-ca-condominium bumps
`Pandatech.SharedKernel` in `Condominium.SharedKernel.csproj`). Wait for nuget.org to list each version before the
next leg, and never commit or push without the owner's go.
