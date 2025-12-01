namespace FileExporter.Dtos;

public sealed class ExportFile(string name, MimeTypes mimeType, byte[] content)
{
   public string Name { get; } = name;
   public MimeTypes MimeType { get; } = mimeType;
   public byte[] Content { get; } = content;
}