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
        return await data.ToCsvAsync(null, ct);
    }

    /// <summary>Materializes an async sequence and exports it as a CSV file, applying per-request overrides.</summary>
    public static async Task<ExportFile> ToCsvAsync<T>(this IAsyncEnumerable<T> data,
        ExportOptions? options,
        CancellationToken ct = default)
        where T : class
    {
        var list = await MaterializeAsync(data, ct);

        return CsvExporter.Export(list, FileExporterRuntime.Registry.GetRule<T>(), options);
    }

    /// <summary>
    ///     Materializes an async sequence and exports it as an XLSX file using the registered rule for
    ///     <typeparamref name="T" />.
    /// </summary>
    public static async Task<ExportFile> ToXlsxAsync<T>(this IAsyncEnumerable<T> data,
        CancellationToken ct = default)
        where T : class
    {
        return await data.ToXlsxAsync(null, ct);
    }

    /// <summary>Materializes an async sequence and exports it as an XLSX file, applying per-request overrides.</summary>
    public static async Task<ExportFile> ToXlsxAsync<T>(this IAsyncEnumerable<T> data,
        ExportOptions? options,
        CancellationToken ct = default)
        where T : class
    {
        var list = await MaterializeAsync(data, ct);

        return await XlsxExporter.ExportAsync(list, FileExporterRuntime.Registry.GetRule<T>(), options, ct);
    }

    /// <summary>Materializes an async sequence and exports it in the given <paramref name="format" /> (CSV or XLSX).</summary>
    public static async Task<ExportFile> ToFileFormatAsync<T>(this IAsyncEnumerable<T> data,
        ExportFormat format,
        CancellationToken ct = default)
        where T : class
    {
        return await data.ToFileFormatAsync(format, null, ct);
    }

    /// <summary>Materializes an async sequence and exports it in the given format, applying per-request overrides.</summary>
    public static async Task<ExportFile> ToFileFormatAsync<T>(this IAsyncEnumerable<T> data,
        ExportFormat format,
        ExportOptions? options,
        CancellationToken ct = default)
        where T : class
    {
        EnsureSupported(format);

        var list = await MaterializeAsync(data, ct);

        return await list.ToFileFormatAsync(format, options, ct);
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
        return await data.ToFileFormatAsync(format, null, ct);
    }

    /// <summary>
    ///     Exports a sequence in the given <paramref name="format" /> (CSV or XLSX), applying the per-request
    ///     <paramref name="options" />.
    /// </summary>
    /// <remarks>
    ///     This is the only way to localize headers or enum values: everything on the registered <c>ExportRule</c> is
    ///     fixed at application start and shared by every request.
    /// </remarks>
    public static async Task<ExportFile> ToFileFormatAsync<T>(this IEnumerable<T> data,
        ExportFormat format,
        ExportOptions? options,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);
        EnsureSupported(format);

        var rule = FileExporterRuntime.Registry.GetRule<T>();

        return format switch
        {
            ExportFormat.Csv => CsvExporter.Export(data, rule, options),
            _ => await XlsxExporter.ExportAsync(data, rule, options, ct)
        };
    }

    private static async Task<List<T>> MaterializeAsync<T>(IAsyncEnumerable<T> data, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(data);

        var list = new List<T>();

        await foreach (var item in data.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    ///     Rejects undefined formats up front. A missing <c>?exportFormat=</c> query-string value binds to
    ///     <c>(ExportFormat)0</c>, which is not a member and used to fall through the exporter's switch.
    /// </summary>
    private static void EnsureSupported(ExportFormat format)
    {
        if (format is ExportFormat.Csv or ExportFormat.Xlsx)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(format),
            format,
            $"Unsupported export format. Use {nameof(ExportFormat.Csv)} (1) or {nameof(ExportFormat.Xlsx)} (2). "
            + "A missing or unbound query-string value arrives here as 0.");
    }
}
