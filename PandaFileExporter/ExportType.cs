using System.Text.Json.Serialization;

namespace PandaFileExporter
{
    [JsonConv rter(typeof(JsonStringEnumConverter))]
    public enum ExportType
    {
        CSV,
        PDF,
        XLS,
        XLSX
    }
}
