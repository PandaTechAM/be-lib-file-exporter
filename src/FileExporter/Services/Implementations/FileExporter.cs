using System;
using System.Collections.Generic;
using FileExporter.Dtos;
using FileExporter.Exporters;
using FileExporter.Rules;
using FileExporter.Services.Interfaces;

namespace FileExporter.Services.Implementations;

internal sealed class FileExporter(IExportRuleRegistry registry) : IFileExporter
{
   private readonly IExportRuleRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));

   public ExportFile ExportToCsv<T>(IEnumerable<T> data) where T : class
   {
      ArgumentNullException.ThrowIfNull(data);

      var rule = _registry.GetRule<T>();
      return CsvExporter.Export(data, rule);
   }

   public ExportFile ExportToXlsx<T>(IEnumerable<T> data) where T : class
   {
      ArgumentNullException.ThrowIfNull(data);

      var rule = _registry.GetRule<T>();
      // XlsxExporter.Export already contains the >1M rows → CSV fallback logic
      return XlsxExporter.Export(data, rule);
   }
}