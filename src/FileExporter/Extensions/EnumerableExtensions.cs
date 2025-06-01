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
   public static ExportFile ToCsv<T>(this IEnumerable<T> data)
   {
      return ToCsv(data, NamingHelper.GetDisplayName<T>());
   }

   public static ExportFile ToCsv<T>(this IEnumerable<T> data, IEnumerable<IPropertyRule> rules)
   {
      return ToCsv(data, NamingHelper.GetDisplayName<T>(), rules);
   }

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

   public static ExportFile ToXlsx<T>(this IEnumerable<T> data)
   {
      return ToXlsx(data, NamingHelper.GetDisplayName<T>());
   }

   public static ExportFile ToXlsx<T>(this IEnumerable<T> data, IEnumerable<IPropertyRule> rules)
   {
      return ToXlsx(data, NamingHelper.GetDisplayName<T>(), rules);
   }

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

   public static ExportFile ToPdf<T>(this IEnumerable<T> data,
      bool headerOnEachPage = true,
      string fontName = Constants.DefaultFontName,
      int fontSize = Constants.DefaultFontSize,
      PageSize pageSize = PageSize.A4,
      PageOrientation pageOrientation = PageOrientation.Landscape)
   {
      return ToPdf(data,
         NamingHelper.GetDisplayName<T>(),
         headerOnEachPage,
         fontName,
         fontSize,
         pageSize,
         pageOrientation);
   }

   public static ExportFile ToPdf<T>(this IEnumerable<T> data,
      IEnumerable<IPropertyRule> rules,
      bool headerOnEachPage = true,
      string fontName = Constants.DefaultFontName,
      int fontSize = Constants.DefaultFontSize,
      PageSize pageSize = PageSize.A4,
      PageOrientation pageOrientation = PageOrientation.Landscape)
   {
      return ToPdf(data,
         NamingHelper.GetDisplayName<T>(),
         headerOnEachPage,
         fontName,
         fontSize,
         pageSize,
         pageOrientation);
   }

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


   private static ExportFile CreateZip(string baseName, MimeTypes innerType, IReadOnlyList<byte[]> parts)
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

      // no extra ToArray();  MemoryStream already owns the buffer we need
      return new ExportFile($"{baseName}.zip", MimeTypes.Zip, ms.GetBuffer()[..(int)ms.Length]);
   }


   private static ExportFile ReturnFileOrZippedVersion<T>(DataTable<T> dataTable, List<byte[]> files, MimeTypes type)
   {
      if (files.Count == 1 && files.First()
                                   .Length < Constants.FileMaxSizeInBytes)
      {
         return new ExportFile(dataTable.Name, type, files.First());
      }

      return CreateZip(dataTable.Name, type, files);
   }
}