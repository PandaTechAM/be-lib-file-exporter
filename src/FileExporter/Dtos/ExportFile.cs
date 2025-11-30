namespace FileExporter.Dtos;

public sealed class ExportFile
{
   public ExportFile(string name, MimeTypes mimeType, byte[] content)
   {
      Name = name;
      MimeType = mimeType;
      Content = content;
   }

   public string Name { get; }
   public MimeTypes MimeType { get; }
   public byte[] Content { get; }
}