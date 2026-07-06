using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exporters;
using FileExporter.Helpers;

namespace FileExporter.Extensions;

/// <summary>Extension methods that export sequences to CSV or XLSX using the registered export rule for the element type.</summary>
public static class EnumerableExportExtensions
{
    /// <summary>
    ///     Materializes an async sequence and exports it as a CSV file using the registered rule for
    ///     <typeparamref name="T" />.
    /// </summary>
    public static async Task<ExportFile> ToCsvAsync<T>(this IAsyncEnumerable<T> data,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);

        var list = new List<T>();

        await foreach (var item in data.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            list.Add(item);
        }

        var registry = FileExporterRuntime.Registry;
        var rule = registry.GetRule<T>();

        return CsvExporter.Export(list, rule);
    }

    /// <summary>
    ///     Materializes an async sequence and exports it as an XLSX file using the registered rule for
    ///     <typeparamref name="T" />.
    /// </summary>
    public static async Task<ExportFile> ToXlsxAsync<T>(this IAsyncEnumerable<T> data,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);

        var list = new List<T>();

        await foreach (var item in data.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            list.Add(item);
        }

        var registry = FileExporterRuntime.Registry;
        var rule = registry.GetRule<T>();

        return await XlsxExporter.ExportAsync(list, rule, ct);
    }

    /// <summary>Materializes an async sequence and exports it in the given <paramref name="format" /> (CSV or XLSX).</summary>
    public static async Task<ExportFile> ToFileFormatAsync<T>(this IAsyncEnumerable<T> data,
        ExportFormat format,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);

        var list = new List<T>();

        await foreach (var item in data.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            list.Add(item);
        }

        return await list.ToFileFormatAsync(format, ct);
    }

    /// <summary>
    ///     Exports a sequence in the given <paramref name="format" /> (CSV or XLSX) using the registered rule for
    ///     <typeparamref name="T" />.
    /// </summary>
    public static async Task<ExportFile> ToFileFormatAsync<T>(this IEnumerable<T> data,
        ExportFormat format,
        CancellationToken ct = default)
        where T : class
    {
        var registry = FileExporterRuntime.Registry;
        var rule = registry.GetRule<T>();

        return format switch
        {
            ExportFormat.Csv => CsvExporter.Export(data, rule),
            ExportFormat.Xlsx => await XlsxExporter.ExportAsync(data, rule, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}
