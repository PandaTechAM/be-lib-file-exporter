using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using FileExporter.Dtos;
using FileExporter.Extensions;
using FileExporter.Rules;
using Microsoft.OpenApi.Extensions;
using PdfSharpCore;
using PageOrientation = PdfSharpCore.PageOrientation;

namespace FileExporter.Helpers;

internal class DataTable<T>
{
   private readonly IEnumerable<PropertyData> _properties;
   private readonly IEnumerable<T> _records;
   private readonly Dictionary<string, object?> _defaults = new();

   private DataTable()
   {
      _records = [];
      _properties = [];
      Name = string.Empty;
      Headers = [];
   }

   internal DataTable(IEnumerable<T> data, string name)
      : this()
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(name);
      ArgumentNullException.ThrowIfNull(data);

      Name = name;

      _properties = typeof(T).GetProperties()
                             .Select(x => new PropertyData
                             {
                                Property = x,
                                HasBaseConverter = false,
                                Name = x.GetCustomAttribute<DisplayNameAttribute>()
                                        ?.DisplayName ?? x.Name
                             })
                             .ToList();

      Headers = _properties.Select(x => x.Name)
                           .ToList();

      _records = data;
   }

   internal DataTable(IEnumerable<T> data, string name, IEnumerable<IPropertyRule> rules)
      : this()
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(name);
      ArgumentNullException.ThrowIfNull(data);

      Name = name;

      var modelProperties = typeof(T).GetProperties()
                                     .ToDictionary(x => x.Name, x => x);

      _properties = rules.Select(r => new PropertyData
                         {
                            Property = modelProperties[r.PropertyName()],
                            HasBaseConverter = false,
                            Name = r.ColumnName()
                         })
                         .ToList();

      Headers = _properties.Select(p => p.Name)
                           .ToList();

      // collect defaults per *model* property name
      foreach (var r in rules)
      {
         var def = r.DefaultColumnValue();
         if (def != null)
         {
            _defaults[r.PropertyName()] = def;
         }
      }

      _records = data;
   }

   internal string Name { get; set; }
   internal List<string> Headers { get; set; }

   internal IEnumerable<IDictionary<string, string>> GetRecordsForExport()
   {
      foreach (var dataRow in _records)
      {
         var row = new Dictionary<string, string>();

         foreach (var property in _properties)
         {
            var raw = property.Property.GetValue(dataRow);

            // If the model value is null and we have a default for that model property, use it
            if (raw is null && _defaults.TryGetValue(property.ModelPropertyName, out var def))
            {
               raw = def;
            }

            var toString = ConvertDataToString(raw, property.HasBaseConverter);
            row.Add(property.Name, toString);
         }

         yield return row;
      }
   }

   internal List<byte[]> ToCsv()
   {
      var records = GetRecordsForExport();
      var chunks = records.Chunk(Constants.CsvLinesCount);

      var files = new List<byte[]>();

      foreach (var chunk in chunks)
      {
         var sb = new StringBuilder();
         sb.AppendLine(string.Join(",", Headers));

         foreach (var record in chunk)
         {
            sb.AppendLine(string.Join(",", record.Values.Select(Encapsulate)));
         }

         var data = Encoding.UTF8.GetBytes(sb.ToString()
                                             .TrimEnd());
         var file = Encoding.UTF8
                            .GetPreamble()
                            .Concat(data)
                            .ToArray();
         files.Add(file);
      }

      return files;
   }

   internal List<byte[]> ToXlsx()
   {
      var records = GetRecordsForExport();
      var recordsChunks = records.Chunk(Constants.ExcelLinesCount);

      var files = new List<byte[]>();

      foreach (var chunk in recordsChunks)
      {
         using var workbook = new XLWorkbook();
         var ws = workbook.Worksheets.Add(Name.ToValidName());

         // header
         for (var c = 0; c < Headers.Count; c++)
         {
            var cell = ws.Cell(1, c + 1);
            cell.Value = Headers[c];
            cell.Style.Font.Bold = true;
         }

         ws.SheetView.FreezeRows(1);
         ws.Range(1, 1, 1, Headers.Count)
           .SetAutoFilter();

         // data
         var row = 2;
         foreach (var t in chunk)
         {
            for (var j = 0; j < Headers.Count; j++)
            {
               var cell = ws.Cell(row, j + 1);
               cell.Value = t[Headers[j]];
               cell.Style.NumberFormat.Format = "@";
            }

            row++;
         }

         // cheap width based on header only
         for (var c = 1; c <= Headers.Count; c++)
         {
            ws.Column(c)
              .Width = Math.Max(Headers[c - 1].Length + 2, 10);
         }

         using var stream = new MemoryStream();
         workbook.SaveAs(stream);
         files.Add(stream.ToArray());
      }

      return files;
   }

   internal List<byte[]> ToPdf(bool headerOnEachPage,
      string fontName,
      int fontSize,
      PageSize pageSize,
      PageOrientation pageOrientation)
   {
      var records = GetRecordsForExport();

      var recordsChunks = records.Chunk(Constants.PdfLinesCount);

      var files = new List<byte[]>();

      var pdfDrawer = new PdfDrawer<T>(this, fontName, fontSize, pageOrientation, pageSize);

      pdfDrawer.AddSpacing(10);
      pdfDrawer.AddDocumentHeader();
      pdfDrawer.AddSpacing(10);
      pdfDrawer.AddTableHeaders();

      foreach (var item in recordsChunks)
      {
         pdfDrawer.AddTableRows(item, headerOnEachPage);
      }

      files.Add(pdfDrawer.GetBytes());

      return files;
   }

   private static bool IsCollectionType(Type type)
   {
      return type.GetInterface(nameof(ICollection)) != null;
   }

   private static string ConvertDataToString(object? value, bool hasBaseConverter)
   {
      string stringValue;
      switch (value)
      {
         case null:
            stringValue = string.Empty;
            break;
         case string s:
            stringValue = s;
            break;
         case DateTime d:
            stringValue = d.ToString("yyyy-MM-dd HH:mm:ss");
            break;
         case Enum e:
            stringValue = e.GetDisplayName();
            break;
         case bool b:
            stringValue = b ? "Yes" : "No";
            break;
         default:
         {
            if (Constants.NumericTypesWithNullables.Contains(value.GetType())
                && hasBaseConverter)
            {
               stringValue = value.ToString() ?? string.Empty;
            }
            else if (value.GetType()
                          .IsArray)
            {
               List<string> list = (from object? obj in (value as IEnumerable)!
                                    select ConvertDataToString(obj, hasBaseConverter)).ToList();

               stringValue = string.Join(';', list);
            }
            else if (IsCollectionType(value.GetType()))
            {
               List<string> list = (from object? obj in (value as IEnumerable)!
                                    select ConvertDataToString(obj, hasBaseConverter)).ToList();

               stringValue = string.Join(';', list);
            }
            else
            {
               stringValue = value.ToString() ?? string.Empty;
            }

            break;
         }
      }

      return stringValue;
   }

   private static string Encapsulate(string value)
   {
      if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
      {
         value = value.Replace("\"", "\"\"");
         value = $"\"{value}\"";
      }

      return value;
   }
}