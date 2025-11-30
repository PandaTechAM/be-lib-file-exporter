using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FileExporter.Dtos;

namespace FileExporter.Helpers;

internal static class ZipHelper
{
   public static ExportFile CreateZip(string baseName, MimeTypes innerType, IReadOnlyList<byte[]> parts)
   {
      using var ms = new MemoryStream();
      using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
      {
         for (var i = 0; i < parts.Count; i++)
         {
            var entryName = parts.Count == 1
               ? $"{baseName}{innerType.Extension}"
               : $"{baseName}_{i + 1}{innerType.Extension}";

            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using var es = entry.Open();
            es.Write(parts[i]);
         }
      }

      return new ExportFile(baseName, MimeTypes.Zip, ms.ToArray());
   }
}