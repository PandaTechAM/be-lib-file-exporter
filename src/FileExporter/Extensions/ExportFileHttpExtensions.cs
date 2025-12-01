using System;
using FileExporter.Dtos;
using Microsoft.AspNetCore.Http;

namespace FileExporter.Extensions;

public static class ExportFileHttpExtensions
{
   public static IResult ToFileResult(this ExportFile file)
   {
      return Results.File(
         file.Content,
         file.MimeType.Value,
         file.Name);
   }
}