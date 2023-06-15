using System.Text.Json.Serialization;

namespace PandaFileExporter
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExportType
    {
        CSV,
        PDF,
        XLS,
        XLSX
    }
}
