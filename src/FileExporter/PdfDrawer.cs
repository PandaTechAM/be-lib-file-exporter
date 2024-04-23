using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PageOrientation = PdfSharpCore.PageOrientation;

namespace FileExporter;

public class PdfDrawer<T>
{
    private const string FONT_NAME = "Arial";
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
    private readonly double rowHeight = 0;

    private readonly List<XGraphics> _graphicsList = [];
    private readonly List<double> _columnWidths;
    private readonly int _documentsInRow = 0;

    private readonly DataTable<T> _table;

    public PdfDrawer(DataTable<T> table, PageOrientation pageOrientation, PageSize pageSize)
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
                table.GetRecordsForExport().Select(x => graphics.MeasureString(x[t], Font(FONT_SIZE)).Width).Max(),
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

        foreach (var row in _table.GetRecordsForExport())
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

    private double NormalizeX(double x)
    {
        return x % _pageWidth;
    }

    private double NormalizeY(double y)
    {
        return y % _pageHeight;
    }

    private static XFont Font(double fontSize, bool bold = false)
    {
        return new XFont(FONT_NAME, fontSize, bold ? XFontStyle.Bold : XFontStyle.Regular,
            new XPdfFontOptions(PdfFontEncoding.Unicode));
    }
}
