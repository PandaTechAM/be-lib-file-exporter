namespace FileExporter.Enums;

/// <summary>How enum values are rendered in the exported output.</summary>
public enum EnumFormatMode
{
    /// <summary>Both the underlying integer and the name, e.g. "1 - Active".</summary>
    MixedIntAndName,

    /// <summary>The underlying integer value only.</summary>
    Int,

    /// <summary>The enum member name only.</summary>
    Name
}
