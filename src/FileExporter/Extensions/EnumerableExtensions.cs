using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Helpers;
using FileExporter.Rules;
using PdfSharpCore;

namespace FileExporter.Extensions;

public static class EnumerableExtensions
{
    public static ExportFile ToCsv<T>(this IEnumerable<T> data) => ToCsv(data, NamingHelper.GetDisplayName<T>());

    public static ExportFile ToCsv<T>(this IEnumerable<T> data, IEnumerable<IPropertyRule> rules) =>
        ToCsv(data, NamingHelper.GetDisplayName<T>(), rules);

    public static ExportFile ToCsv<T>(this IEnumerable<T> data, string name)
    {
        var datatable = new DataTable<T>(data, name);

        var files = datatable.ToCsv();

        return ReturnFileOrZippedVersion(datatable, files, MimeTypes.Csv);
    }

    public static ExportFile ToCsv<T>(this IEnumerable<T> data, string name, IEnumerable<IPropertyRule> rules)
    {
        var datatable = new DataTable<T>(data, name, rules);

        var files = datatable.ToCsv();

        return ReturnFileOrZippedVersion(datatable, files, MimeTypes.Csv);
    }

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data) => ToXlsx(data, NamingHelper.GetDisplayName<T>());

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data, IEnumerable<IPropertyRule> rules) =>
        ToXlsx(data, NamingHelper.GetDisplayName<T>(), rules);

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data, string name)
    {
        var datatable = new DataTable<T>(data, name);

        var files = datatable.ToXlsx();

        return ReturnFileOrZippedVersion(datatable, files, MimeTypes.Xlsx);
    }

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data, string name, IEnumerable<IPropertyRule> rules)
    {
        var datatable = new DataTable<T>(data, name, rules);

        var files = datatable.ToXlsx();

        return ReturnFileOrZippedVersion(datatable, files, MimeTypes.Xlsx);
    }

    public static ExportFile ToPdf<T>(this IEnumerable<T> data, bool headerOnEachPage = true,
        string fontName = Constants.DefaultFontName, int fontSize = Constants.DefaultFontSize,
        PageSize pageSize = PageSize.A4,
        PageOrientation pageOrientation = PageOrientation.Landscape)
        => ToPdf(data, NamingHelper.GetDisplayName<T>(), headerOnEachPage, fontName, fontSize, pageSize,
            pageOrientation);

    public static ExportFile ToPdf<T>(this IEnumerable<T> data, IEnumerable<IPropertyRule> rules,
        bool headerOnEachPage = true, string fontName = Constants.DefaultFontName,
        int fontSize = Constants.DefaultFontSize, PageSize pageSize = PageSize.A4,
        PageOrientation pageOrientation = PageOrientation.Landscape)
        => ToPdf(data, NamingHelper.GetDisplayName<T>(), headerOnEachPage, fontName, fontSize, pageSize,
            pageOrientation);

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

        return ReturnFileOrZippedVersion(datatable, files, MimeTypes.Pdf);
    }

    public static ExportFile ToPdf<T>(this IEnumerable<T> data,
        string name,
        IEnumerable<IPropertyRule> rules,
        bool headerOnEachPage = true,
        string fontName = Constants.DefaultFontName,
        int fontSize = Constants.DefaultFontSize,
        PageSize pageSize = PageSize.A4,
        PageOrientation pageOrientation = PageOrientation.Landscape)
    {
        var datatable = new DataTable<T>(data, name, rules);

        var files = datatable.ToPdf(headerOnEachPage, fontName, fontSize, pageSize, pageOrientation);

        return ReturnFileOrZippedVersion(datatable, files, MimeTypes.Pdf);
    }

    public static ExportFile ToFileFormat<T>(this IEnumerable<T> data, ExportType type)
    {
        return type switch
        {
            ExportType.Xlsx => data.ToXlsx(),
            ExportType.Csv => data.ToCsv(),
            ExportType.Pdf => data.ToPdf(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    private static ExportFile Zip(string fileName, MimeTypes mimeType, List<byte[]> files)
    {
        using var memoryStream = new MemoryStream();
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
        {
        }
        for (var i = 0; i < files.Count; i++)
        {
            var entry = archive.CreateEntry(fileName + Suffix(i), CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            entryStream.Write(files[i], 0, files[i].Length);
        }

        archive.Dispose(); //don't delete this line otherwize file will be corrupted

        return new ExportFile(fileName, MimeTypes.Zip, memoryStream.ToArray());

        string Suffix(int index) => files.Count == 1 ? $"{mimeType.Extension}" : $"_{index + 1}{mimeType.Extension}";
    }

    private static ExportFile ReturnFileOrZippedVersion<T>(DataTable<T> dataTable, List<byte[]> files, MimeTypes type)
    {
        if (files.Count == 1 && files.First().Length < Constants.FileMaxSizeInBytes)
        {
            return new ExportFile(dataTable.Name, type, files.First());
        }

        return Zip(dataTable.Name, type, files);
    }
}