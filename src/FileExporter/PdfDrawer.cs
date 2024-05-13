using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PageOrientation = PdfSharpCore.PageOrientation;

namespace FileExporter;

internal class PdfDrawer<T>
{
    private readonly string _fontName;
    private readonly double _fontSize;
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
    private readonly double rowHeight = 0;

    private readonly List<XGraphics> _graphicsList = [];
    private readonly List<double> _columnWidths;
    private readonly int _documentsInRow = 0;

    private readonly string _name;
    private readonly IEnumerable<string> _headers;

    static PdfDrawer()
    {
        if (GlobalFontSettings.FontResolver is not FontResolver)
        {
            GlobalFontSettings.FontResolver = new FontResolver();
        }
    }

    internal PdfDrawer(DataTable<T> dataTable, string fontName, int fontSize, PageOrientation pageOrientation, PageSize pageSize)
    {
        _name = dataTable.Name;
        _headers = dataTable.Headers;
        _pageOrientation = pageOrientation;
        _pageSize = pageSize;
        _currentX = 0;
        _currentY = 0;
        _columnWidths = [];
        _fontName = fontName;
        _fontSize = fontSize;

        _document = new PdfDocument();
        var page = _document.AddPage();
        page.Size = _pageSize;
        page.Orientation = _pageOrientation;
        _pageWidth = page.Width;
        _pageHeight = page.Height;

        var graphics = XGraphics.FromPdfPage(page);

        foreach (var t in _headers)
        {
            var records = dataTable.GetRecordsForExport().ToArray();
            double recordsMaxLength = 0;
            if (records.Length != 0)
            {
                recordsMaxLength = records.Select(x => graphics.MeasureString(x[t], Font(_fontSize)).Width)
                    .Max();
            }
            _columnWidths.Add(
                              Math.Max(recordsMaxLength,
                                       graphics.MeasureString(t, Font(_fontSize, true)).Width) + 5);
        }

        _documentsInRow = 1;

        rowHeight = graphics.MeasureString("Test", Font(_fontSize)).Height + 4;

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

    internal void AddRow(IReadOnlyList<string> values, XFont font, bool gridUp, bool gridDown, bool gridLeft,
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
            {
                currentGraphics.DrawLine(
                    XPens.Black, NormalizeX(_currentX), NormalizeY(_currentY),
                    NormalizeX(_currentX + _columnWidths[index] + 2 * CELL_PADDING), NormalizeY(_currentY));
            }

            if (gridDown)
            {
                currentGraphics.DrawLine(XPens.Black, NormalizeX(_currentX), NormalizeY(_currentY + cellHeight),
                    NormalizeX(_currentX + _columnWidths[index] + 2 * CELL_PADDING),
                    NormalizeY(_currentY + cellHeight));
            }

            if (gridLeft)
            {
                currentGraphics.DrawLine(XPens.Black, NormalizeX(_currentX), NormalizeY(_currentY),
                    NormalizeX(_currentX), NormalizeY(_currentY + cellHeight));
            }

            if (gridRight)
            {
                currentGraphics.DrawLine(XPens.Black,
                    NormalizeX(_currentX) + _columnWidths[index] + 2 * CELL_PADDING, NormalizeY(_currentY),
                    NormalizeX(_currentX + _columnWidths[index] + 2 * CELL_PADDING),
                    NormalizeY(_currentY + cellHeight));
            }

            _currentX += _columnWidths[index] + 2 * CELL_PADDING;
        }

        _currentY += cellHeight;
    }

    internal void AddSpacing(double x)
    {
        _currentY += x;
    }

    internal void AddRow(string value, XFont font)
    {
        _currentX = DOCUMENT_PADDING;
        var graphics = _graphicsList.TakeLast(_documentsInRow).First();
        var cellHeight = (int)graphics.MeasureString("Test", font).Height + 4;
        graphics.DrawString(value, font, XBrushes.Black, _currentX + 2, _currentY + cellHeight - 2);
        _currentY += cellHeight;

        // TODO: Add new page if needed
        // TODO: Handle long values
    }

    internal void AddDocumentHeader()
    {
        AddRow(_name, Font(_fontSize * 2, true));
    }

    internal void AddTableHeaders()
    {
        AddRow(_headers.ToList(), Font(_fontSize * 1, true), true, true, true, true);
    }

    internal byte[] GetBytes()
    {
        var stream = new System.IO.MemoryStream();

        _document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;

        _document.Save(stream);

        return stream.ToArray();
    }

    internal void AddTableRows(IEnumerable<IDictionary<string, string>> records, bool headerOnEachPage = true)
    {
        var neddUpperLine = false;

        foreach (var row in records)
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

            AddRow(row.Values.ToList(), Font(_fontSize), neddUpperLine, true, true, true);
            neddUpperLine = false;
        }
    }

    private double NormalizeX(double x)
    {
        return x % _pageWidth;
    }

    private double NormalizeY(double y)
    {
        return y % _pageHeight;
    }

    private XFont Font(double fontSize, bool bold = false)
    {
        return new XFont(_fontName, fontSize, bold ? XFontStyle.Bold : XFontStyle.Regular,
            new XPdfFontOptions(PdfFontEncoding.Unicode));
    }
}
