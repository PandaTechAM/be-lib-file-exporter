using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using BaseConverter;
using System;
using PdfSharpCore;

namespace FileExporter;

public static class CommonExtensions
{
    public static string ToBase36String(this object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        _ = long.TryParse((string)value, out var convertedValue);

        return PandaBaseConverter.Base10ToBase36(convertedValue) ?? string.Empty;
    }

    public static byte[] ToZip(this byte[] source, string filename)
    {
        using var memoryStream = new MemoryStream();
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

        var entry = archive.CreateEntry(filename, CompressionLevel.Optimal);

        using var entryStream = entry.Open();
        entryStream.Write(source, 0, source.Length);

        return memoryStream.ToArray();
    }

    public static string ToValidName(this string name)
    {
        var invalidChars = "\0\u0003:\\/?*[]".ToCharArray();

        var validName = name;
        foreach (var invalidChar in invalidChars)
        {
            validName = validName.Replace(invalidChar, '_');
        }

        return validName[..Math.Min(validName.Length, Constants.NameLength)];
    }
}
