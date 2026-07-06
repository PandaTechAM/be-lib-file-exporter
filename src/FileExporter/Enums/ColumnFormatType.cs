namespace FileExporter.Enums;

/// <summary>How a column's values are formatted in the exported output.</summary>
public enum ColumnFormatType
{
    /// <summary>No explicit format; inferred from the property type.</summary>
    Default,

    /// <summary>Plain text.</summary>
    Text,

    /// <summary>Whole number.</summary>
    Integer,

    /// <summary>Decimal number, rounded to the rule's precision.</summary>
    Decimal,

    /// <summary>Currency amount, prefixed with the culture's currency symbol.</summary>
    Currency,

    /// <summary>Percentage, suffixed with a percent sign.</summary>
    Percentage,

    /// <summary>Date only.</summary>
    Date,

    /// <summary>Date and time.</summary>
    DateTime,

    /// <summary>Boolean rendered as Yes/No.</summary>
    Boolean
}
