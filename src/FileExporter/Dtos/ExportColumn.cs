using System.Reflection;
using FileExporter.Rules;

namespace FileExporter.Dtos;

internal sealed class ExportColumn
{
    public required PropertyInfo Property { get; init; }
    public required IPropertyRule Rule { get; init; }

    /// <summary>
    ///     Header actually written for this export. Defaults to the rule's <c>ColumnName</c> and is overridden by
    ///     <see cref="ExportOptions.ColumnHeaders" /> — the only per-request hook, since the rule is a boot singleton.
    /// </summary>
    public required string Header { get; init; }
}
