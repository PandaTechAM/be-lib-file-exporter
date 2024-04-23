using DocumentFormat.OpenXml.Bibliography;
using PdfSharpCore;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.ComponentModel;
using System;
using System.Reflection;
using System.Xml.Schema;

namespace FileExporter;

public static class EnumerableExtensions
{
    public static ExportFile ToCsv<T>(this IEnumerable<T> data) => ToCsv(data, GetDisplayName<T>());

    public static ExportFile ToCsv<T>(this IEnumerable<T> data, string name)
    {
        var datatable = new DataTable<T>(data, name);

        var csvs = datatable.ToCsv();

        if (csvs.Count == 1)
        {
            return new ExportFile(datatable.Name, MimeTypes.CSV, csvs.First());
        }

        return ZipMultipeFiles(datatable.Name, MimeTypes.CSV, csvs);
    }

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data) => ToXlsx(data, GetDisplayName<T>());

    public static ExportFile ToXlsx<T>(this IEnumerable<T> data, string name)
    {
        var datatable = new DataTable<T>(data, name);

        var files = datatable.ToXlsx();

        if (files.Count == 1)
        {
            return new ExportFile(datatable.Name, MimeTypes.XLSX, files.First());
        }

        return ZipMultipeFiles(datatable.Name, MimeTypes.XLSX, files);
    }

    public static ExportFile ToPdf<T>(this IEnumerable<T> data, bool headerOnEachPage = false, PageSize pageSize = PageSize.A4,
    PageOrientation pageOrientation = PageOrientation.Landscape)
    => ToPdf(data, GetDisplayName<T>(), headerOnEachPage, pageSize, pageOrientation);

    public static ExportFile ToPdf<T>(this IEnumerable<T> data, string name, bool headerOnEachPage = false, PageSize pageSize = PageSize.A4,
    PageOrientation pageOrientation = PageOrientation.Landscape)
    {
        var datatable = new DataTable<T>(data, name);

        var pdfData = datatable.ToPdf(headerOnEachPage, pageSize, pageOrientation);

        return new ExportFile(datatable.Name, MimeTypes.PDF, pdfData);
    }


    private static ExportFile ZipMultipeFiles(string fileName, MimeTypes mimeType, List<byte[]> files)
    {
        using var memoryStream = new MemoryStream();
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);

        for (int i = 0; i < files.Count; i++)
        {
            var entry = archive.CreateEntry(fileName + $"_{i + 1}{mimeType.Extension}", CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            entryStream.Write(files[i], 0, files[i].Length);
        }

        archive.Dispose(); //don't delete this line otherwize file will be corrupted

        return new ExportFile(fileName, MimeTypes.ZIP, memoryStream.ToArray());
    }

    private static string GetDisplayName<T>()
    {
        var displayName = typeof(T).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

        displayName ??= typeof(T).Name + " " + Constants.DateTimePlaceHolder;

        displayName = displayName.Replace(Constants.DateTimePlaceHolder, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return displayName;
    }
}
