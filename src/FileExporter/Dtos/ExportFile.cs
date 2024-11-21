using Microsoft.AspNetCore.Mvc;

namespace FileExporter.Dtos;

public class ExportFile(string name, MimeTypes mimeType, byte[] data)
{
   public string Name { get; set; } = name + mimeType.Extension;
   public MimeTypes Type { get; set; } = mimeType;
   public byte[] Data { get; set; } = data;

   public FileContentResult ToFile()
   {
      return new FileContentResult(Data, Type)
      {
         FileDownloadName = Name
      };
   }
}