using System.Globalization;
using FileExporter.Enums;

namespace FileExporter.Exceptions;

/// <summary>
///     Thrown when an export holds more rows than its format accepts. It is raised before any value is formatted, and
///     a lazy source is read only one row past the limit.
/// </summary>
public sealed class ExportRowLimitExceededException : Exception
{
    /// <summary>Creates the exception for the format whose row limit was exceeded.</summary>
    /// <param name="format">The requested export format.</param>
    /// <param name="limit">The maximum number of rows that format accepts.</param>
    public ExportRowLimitExceededException(ExportFormat format, int limit)
        : base($"{format} exports are limited to {limit.ToString(CultureInfo.InvariantCulture)} rows.")
    {
        Format = format;
        Limit = limit;
    }

    /// <summary>The requested export format.</summary>
    public ExportFormat Format { get; }

    /// <summary>The maximum number of rows the format accepts.</summary>
    public int Limit { get; }
}
