using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using FileExporter.Dtos;
using FileExporter.Helpers;
using FileExporter.Rules;

namespace FileExporter.Exporters;

internal static class CsvExporter
{
   public static ExportFile Export<T>(IEnumerable<T> data, ExportRule<T> rule)
      where T : class
   {
      ArgumentNullException.ThrowIfNull(data);

      ArgumentNullException.ThrowIfNull(rule);

      var columns = BuildColumns(rule);
      var baseName = rule.FileName;
      var fileName = NamingHelper.EnsureExtension(baseName, MimeTypes.Csv.Extension);

      using var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms, new System.Text.UTF8Encoding(true), leaveOpen: true))
      {
         var config = new CsvConfiguration(CultureInfo.InvariantCulture)
         {
            HasHeaderRecord = true
         };

         using var csv = new CsvWriter(writer, config);

         foreach (var column in columns)
         {
            csv.WriteField(column.Rule.ColumnName);
         }

         csv.NextRecord();

         foreach (var item in data)
         {
            foreach (var column in columns)
            {
               var raw = column.Property.GetValue(item);
               var formatted = ValueFormatter.FormatForCsv(raw, column.Rule, CultureInfo.InvariantCulture);
               csv.WriteField(formatted);
            }

            csv.NextRecord();
         }
      }

      var bytes = ms.ToArray();

      if (bytes.Length < ExportLimits.ZipThresholdBytes)
      {
         return new ExportFile(fileName, MimeTypes.Csv, bytes);
      }

      var zipped = ZipHelper.CreateZip(fileName,
         MimeTypes.Csv,
         new List<byte[]>
         {
            bytes
         });
      return zipped;
   }

   private static List<ExportColumn> BuildColumns<T>(ExportRule<T> rule)
      where T : class
   {
      var modelType = typeof(T);
      var properties = modelType
                       .GetProperties()
                       .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

      var columns = new List<ExportColumn>();

      foreach (var r in rule.Rules)
      {
         if (!properties.TryGetValue(r.PropertyName, out var property))
         {
            continue;
         }

         columns.Add(new ExportColumn
         {
            Property = property,
            Rule = r
         });
      }

      return columns;
   }
}