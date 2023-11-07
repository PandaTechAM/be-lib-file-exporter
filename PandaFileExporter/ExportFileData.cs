using Microsoft.AspNetCore.Mvc;

namespace PandaFileExporter;

public class ExportFileData
{
    public byte[] Data { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;

    public FileContentResult ToFile() => new FileContentResult(Data, Type) { FileDownloadName = Name };
}