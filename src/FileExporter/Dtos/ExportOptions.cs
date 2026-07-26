namespace FileExporter.Dtos;

/// <summary>
///     Per-request overrides for a single export call. Everything on it is optional; anything left null falls back to
///     the registered <c>ExportRule</c>'s configuration.
/// </summary>
/// <remarks>
///     An <c>ExportRule</c> is discovered once at <c>AddFileExporter</c> boot and cached for the process lifetime, so
///     nothing configured on it can vary per request. This is the object that can: it is the only way to localize
///     headers or enum values, or to name the file after something the caller knows and the rule cannot.
/// </remarks>
public sealed class ExportOptions
{
    /// <summary>
    ///     Base file name for this export, used verbatim. Invalid file-name characters are replaced and the format's
    ///     extension is appended, so do not include one. A <c>{DateTime}</c> placeholder is substituted with the UTC
    ///     timestamp at export time; unlike the rule's own name, no timestamp is appended when it is absent.
    ///     Null or blank uses the rule's name.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     Worksheet name for XLSX exports, truncated to Excel's 31-character limit. Ignored for CSV. Null or blank
    ///     uses the rule's name without its timestamp.
    /// </summary>
    public string? SheetName { get; set; }

    /// <summary>
    ///     Header text per column, keyed by the <b>model property name</b> (use <c>nameof</c>). Columns absent from the
    ///     dictionary, and entries with a blank value, keep the header the rule declares.
    /// </summary>
    /// <remarks>
    ///     Only affects the header row. Nothing about which columns are exported, or in what order, is changed here —
    ///     that stays the rule's job.
    /// </remarks>
    public IReadOnlyDictionary<string, string>? ColumnHeaders { get; set; }

    /// <summary>
    ///     Renders an enum value as text. Applied wherever the column's <c>EnumFormatMode</c> asks for a name, so
    ///     <c>MixedIntAndName</c> still emits <c>"1 - {label}"</c> and <c>Int</c> is still a bare number.
    /// </summary>
    /// <remarks>
    ///     Returning null or whitespace falls back to the C# member name, then to the numeric value for members that
    ///     have none. The delegate is called once per enum cell, so it must be cheap — resolve the whole translation
    ///     set once and close over it rather than looking each value up individually.
    /// </remarks>
    public Func<Enum, string>? EnumLabelResolver { get; set; }
}
