using FileExporter.Enums;

namespace FileExporter.Rules;

/// <summary>Resolved export configuration for a single property/column.</summary>
public interface IPropertyRule
{
    /// <summary>Name of the source model property.</summary>
    string PropertyName { get; }

    /// <summary>Header text written for the column.</summary>
    string ColumnName { get; }

    /// <summary>Value substituted when the property is null; null uses an empty cell.</summary>
    string? DefaultValue { get; }

    /// <summary>Zero-based column order; null sorts after all ordered columns.</summary>
    int? Order { get; }

    /// <summary>Whether the column is excluded from the export.</summary>
    bool IsIgnored { get; }

    /// <summary>How the value is formatted.</summary>
    ColumnFormatType FormatType { get; }

    /// <summary>Decimal places for numeric formats; null uses the default.</summary>
    int? Precision { get; }

    /// <summary>Column width in the XLSX output; null uses the default.</summary>
    int? ColumnWidth { get; }

    /// <summary>How enum values are rendered.</summary>
    EnumFormatMode EnumFormat { get; }

    /// <summary>Optional transform applied to the raw value before formatting.</summary>
    Func<object?, object?>? CustomTransform { get; }
}
