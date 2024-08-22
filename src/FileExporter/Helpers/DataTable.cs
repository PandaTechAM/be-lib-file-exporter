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
using Microsoft.OpenApi.Extensions;
using PageOrientation = PdfSharpCore.PageOrientation;

namespace FileExporter.Helpers;

internal class DataTable<T>
{
    private readonly IEnumerable<PropertyData> _properties;
    private readonly IEnumerable<T> _records;

    internal string Name { get; set; }
    internal List<string> Headers { get; set; }

    internal DataTable()
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
                Name = x.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name
            }).ToList();

        Headers = _properties.Select(x => x.Name).ToList();

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

        _properties = rules
            .Select(x => new PropertyData
            {
                Property = modelProperties[x.PropertyName()],
                HasBaseConverter = false,
                Name = x.ColumnName()
            }).ToList();

        Headers = _properties.Select(x => x.Name).ToList();

        var ruleByDefaultValue = rules
            .Where(x => x.DefaultColumnValue() != null)
            .ToDictionary(x => x.PropertyName(), x => x.DefaultColumnValue());

        foreach (var model in data)
        {
            var properties = model!.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (ruleByDefaultValue.TryGetValue(property.Name, out var value))
                {
                    if (!model.GetType().IsGenericType || model.GetType().Name.Contains("String"))
                    {
                        model.GetType().GetProperty(property.Name)!.SetValue(model, value);
                    }
                }
            }
        }

        _records = data;
    }

    internal IEnumerable<IDictionary<string, string>> GetRecordsForExport()
    {
        foreach (var dataRow in _records)
        {
            var row = new Dictionary<string, string>();

            foreach (var property in _properties)
            {
                var value = property.Property.GetValue(dataRow);

                var toString = ConvertDataToString(value, property.HasBaseConverter);

                row.Add(property.Name, toString);
            }

            yield return row;
        }
    }

    internal List<byte[]> ToCsv()
    {
        var records = GetRecordsForExport();

        var recordsChunks = records.Chunk(Constants.CsvLinesCount);

        var files = new List<byte[]>();

        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", Headers));

        foreach (var chunk in recordsChunks)
        {
            foreach (var record in chunk)
            {
                csv.AppendLine(string.Join(",", record.Values.Select(Encapsulate)));
            }
        }

        var data = Encoding.UTF8
            .GetBytes(csv.ToString().Trim());

        var file = Encoding.UTF8
            .GetPreamble()
            .Concat(data)
            .ToArray();

        files.Add(file);

        return files;
    }

    internal List<byte[]> ToXlsx()
    {
        var records = GetRecordsForExport();

        var recordsChunks = records.Chunk(Constants.ExcelLinesCount);

        var files = new List<byte[]>();

        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(Name.ToValidName());

        for (var i = 0; i < Headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = Headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        worksheet.SheetView.FreezeRows(1);
        worksheet.RangeUsed().SetAutoFilter(true);
        worksheet.Columns().AdjustToContents();

        foreach (var chunk in recordsChunks)
        {
            for (var i = 0; i < chunk.Length; i++)
            {
                for (var j = 0; j < Headers.Count; j++)
                {
                    worksheet.Cell(i + 2, j + 1).Value = chunk[i][Headers[j]];
                }
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        files.Add(stream.ToArray());

        return files;
    }

    internal List<byte[]> ToPdf(bool headerOnEachPage, string fontName, int fontSize, PdfSharpCore.PageSize pageSize,
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
                else if (value.GetType().IsArray)
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