using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exporters;
using FileExporter.Helpers;

namespace FileExporter.Extensions;

public static class EnumerableExportExtensions
{
   public static async Task<ExportFile> ToCsvAsync<T>(this IAsyncEnumerable<T> data,
      CancellationToken ct = default)
      where T : class
   {
      ArgumentNullException.ThrowIfNull(data);

      var list = new List<T>();

      await foreach (var item in data.WithCancellation(ct))
      {
         ct.ThrowIfCancellationRequested();
         list.Add(item);
      }

      var registry = FileExporterRuntime.Registry;
      var rule = registry.GetRule<T>();

      return CsvExporter.Export(list, rule);
   }

   public static async Task<ExportFile> ToXlsxAsync<T>(this IAsyncEnumerable<T> data,
      CancellationToken ct = default)
      where T : class
   {
      ArgumentNullException.ThrowIfNull(data);

      var list = new List<T>();

      await foreach (var item in data.WithCancellation(ct))
      {
         ct.ThrowIfCancellationRequested();
         list.Add(item);
      }

      var registry = FileExporterRuntime.Registry;
      var rule = registry.GetRule<T>();

      return await XlsxExporter.ExportAsync(list, rule, ct);
   }

   public static async Task<ExportFile> ToFileFormatAsync<T>(this IAsyncEnumerable<T> data,
      ExportFormat format,
      CancellationToken ct = default)
      where T : class
   {
      ArgumentNullException.ThrowIfNull(data);

      var list = new List<T>();

      await foreach (var item in data.WithCancellation(ct))
      {
         ct.ThrowIfCancellationRequested();
         list.Add(item);
      }

      return await list.ToFileFormatAsync(format, ct);
   }

   public static async Task<ExportFile> ToFileFormatAsync<T>(this IEnumerable<T> data,
      ExportFormat format,
      CancellationToken ct = default)
      where T : class
   {
      var registry = FileExporterRuntime.Registry;
      var rule = registry.GetRule<T>();

      return format switch
      {
         ExportFormat.Csv => CsvExporter.Export(data, rule),
         ExportFormat.Xlsx => await XlsxExporter.ExportAsync(data, rule, ct),
         _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
      };
   }
}