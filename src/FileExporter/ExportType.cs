using System.Text.Json.Serialization;

namespace FileExporter;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExportType
{
    Csv,
    Pdf,
    Xlsx
}