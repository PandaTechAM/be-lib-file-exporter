using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Gehtsoft.PDFFlow.Builder;

namespace PandaFileExporter;

public static class FileExporter
{
    private static readonly int ExportSizeLimit =
        Convert.ToInt32(Environment.GetEnvironmentVariable("EXPORT_SIZE_LIMIT") ?? "10");

    public static byte[] ToExcelArray<T>(IQueryable<T>? source)
    {
        try
        {
            return source.ToDataTable().ToXlsx();
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Export failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Export failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    public static byte[] ToExcelArray<T>(IEnumerable<T>? source)
    {
        return ToExcelArray(source?.AsQueryable());
    }

    public static byte[] ToExcelArray<T>(List<T>? source)
    {
        return ToExcelArray(source?.AsQueryable());
    }

    public static byte[] ToCsvArray<T>(IQueryable<T>? source)
    {
        try
        {
            return source.ToDataTable().ToCsv();
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Export failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Export failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    public static byte[] ToCsvArray<T>(IEnumerable<T>? source)
    {
        return ToCsvArray(source?.AsQueryable());
    }

    public static byte[] ToCsvArray<T>(List<T>? source)
    {
        return ToCsvArray(source?.AsQueryable());
    }

    public static byte[] ToPdfArray<T>(IQueryable<T>? source)
    {
        try
        {
            return source.ToDataTable().ToPdf();
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Export failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Export failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    public static byte[] ToPdfArray<T>(IEnumerable<T>? source)
    {
        return ToPdfArray(source?.AsQueryable());
    }

    public static byte[] ToPdfArray<T>(List<T>? source)
    {
        return ToPdfArray(source?.AsQueryable());
    }

    public static byte[] ToZipArray(byte[] source, string filename)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Zip failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Zip failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    private static FontBuilder GetArialUtf8Font(int fontSize = 9, bool bold = false)
    {
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "ARIAL.ttf");

        var fontLoader = FontBuilder.New().FromFile(fontPath, fontSize).SetBold(bold); //Fonts/ARIAL.TTF

        return fontLoader;
    }

    private static Task<ExportFileData> GetFileData<T>(IQueryable<T> source, ExportType exportType)
    {
        var data = exportType switch
        {
            ExportType.XLSX => new ExportFileData
            {
                Data = ToExcelArray(source),
                Type = MimeTypes.XLSX,
            },
            ExportType.CSV => new ExportFileData
            {
                Data = ToCsvArray(source),
                Type = MimeTypes.CSV,
            },
            ExportType.PDF => new ExportFileData
            {
                Data = ToPdfArray(source),
                Type = MimeTypes.PDF,
            },
            _ => throw new ArgumentException("Unsupported data file type")
        };

        data.Name = $"{typeof(T).Name /*.ToSnakeCase()*/}.{exportType.ToString().ToLower()}";

        if (data.Data.Length > ExportSizeLimit * 1024 * 1024)
        {
            data = new ExportFileData
            {
                Data = ToZipArray(data.Data, $"{typeof(T).Name /*.ToSnakeCase()*/}.{exportType.ToString().ToLower()}"),
                Type = MimeTypes.ZIP,
                Name = $"{typeof(T).Name /*.ToSnakeCase()*/}.zip"
            };
        }

        return Task.FromResult(data);
    }
}