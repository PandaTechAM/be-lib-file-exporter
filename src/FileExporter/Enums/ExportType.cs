namespace FileExporter.Enums;

/// <summary>Target file format for an export operation.</summary>
public enum ExportFormat
{
    /// <summary>Comma-separated values (.csv).</summary>
    Csv = 1,

    /// <summary>Excel spreadsheet (.xlsx).</summary>
    Xlsx = 2,

    /// <summary>
    ///     PDF document (.pdf): a paginated table with automatic column widths and page size, limited to 100,000 rows.
    /// </summary>
    Pdf = 3
}
