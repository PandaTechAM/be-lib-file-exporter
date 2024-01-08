using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Gehtsoft.PDFFlow.Builder;

namespace FileExporter;

public static class ZipExtensions
{ 
    public static byte[] ToZip(this byte[] source, string filename)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry(filename, CompressionLevel.Optimal);

            using var entryStream = entry.Open();
            entryStream.Write(source, 0, source.Length);
            entryStream.Close();
        }

        return memoryStream.ToArray();
    }
}