using System;
using System.Collections.Generic;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exporters;
using FileExporter.Helpers;

namespace FileExporter.Extensions;

public static class EnumerableExportExtensions
{
   public static ExportFile ToCsv<T>(this IEnumerable<T> data)
      where T : class
   {
      var registry = FileExporterRuntime.Registry;
      var rule = registry.GetRule<T>();

      return CsvExporter.Export(data, rule);
   }

   public static ExportFile ToXlsx<T>(this IEnumerable<T> data)
      where T : class
   {
      var registry = FileExporterRuntime.Registry;
      var rule = registry.GetRule<T>();

      return XlsxExporter.Export(data, rule);
   }

   public static ExportFile ToFileFormat<T>(this IEnumerable<T> data,
      ExportFormat format)
      where T : class
   {
      var registry = FileExporterRuntime.Registry;
      var rule = registry.GetRule<T>();

      return format switch
      {
         ExportFormat.Csv => CsvExporter.Export(data, rule),
         ExportFormat.Xlsx => XlsxExporter.Export(data, rule),
         _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
      };
   }
}