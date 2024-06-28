using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Helpers;
using PdfSharpCore;

namespace FileExporter.Extensions;

public static class EnumerableExtensions
{
    public static ExportFile ToCsv<T>(this IEnumerable<T> data) => ToCsv(data, GetDisplayName<T>());

    public static ExportFile ToCsv<T>(this IEnumerable<T> data, string name)
    {
        var datatable = new DataTable<T>(data, name);

        var files = datatable.ToCsv();

        if (files.Count == 1 && files.First().Length < Constants.FileMaxSizeInBytes)
        {
            return new ExportFile(datatable.Name, MimeTypes.Csv, files.First());
        }

        return Zip(datatable.Name, MimeTypes.Csv, files);
    }

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data) => ToXlsx(data, GetDisplayName<T>());

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data, string name)
    {
        var datatable = new DataTable<T>(data, name);

        var files = datatable.ToXlsx();

        if (files.Count == 1 && files.First().Length < Constants.FileMaxSizeInBytes)
        {
            return new ExportFile(datatable.Name, MimeTypes.Xlsx, files.First());
        }

        return Zip(datatable.Name, MimeTypes.Xlsx, files);
    }

    public static ExportFile ToPdf<T>(this IEnumerable<T> data, bool headerOnEachPage = true, string fontName = Constants.DefaultFontName, int fontSize = Constants.DefaultFontSize,  PageSize pageSize = PageSize.A4,
    PageOrientation pageOrientation = PageOrientation.Landscape)
    => ToPdf(data, GetDisplayName<T>(), headerOnEachPage, fontName, fontSize, pageSize, pageOrientation);

    public static ExportFile ToPdf<T>(this IEnumerable<T> data,
        string name,
        bool headerOnEachPage = true,
        string fontName = Constants.DefaultFontName,
        int fontSize = Constants.DefaultFontSize,
        PageSize pageSize = PageSize.A4,
        PageOrientation pageOrientation = PageOrientation.Landscape)
    {
        var datatable = new DataTable<T>(data, name);

        var files = datatable.ToPdf(headerOnEachPage, fontName, fontSize, pageSize, pageOrientation);

        if (files.Count == 1 && files.First().Length < Constants.FileMaxSizeInBytes)
        {
            return new ExportFile(datatable.Name, MimeTypes.Pdf, files.First());
        }

        return Zip(datatable.Name, MimeTypes.Pdf, files);
    }
    
    public static ExportFile ToRequestedFormat<T>(this IEnumerable<T> data, ExportType type)
    {
        return type switch
        {
            ExportType.Excel => data.ToXlsx(),
            ExportType.Csv => data.ToCsv(),
            ExportType.Pdf => data.ToPdf(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }


    private static ExportFile Zip(string fileName, MimeTypes mimeType, List<byte[]> files)
    {
        using var memoryStream = new MemoryStream();
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

        for (int i = 0; i < files.Count; i++)
        {
            var entry = archive.CreateEntry(fileName + Suffix(i), CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            entryStream.Write(files[i], 0, files[i].Length);
        }

        archive.Dispose(); //don't delete this line otherwize file will be corrupted

        return new ExportFile(fileName, MimeTypes.Zip, memoryStream.ToArray());

        string Suffix(int index) => files.Count == 1 ? $"{mimeType.Extension}" : $"_{index + 1}{mimeType.Extension}";
    }

    private static string GetDisplayName<T>()
    {
        var displayName = typeof(T).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

        displayName ??= typeof(T).Name + " " + Constants.DateTimePlaceHolder;

        displayName = displayName.Replace(Constants.DateTimePlaceHolder, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return displayName;
    }
}
