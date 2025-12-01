using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Helpers;
using FileExporter.Rules;
using SpreadCheetah;
using SpreadCheetah.Styling;
using SpreadCheetah.Worksheets;

namespace FileExporter.Exporters;

internal static class XlsxExporter
{
   internal static async Task<ExportFile> ExportAsync<T>(IEnumerable<T> data,
      ExportRule<T> rule,
      CancellationToken cancellationToken = default)
      where T : class
   {
      ArgumentNullException.ThrowIfNull(data);
      ArgumentNullException.ThrowIfNull(rule);

      var list = data as IList<T> ?? data.ToList();
      var totalRows = list.Count;

      var columns = BuildColumns(rule);

      var baseName = rule.FileName;
      var singleFileName = NamingHelper.EnsureExtension(baseName, MimeTypes.Xlsx.Extension);

      const int rowsPerSheet = ExportLimits.MaxXlsxRowsPerFile;

      byte[] bytes;

      if (totalRows <= rowsPerSheet)
      {
         // Single sheet
         bytes = await CreateXlsxFileAsync(list, columns, rule, cancellationToken);
      }
      else
      {
         // Multi-sheet single workbook
         bytes = await CreateMultiSheetXlsxFileAsync(list, columns, rule, rowsPerSheet, cancellationToken);
      }

      if (bytes.Length < ExportLimits.ZipThresholdBytes)
      {
         return new ExportFile(singleFileName, MimeTypes.Xlsx, bytes);
      }

      // Zip only when size threshold exceeded
      return ZipHelper.CreateZip(baseName,
         MimeTypes.Xlsx,
         [
            bytes
         ]);
   }

   private static async Task<byte[]> CreateXlsxFileAsync<T>(IEnumerable<T> dataSlice,
      List<ExportColumn> columns,
      ExportRule<T> rule,
      CancellationToken cancellationToken)
      where T : class
   {
      await using var ms = new MemoryStream();

      await using (var spreadsheet = await Spreadsheet.CreateNewAsync(ms, cancellationToken: cancellationToken))
      {
         var sheetName = rule.FileName.ToValidName(30);

         var options = new WorksheetOptions();
         ApplyColumnWidths(options, columns);

         await spreadsheet.StartWorksheetAsync(sheetName, options, token: cancellationToken);

         var headerStyleId = AddHeaderStyle(spreadsheet);
         await AddHeaderRowAsync(spreadsheet, columns, headerStyleId, cancellationToken);

         foreach (var item in dataSlice)
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

      return ms.ToArray();
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
         var col = columns[i];
         var rule = col.Rule;
         var propType = col.Property.PropertyType;
         var underlying = Nullable.GetUnderlyingType(propType) ?? propType;

         double width;

         if (rule.ColumnWidth.HasValue)
         {
            width = rule.ColumnWidth.Value;
         }
         else
         {
            width = InferWidth(rule, underlying);
         }

         var columnOptions = options.Column(i + 1);
         columnOptions.Width = width;

         var format = BuildNumberFormat(rule, propType);

         if (format is not null)
         {
            columnOptions.DefaultStyle = new Style
            {
               Format = NumberFormat.Custom(format)
            };
         }
      }
   }

   private static double InferWidth(IPropertyRule rule, Type underlyingType)
   {
      var formatType = rule.FormatType;

      // If user explicitly chose Text, treat as text column
      if (formatType == ColumnFormatType.Text)
      {
         return ColumnWidthDefaults.FromHeader(rule.ColumnName);
      }

      // Date / DateTime: either explicit format or type-based
      if (formatType == ColumnFormatType.DateTime ||
          underlyingType == typeof(DateTime) ||
          underlyingType == typeof(DateOnly) ||
          underlyingType == typeof(TimeOnly))
      {
         return ColumnWidthDefaults.DateTimeWidth;
      }

      if (formatType == ColumnFormatType.Date)
      {
         return ColumnWidthDefaults.DateWidth;
      }

      // Boolean
      if (formatType == ColumnFormatType.Boolean ||
          underlyingType == typeof(bool))
      {
         return ColumnWidthDefaults.BooleanWidth;
      }

      // Decimal-like
      if (formatType == ColumnFormatType.Decimal ||
          formatType == ColumnFormatType.Currency ||
          formatType == ColumnFormatType.Percentage ||
          underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
      {
         return ColumnWidthDefaults.ForDecimal(rule.Precision);
      }

      // Integer-ish
      if (formatType == ColumnFormatType.Integer ||
          underlyingType == typeof(int) || underlyingType == typeof(long) ||
          underlyingType == typeof(short) || underlyingType == typeof(byte) ||
          underlyingType == typeof(uint) || underlyingType == typeof(ulong) ||
          underlyingType == typeof(ushort))
      {
         return ColumnWidthDefaults.IntegerWidth;
      }

      // Fallback: header-based heuristic for everything else
      return ColumnWidthDefaults.FromHeader(rule.ColumnName);
   }

   private static string? BuildNumberFormat(IPropertyRule rule, Type propertyType)
   {
      var formatType = rule.FormatType;
      var precision = rule.Precision;

      if (formatType == ColumnFormatType.Text)
      {
         return null;
      }

      var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

      var isDecimalLike =
         underlying == typeof(decimal) ||
         underlying == typeof(double) ||
         underlying == typeof(float);

      if (isDecimalLike ||
          formatType == ColumnFormatType.Decimal ||
          formatType == ColumnFormatType.Currency ||
          formatType == ColumnFormatType.Percentage)
      {
         var p = precision ?? 2;

         return formatType switch
         {
            ColumnFormatType.Currency =>
               p <= 0 ? "#,##0" : "#,##0." + new string('0', p),

            ColumnFormatType.Percentage =>
               p <= 0 ? "0%" : "0." + new string('0', p) + "%",

            _ => p <= 0 ? "0" : "0." + new string('0', p)
         };
      }

      if (formatType == ColumnFormatType.Integer ||
          underlying == typeof(int) || underlying == typeof(long) ||
          underlying == typeof(short) || underlying == typeof(byte) ||
          underlying == typeof(uint) || underlying == typeof(ulong) ||
          underlying == typeof(ushort))
      {
         return "0";
      }

      if (formatType == ColumnFormatType.DateTime ||
          underlying == typeof(DateTime) || underlying == typeof(TimeOnly))
      {
         return "yyyy-mm-dd hh:mm:ss";
      }

      if (formatType == ColumnFormatType.Date || underlying == typeof(DateOnly))
      {
         return "yyyy-mm-dd";
      }

      return null;
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
         DateOnly d => new Cell(d.ToDateTime(TimeOnly.MinValue)),
         TimeOnly t => new Cell(DateTime.Today.Add(t.ToTimeSpan())),
         _ => new Cell(value.ToString() ?? string.Empty)
      };
   }

   private static async Task<byte[]> CreateMultiSheetXlsxFileAsync<T>(IList<T> list,
      List<ExportColumn> columns,
      ExportRule<T> rule,
      int rowsPerSheet,
      CancellationToken cancellationToken)
      where T : class
   {
      await using var ms = new MemoryStream();

      await using (var spreadsheet = await Spreadsheet.CreateNewAsync(ms, cancellationToken: cancellationToken))
      {
         // Sanitize once for sheet names
         var sanitizedBaseName = rule.FileName.ToValidName(ExportLimits.MaxSheetNameLength);

         var headerStyleId = AddHeaderStyle(spreadsheet);

         var totalRows = list.Count;
         var sheetIndex = 0;

         for (var offset = 0; offset < totalRows; offset += rowsPerSheet, sheetIndex++)
         {
            var take = Math.Min(rowsPerSheet, totalRows - offset);

            var sheetName = BuildSheetName(sanitizedBaseName, sheetIndex);

            var options = new WorksheetOptions();
            ApplyColumnWidths(options, columns);

            await spreadsheet.StartWorksheetAsync(sheetName, options, token: cancellationToken);
            await AddHeaderRowAsync(spreadsheet, columns, headerStyleId, cancellationToken);

            for (var i = 0; i < take; i++)
            {
               var item = list[offset + i];
               var row = new List<Cell>(columns.Count);

               foreach (var column in columns)
               {
                  var raw = column.Property.GetValue(item);
                  var formatted = ValueFormatter.FormatForXlsx(raw, column.Rule);

                  row.Add(formatted == null ? new Cell(string.Empty) : CreateCell(formatted));
               }

               await spreadsheet.AddRowAsync(row, cancellationToken);
            }
         }

         await spreadsheet.FinishAsync(cancellationToken);
      }

      return ms.ToArray();
   }


   private static string BuildSheetName(string baseName, int sheetIndex)
   {
      const int maxSheetNameLength = ExportLimits.MaxSheetNameLength;
      if (sheetIndex == 0)
      {
         return baseName.Length <= maxSheetNameLength
            ? baseName
            : baseName[..maxSheetNameLength];
      }

      var suffix = "_" + (sheetIndex + 1);
      var baseMaxLen = maxSheetNameLength - suffix.Length;

      if (baseMaxLen <= 0)
      {
         var fallback = "Sheet" + suffix;
         return fallback.Length <= maxSheetNameLength
            ? fallback
            : fallback[..maxSheetNameLength];
      }

      var trimmedBase = baseName.Length <= baseMaxLen
         ? baseName
         : baseName[..baseMaxLen];

      return trimmedBase + suffix;
   }
}