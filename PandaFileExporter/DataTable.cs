using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace PandaFileExporter;

public class DataTable
{
    public string Name { get; set; } = null!;
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; }

    public static DataTable FromQueryable<T>(IQueryable<T>? data)
    {
        var table = new DataTable();
        if (data == null) return table;

        table.Name = typeof(T).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? typeof(T).Name;

        var properties = typeof(T).GetProperties()
            // .Where(x => x.GetCustomAttributes<DisplayNameAttribute>().Any())
            .Select(x => new
            {
                Property = x,
                HasBaseConverter = x.GetCustomAttributes<PandaPropertyBaseConverterAttribute>().Any(),
                Name = x.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name
            }).ToList();

        foreach (var property in properties)
        {
            table.Headers.Add(property.Name);
        }

        table.Rows = new List<Dictionary<string, string>>();

        foreach (var dataRow in data)
        {
            var row = new Dictionary<string, string>();

            foreach (var property in properties)
            {
                var value = property.Property.GetValue(dataRow);

                var toString = ConvertDataToString(value, property.HasBaseConverter);

                row.Add(property.Name, toString);
            }

            table.Rows.Add(row);
        }


        return table;
    }

    static bool IsCollectionType(Type type)
    {
        return type.GetInterface(nameof(ICollection)) != null;
    }

    static string ConvertDataToString(object? value, bool HasBaseConverter)
    {
        string stringValue;
        switch (value)
        {
            case null:
                stringValue = "";
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
                if (NumericTypesWithNullables.Contains(value.GetType())
                    && HasBaseConverter)
                {
                    stringValue = value.ToString().Base36String() ?? "";
                }
                else if (value.GetType().IsArray)
                {
                    List<string> list = (from object? obj in (value as IEnumerable)!
                        select ConvertDataToString(obj, HasBaseConverter)).ToList();

                    stringValue = string.Join(';', list);
                }
                else if (IsCollectionType(value.GetType()))
                {
                    List<string> list = (from object? obj in (value as IEnumerable)!
                        select ConvertDataToString(obj, HasBaseConverter)).ToList();

                    stringValue = string.Join(';', list);
                }
                else
                {
                    stringValue = value.ToString() ?? "";
                }

                break;
            }
        }

        return stringValue;
    }

    private static readonly Type[] NumericTypesWithNullables =
    {
        typeof(int), typeof(double), typeof(decimal), typeof(long),
        typeof(short), typeof(sbyte), typeof(byte), typeof(ulong),
        typeof(ushort), typeof(uint), typeof(float),
        typeof(int?), typeof(double?), typeof(decimal?), typeof(long?),
        typeof(short?), typeof(sbyte?), typeof(byte?), typeof(ulong?),
        typeof(ushort?), typeof(uint?), typeof(float?)
    };

    private string Encapsulate(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            value = $"\"{value}\"";
        }

        return value;
    }

    public byte[] ToCsv()
    {
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", Headers));
        foreach (var row in Rows)
        {
            csv.AppendLine(string.Join(",", row.Values.Select(Encapsulate)));
        }

        var data = Encoding.UTF8.GetBytes(csv.ToString());
        return Encoding.UTF8.GetPreamble().Concat(data).ToArray();
    }


    public byte[] ToXlsx()
    {
        var workbook = new XLWorkbook();
        var length = Math.Min(Name.Length, 30);
        var worksheet = workbook.Worksheets.Add(Name[..length]);
        for (int i = 0; i < Headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = Headers[i];
        }

        for (int i = 0; i < Rows.Count; i++)
        {
            var row = Rows[i];
            for (int j = 0; j < Headers.Count; j++)
            {
                worksheet.Cell(i + 2, j + 1).Value = row[Headers[j]];
            }
        }

        var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ToPdf()
    {
        var columnWidths = new List<double>();

        var fontHeader =
            new XFont("Calibri", 10, XFontStyle.Bold,
                new XPdfFontOptions(PdfFontEncoding.Unicode));

        var fontData = new XFont("Calibri", 10, XFontStyle.Regular,
            new XPdfFontOptions(PdfFontEncoding.Unicode)); 
        
        var document = new PdfDocument();
        var page = document.AddPage();
        page.Orientation = PageOrientation.Landscape;
        var graphics = XGraphics.FromPdfPage(page);

        // Add header 

        graphics.DrawString(Name, fontHeader, XBrushes.Black, 20, 10);

        foreach (var t in Headers)
        {
            columnWidths.Add(Math.Max(
                Rows.Select(x => graphics.MeasureString(x[t], fontData).Width).Max(),
                graphics.MeasureString(t, fontHeader).Width) + 5);
        }


        var x = 20; // Starting X position
        var y = 50; // Starting Y position


        var brush = XBrushes.Black;

        for (int i = 0; i < Headers.Count; i++)
        {
            graphics.DrawString(Headers[i], fontHeader, brush, x + 2, y + 18);
            x += (int)Math.Floor(columnWidths[i]); // Increment X position for the next data cell
        }

        y += 20; // Increase Y position for rows

        foreach (var row in Rows)
        {
            x = 20; // Reset X position for each row
            for (var index = 0; index < Headers.Count; index++)
            {
                var t = Headers[index];
                graphics.DrawString(row[t], fontData, brush, x + 2, y + 18);
                x += (int)Math.Floor(columnWidths[index]); // Increment X position for the next data cell
            }

            y += 20; // Increase Y position for the next row
        }

        // fit width to content
        page.Width = columnWidths.Sum() + 40;

        // ADD LINES TO SEPARATE COLUMNS
        x = 20; // Reset X position for each row
        for (var i = 0; i < Headers.Count; i++)
        {
            graphics.DrawLine(XPens.Black, x, 50, x, y);
            x += (int)Math.Floor(columnWidths[i]); // Increment X position for the next data cell
        }

        graphics.DrawLine(XPens.Black, x, 50, x, y);


        // ADD LINES TO SEPARATE ROWS
        y = 50; // Reset Y position for each row
        for (var i = 0; i < Rows.Count + 2; i++)
        {
            graphics.DrawLine(XPens.Black, 20, y, x, y);
            y += 20; // Increment Y position for the next data cell
        }

        var stream = new System.IO.MemoryStream();

        document.Save(stream);
        return stream.ToArray();
    }
}