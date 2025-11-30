using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExporter.Dtos;
using FileExporter.Helpers;
using FileExporter.Rules;
using SpreadCheetah;
using SpreadCheetah.Styling;
using SpreadCheetah.Worksheets;

namespace FileExporter.Exporters;

internal static class XlsxExporter
{
   // Primary async API
   public static async Task<ExportFile> ExportAsync<T>(IEnumerable<T> data,
      ExportRule<T> rule,
      CancellationToken cancellationToken = default)
      where T : class
   {
      ArgumentNullException.ThrowIfNull(data);
      ArgumentNullException.ThrowIfNull(rule);

      var list = data as IList<T> ?? data.ToList();
      var totalRows = list.Count;

      if (totalRows > ExportLimits.MaxXlsxRowsPerFile)
      {
         return CsvExporter.Export(list, rule);
      }

      var columns = BuildColumns(rule);

      var baseName = rule.FileName;
      var fileName = NamingHelper.EnsureExtension(baseName, MimeTypes.Xlsx.Extension);
      
      await using var ms = new MemoryStream();

      await using (var spreadsheet = await Spreadsheet.CreateNewAsync(ms, cancellationToken: cancellationToken))
      {
         var sheetName = rule.FileName.ToValidName(30);

         var options = new WorksheetOptions();
         ApplyColumnWidths(options, columns);

         await spreadsheet.StartWorksheetAsync(sheetName, options, token: cancellationToken);

         var headerStyleId = AddHeaderStyle(spreadsheet);
         await AddHeaderRowAsync(spreadsheet, columns, headerStyleId, cancellationToken);

         foreach (var item in list)
         {
            var row = new List<Cell>(columns.Count);

            foreach (var column in columns)
            {
               var raw = column.Property.GetValue(item);
               var formatted = ValueFormatter.FormatForXlsx(raw, column.Rule);

               row.Add(formatted == null ? new Cell(string.Empty) : CreateCell(formatted));
            }

            await spreadsheet.AddRowAsync(row, cancellationToken);
         }

         await spreadsheet.FinishAsync(cancellationToken);
      }

      var bytes = ms.ToArray();

      if (bytes.Length < ExportLimits.ZipThresholdBytes)
      {
         return new ExportFile(fileName, MimeTypes.Xlsx, bytes);
      }

      var zipped = ZipHelper.CreateZip(fileName,
         MimeTypes.Xlsx,
         new List<byte[]>
         {
            bytes
         });
      return zipped;
   }

   // Optional sync convenience wrapper if you still want it
   public static ExportFile Export<T>(IEnumerable<T> data, ExportRule<T> rule)
      where T : class
   {
      return ExportAsync(data, rule, CancellationToken.None)
             .GetAwaiter()
             .GetResult();
   }

   private static IReadOnlyList<ExportColumn> BuildColumns<T>(ExportRule<T> rule)
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

   private static StyleId AddHeaderStyle(Spreadsheet spreadsheet)
   {
      var style = new Style
      {
         Font =
         {
            Bold = true
         }
      };

      return spreadsheet.AddStyle(style);
   }

   private static async Task AddHeaderRowAsync(Spreadsheet spreadsheet,
      IReadOnlyList<ExportColumn> columns,
      StyleId headerStyleId,
      CancellationToken cancellationToken)
   {
      var headers = new string[columns.Count];

      for (var i = 0; i < columns.Count; i++)
      {
         headers[i] = columns[i].Rule.ColumnName;
      }

      await spreadsheet.AddHeaderRowAsync(headers, headerStyleId, cancellationToken);
   }

   private static void ApplyColumnWidths(WorksheetOptions options, IReadOnlyList<ExportColumn> columns)
   {
      for (var i = 0; i < columns.Count; i++)
      {
         var rule = columns[i].Rule;
         var width = rule.ColumnWidth;

         if (!width.HasValue)
         {
            var headerLength = rule.ColumnName.Length;
            width = Math.Clamp(headerLength + 2, 10, 30);
         }

         options.Column(i + 1)
                .Width = width.Value;
      }
   }

   private static Cell CreateCell(object value)
   {
      return value switch
      {
         string s => new Cell(s),
         bool b => new Cell(b),
         int i => new Cell(i),
         long l => new Cell(l),
         short sh => new Cell(sh),
         byte b8 => new Cell(b8),
         uint ui => new Cell((double)ui),
         ulong ul => new Cell((double)ul),
         float f => new Cell((double)f),
         double d => new Cell(d),
         decimal m => new Cell((double)m),
         DateTime dt => new Cell(dt),
         _ => new Cell(value.ToString() ?? string.Empty)
      };
   }
}