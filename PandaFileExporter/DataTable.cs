using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseConverter;
using ClosedXML.Excel;
using Microsoft.OpenApi.Extensions;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PageOrientation = PdfSharpCore.PageOrientation;

namespace PandaFileExporter;

public class DataTable
{
    public string Name { get; set; } = null!;
    public List<string> Headers { get; set; } = [];
    public List<Dictionary<string, string>> Rows { get; set; } = [];

    public static DataTable FromQueryable<T>(IQueryable<T>? data)
    {
        var table = new DataTable();
        if (data == null) return table;

        var displayName = typeof(T).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
        displayName ??= typeof(T).Name + " {DateTime}";
        displayName = displayName.Replace("{DateTime}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        table.Name = displayName;

        return Table(table, data);
    }

    public static DataTable FromQueryable<T>(IQueryable<T>? data, string name)
    {
        var table = new DataTable();
        if (data == null) return table;

        table.Name = name;

        return Table(table, data);
    }

    private static DataTable Table<T>(DataTable table, IQueryable<T>? data)
    {
        var properties = typeof(T).GetProperties()
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

        foreach (var dataRow in data!)
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
                    && hasBaseConverter)
                {
                    stringValue = value.ToString().Base36String() ?? "";
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

    private static string Encapsulate(string value)
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

        var data = Encoding.UTF8.GetBytes(csv.ToString().Trim());
        return Encoding.UTF8.GetPreamble().Concat(data).ToArray();
    }

    public byte[] ToXlsx()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(ValidName(Name));
        for (var i = 0; i < Headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = Headers[i];
        }

        for (var i = 0; i < Rows.Count; i++)
        {
            var row = Rows[i];
            for (var j = 0; j < Headers.Count; j++)
            {
                worksheet.Cell(i + 2, j + 1).Value = row[Headers[j]];
            }
        }

        var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string ValidName(string name)
    {
        var invalidChars = "\0\u0003:\\/?*[]".ToCharArray();

        var validName = name;
        foreach (var invalidChar in invalidChars)
        {
            validName = validName.Replace(invalidChar, '_');
        }

        return validName[..Math.Min(validName.Length, 30)];
    }

    public byte[] ToPdf(bool headerOnEachPage = false, PageSize pageSize = PageSize.A4,
        PageOrientation pageOrientation = PageOrientation.Landscape)
    {
        var pdfDrawer = new PdfDrawer(this, pageOrientation, pageSize);

        pdfDrawer.AddSpacing(10);
        pdfDrawer.AddDocumentHeader();
        pdfDrawer.AddSpacing(10);
        pdfDrawer.AddTableHeaders();
        pdfDrawer.AddTableRows(headerOnEachPage);

        return pdfDrawer.GetBytes();
    }


    private class PdfDrawer
    {
        private const string FONT_NAME = "Calibri";
        private const double FONT_SIZE = 10;
        private const double DOCUMENT_PADDING = 15;
        private const double CELL_PADDING = 5;
        private readonly PageOrientation _pageOrientation;
        private readonly PageSize _pageSize;


        private double _currentX;
        private double _currentY;
        private readonly PdfDocument _document;
        private readonly double _pageWidth;
        private readonly double _pageHeight;
        private int pages = 1;
        private double rowHeight = 0;


        private readonly List<XGraphics> _graphicsList = new();
        private readonly List<double> _columnWidths;
        private int _documentsInRow = 0;

        private DataTable _table;

        public PdfDrawer(DataTable table, PageOrientation pageOrientation, PageSize pageSize)
        {
            _table = table;
            _pageOrientation = pageOrientation;
            _pageSize = pageSize;
            _currentX = 0;
            _currentY = 0;

            _columnWidths = new List<double>();

            _document = new PdfDocument();
            var page = _document.AddPage();
            page.Size = _pageSize;
            page.Orientation = _pageOrientation;
            _pageWidth = page.Width;
            _pageHeight = page.Height;

            var graphics = XGraphics.FromPdfPage(page);
            foreach (var t in table.Headers)
            {
                _columnWidths.Add(Math.Max(
                    table.Rows.Select(x => graphics.MeasureString(x[t], Font(FONT_SIZE)).Width).Max(),
                    graphics.MeasureString(t, Font(FONT_SIZE, true)).Width) + 5);
            }

            _documentsInRow = 1;
            rowHeight = graphics.MeasureString("Test", Font(FONT_SIZE)).Height + 4;

            _graphicsList.Add(graphics);
            var currentGraphicsIndex = 0;
            var x = DOCUMENT_PADDING;
            foreach (var t in _columnWidths)
            {
                if ((currentGraphicsIndex + 1) * _pageWidth < x + t + DOCUMENT_PADDING + 2 * CELL_PADDING)
                {
                    currentGraphicsIndex++;
                    var newPage = _document.AddPage();
                    newPage.Size = _pageSize;
                    newPage.Orientation = _pageOrientation;

                    _graphicsList.Add(XGraphics.FromPdfPage(newPage));
                    x = currentGraphicsIndex * _pageWidth + DOCUMENT_PADDING;
                    _documentsInRow++;
                }

                x += t + CELL_PADDING * 2;
            }
        }

        private XFont Font(double fontSize, bool bold = false)
        {
            return new XFont(FONT_NAME, fontSize, bold ? XFontStyle.Bold : XFontStyle.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));
        }

        public void AddRow(IReadOnlyList<string> values, XFont font, bool gridUp, bool gridDown, bool gridLeft,
            bool gridRight)
        {
            _currentX = DOCUMENT_PADDING;
            var graphics = _graphicsList.TakeLast(_documentsInRow).ToList();
            var currentGraphicsIndex = 0;
            var currentGraphics = graphics[currentGraphicsIndex];
            var cellHeight = currentGraphics.MeasureString("Test", font).Height + 4;

            for (var index = 0; index < values.Count; index++)
            {
                var value = values[index];
                if ((currentGraphicsIndex + 1) * _pageWidth <
                    _currentX + _columnWidths[index] + DOCUMENT_PADDING + 2 * CELL_PADDING)
                {
                    currentGraphicsIndex++;
                    currentGraphics = graphics[currentGraphicsIndex];
                    _currentX = currentGraphicsIndex * _pageWidth + DOCUMENT_PADDING;
                }

                currentGraphics.DrawString(value, font, XBrushes.Black, NormalizeX(_currentX + CELL_PADDING),
                    NormalizeY(_currentY + cellHeight - 2));

                if (gridUp)
                    currentGraphics.DrawLine(
                        XPens.Black, NormalizeX(_currentX), NormalizeY(_currentY),
                        NormalizeX(_currentX + _columnWidths[index] + 2 * CELL_PADDING), NormalizeY(_currentY));
                if (gridDown)
                    currentGraphics.DrawLine(XPens.Black, NormalizeX(_currentX), NormalizeY(_currentY + cellHeight),
                        NormalizeX(_currentX + _columnWidths[index] + 2 * CELL_PADDING),
                        NormalizeY(_currentY + cellHeight));
                if (gridLeft)
                    currentGraphics.DrawLine(XPens.Black, NormalizeX(_currentX), NormalizeY(_currentY),
                        NormalizeX(_currentX), NormalizeY(_currentY + cellHeight));
                if (gridRight)
                    currentGraphics.DrawLine(XPens.Black,
                        NormalizeX(_currentX) + _columnWidths[index] + 2 * CELL_PADDING, NormalizeY(_currentY),
                        NormalizeX(_currentX + _columnWidths[index] + 2 * CELL_PADDING),
                        NormalizeY(_currentY + cellHeight));

                _currentX += _columnWidths[index] + 2 * CELL_PADDING;
            }

            _currentY += cellHeight;
        }

        private double NormalizeX(double x)
        {
            return x % _pageWidth;
        }

        private double NormalizeY(double y)
        {
            return y % _pageHeight;
        }


        public void AddSpacing(double x)
        {
            _currentY += x;
        }

        public void AddRow(string value, XFont font)
        {
            _currentX = DOCUMENT_PADDING;
            var graphics = _graphicsList.TakeLast(_documentsInRow).First();
            var cellHeight = (int)graphics.MeasureString("Test", font).Height + 4;

            graphics.DrawString(value, font, XBrushes.Black, _currentX + 2, _currentY + cellHeight - 2);
            _currentY += cellHeight;

            // TODO: Add new page if needed
            // TODO: Handle long values
        }

        public void AddDocumentHeader()
        {
            AddRow(_table.Name, Font(FONT_SIZE * 2, true));
        }


        public void AddTableHeaders()
        {
            AddRow(_table.Headers, Font(FONT_SIZE * 1, true), true, true, true, true);
        }

        public byte[] GetBytes()
        {
            var stream = new System.IO.MemoryStream();

            _document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;

            _document.Save(stream);

            return stream.ToArray();
        }

        public void AddTableRows(bool headerOnEachPage = false)
        {
            var neddUpperLine = false;

            foreach (var row in _table.Rows)
            {
                if (_currentY + 2 * DOCUMENT_PADDING + rowHeight > _pageHeight * pages)
                {
                    for (var x = 0; x < _documentsInRow; x++)
                    {
                        var page = _document.AddPage();
                        page.Size = _pageSize;
                        page.Orientation = _pageOrientation;
                        _graphicsList.Add(XGraphics.FromPdfPage(page));
                    }

                    pages++;
                    _currentY = DOCUMENT_PADDING + _pageHeight * (pages - 1);
                    if (headerOnEachPage)
                        AddTableHeaders();
                    else
                        neddUpperLine = true;
                }

                AddRow(row.Values.ToList(), Font(FONT_SIZE), neddUpperLine, true, true, true);
                neddUpperLine = false;
            }
        }
    }
}

public static class DataTableExtender
{
    public static DataTable ToDataTable<T>(this IEnumerable<T>? data, string name)
    {
        return DataTable.FromQueryable(data?.AsQueryable(), name);
    }

    public static DataTable ToDataTable<T>(this IEnumerable<T>? data)
    {
        return DataTable.FromQueryable(data?.AsQueryable());
    }

    public static string Base36String(this object? value)
    {
        if (value is null) return "";

        _ = long.TryParse((string)value, out var convertedValue);

        return PandaBaseConverter.Base10ToBase36(convertedValue) ?? "";
    }
}