namespace FileExporter;

public class MimeTypes
{
    public string Value { get; init; }
    public string Extension { get; init; }

    private MimeTypes(string value, string fileExtension)
    {
        Value = value;
        Extension = fileExtension;
    }

    public static implicit operator string(MimeTypes mimeType) => mimeType.Value;

    public override string ToString() => Value;

    public static readonly MimeTypes Csv = new("application/csv", ".csv");
    public static readonly MimeTypes Pdf = new("application/pdf", ".pdf");
    public static readonly MimeTypes Xlsx = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
    public static readonly MimeTypes Zip = new("application/zip", ".zip");
}