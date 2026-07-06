namespace FileExporter.Dtos;

/// <summary>MIME type and matching file extension for a supported export format.</summary>
public sealed class MimeTypes
{
    /// <summary>CSV: <c>text/csv</c> with a <c>.csv</c> extension.</summary>
    public static readonly MimeTypes Csv = new("text/csv", ".csv");

    /// <summary>XLSX: the OpenXML spreadsheet MIME type with a <c>.xlsx</c> extension.</summary>
    public static readonly MimeTypes Xlsx =
        new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");

    /// <summary>ZIP: <c>application/zip</c> with a <c>.zip</c> extension, used when large exports are zipped.</summary>
    public static readonly MimeTypes Zip = new("application/zip", ".zip");

    private MimeTypes(string value, string extension)
    {
        Value = value;
        Extension = extension;
    }

    /// <summary>The MIME type string, e.g. <c>text/csv</c>.</summary>
    public string Value { get; }

    /// <summary>The file extension, including the leading dot.</summary>
    public string Extension { get; }

    /// <summary>Implicitly converts to the underlying MIME type string.</summary>
    public static implicit operator string(MimeTypes mimeType)
    {
        return mimeType.Value;
    }

    /// <summary>Returns the MIME type string.</summary>
    public override string ToString()
    {
        return Value;
    }
}
