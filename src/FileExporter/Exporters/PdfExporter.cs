using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exceptions;
using FileExporter.Helpers;
using FileExporter.Pdf;
using FileExporter.Rules;

namespace FileExporter.Exporters;

internal static class PdfExporter
{
    /// <summary>
    ///     Bounds concurrent renders per process: a large PDF keeps a core busy for seconds and holds the whole document
    ///     in memory until it is saved. Waiting honours the caller's token, so an abandoned export leaves the queue.
    /// </summary>
    private static readonly SemaphoreSlim RenderGate = new(Math.Max(1, Environment.ProcessorCount / 2));

    internal static async Task<ExportFile> ExportAsync<T>(IEnumerable<T> data,
        ExportRule<T> rule,
        ExportOptions? options = null,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(rule);

        // One row past the limit is enough to know, so an oversized lazy source is never read to the end.
        var rows = data as IReadOnlyList<T> ?? data.Take(ExportLimits.MaxPdfRows + 1).ToList();

        if (rows.Count > ExportLimits.MaxPdfRows)
        {
            throw new ExportRowLimitExceededException(ExportFormat.Pdf, ExportLimits.MaxPdfRows);
        }

        var columns = ExportColumnBuilder.Build(rule, options);
        var baseName = NamingHelper.ResolveFileName(options?.FileName, rule.FileNameTemplate);
        var enumLabelResolver = options?.EnumLabelResolver;

        var requestedTitle = options?.SheetName;
        var title = TextMetrics.Normalize(string.IsNullOrWhiteSpace(requestedTitle) ? rule.DisplayName : requestedTitle);

        var pdfColumns = columns
            .Select(column => new PdfColumn(TextMetrics.Normalize(column.Header),
                ValueFormatter.IsRightAlignedInPdf(column.Rule, column.Property.PropertyType),
                column.Rule.ColumnWidth))
            .ToArray();

        await RenderGate.WaitAsync(ct);

        byte[] bytes;

        try
        {
            using var renderer = new PdfTableRenderer(title, pdfColumns);

            var sample = PdfLayout.SampleIndexes(rows.Count)
                .Select(index => FormatRow(rows[index], columns, enumLabelResolver));

            var plan = PdfLayout.Plan(PdfLayout.Measure(pdfColumns, sample, renderer.Regular, renderer.Bold));

            bytes = renderer.Render(plan, rows.Select(row => FormatRow(row, columns, enumLabelResolver)), ct);
        }
        finally
        {
            RenderGate.Release();
        }

        return ExportFileFactory.Create(baseName, MimeTypes.Pdf, bytes);
    }

    private static string[] FormatRow<T>(T item, List<ExportColumn> columns, Func<Enum, string>? enumLabelResolver)
    {
        var cells = new string[columns.Count];

        for (var c = 0; c < columns.Count; c++)
        {
            var column = columns[c];
            var raw = column.Property.GetValue(item);

            cells[c] = TextMetrics.Normalize(ValueFormatter.FormatForPdf(raw, column.Rule, enumLabelResolver));
        }

        return cells;
    }
}
