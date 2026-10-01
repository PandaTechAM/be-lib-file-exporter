using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace FileExporter.Pdf;

/// <summary>
///     Draws a planned table with PDFsharp and makes no layout decisions of its own: the title and the header row on
///     every page, body rows with a hairline under each, and "x / y" page numbers added once the page count is known.
///     One instance renders one document; its metrics are what the plan was measured with.
/// </summary>
internal sealed class PdfTableRenderer : IDisposable
{
    private const string Creator = "PandaTech.FileExporter";
    private const double MeasureSize = 100;
    private const double TitleSize = 11;
    private const double FooterSize = 7;
    private const double BandHeight = 18;
    private const double FooterBaseline = 12;
    private const double LineSpacing = 1.25;
    private const double RowPadding = 1.5;
    private const double HeaderRuleWidth = 0.75;
    private const double RowRuleWidth = 0.25;

    private static readonly XColor HeaderFill = XColor.FromArgb(0xF0, 0xF0, 0xF6);
    private static readonly XColor HeaderRule = XColor.FromArgb(0x80, 0x80, 0x80);
    private static readonly XColor RowRule = XColor.FromArgb(0xDC, 0xDC, 0xDC);

    // PDFsharp's XBrushes and XStringFormats getters build a new object on every access; these are read-only.
    private static readonly XBrush Ink = XBrushes.Black;
    private static readonly XStringFormat OnBaseline = XStringFormats.BaseLineLeft;

    private readonly IReadOnlyList<PdfColumn> _columns;
    private readonly XGraphics _measure;
    private readonly string _title;

    public PdfTableRenderer(string title, IReadOnlyList<PdfColumn> columns)
    {
        _title = title;
        _columns = columns;
        _measure = XGraphics.CreateMeasureContext(new XSize(1, 1), XGraphicsUnit.Point, XPageDirection.Downwards);
        Regular = CreateMetrics(false);
        Bold = CreateMetrics(true);
    }

    public TextMetrics Regular { get; }

    public TextMetrics Bold { get; }

    public void Dispose()
    {
        _measure.Dispose();
    }

    public byte[] Render(PdfTablePlan plan, IEnumerable<string[]> rows, CancellationToken ct)
    {
        using var document = new PdfDocument();
        document.Info.Title = _title;
        document.Info.Creator = Creator;
        document.Options.CompressContentStreams = true;

        var body = new TextStyle(PdfFonts.Create(plan.FontSize, false),
            PdfFonts.CreateFallback(plan.FontSize, false),
            Regular);

        var header = new TextStyle(PdfFonts.Create(plan.FontSize, true),
            PdfFonts.CreateFallback(plan.FontSize, true),
            Bold);

        var title = new TextStyle(PdfFonts.Create(TitleSize, true), PdfFonts.CreateFallback(TitleSize, true), Bold);
        var titleLine = FitTitle(plan);

        var lineHeight = plan.FontSize * LineSpacing;
        var bottom = plan.Page.Height - PdfLayout.Margin;
        var headerTop = PdfLayout.Margin + BandHeight;

        // The header keeps room for one body line under it, and a body row may take the whole rest of the page.
        var headerLines = NewLineBuffers();
        var headerLineCount = Wrap(_columns.Select(c => c.Header).ToArray(),
            headerLines,
            plan,
            Bold,
            MaxLines(bottom - headerTop, lineHeight, 1));

        var headerHeight = RowHeight(headerLineCount, lineHeight);
        var firstRowTop = headerTop + headerHeight;
        var maxBodyLines = MaxLines(bottom - firstRowTop, lineHeight, 0);

        var bodyLines = NewLineBuffers();
        var rowPen = new XPen(RowRule, RowRuleWidth);
        var headerPen = new XPen(HeaderRule, HeaderRuleWidth);
        var headerBrush = new XSolidBrush(HeaderFill);

        XGraphics? gfx = null;
        var y = 0d;

        try
        {
            foreach (var cells in rows)
            {
                var lineCount = Wrap(cells, bodyLines, plan, Regular, maxBodyLines);
                var height = RowHeight(lineCount, lineHeight);

                if (gfx is null || (y + height > bottom && y > firstRowTop))
                {
                    gfx = NewPage();
                }

                DrawRow(gfx, bodyLines, plan, y, lineHeight, body);
                DrawRule(gfx, rowPen, plan, y + height);
                y += height;
            }

            gfx ??= NewPage();
        }
        finally
        {
            gfx?.Dispose();
        }

        DrawPageNumbers(document, plan);

        using var stream = new MemoryStream();
        document.Save(stream, false);

        return stream.ToArray();

        XGraphics NewPage()
        {
            gfx?.Dispose();
            ct.ThrowIfCancellationRequested();

            var page = document.AddPage();
            page.Width = XUnit.FromPoint(plan.Page.Width);
            page.Height = XUnit.FromPoint(plan.Page.Height);

            var pageGfx = XGraphics.FromPdfPage(page);
            title.Draw(pageGfx, titleLine, PdfLayout.Margin, PdfLayout.Margin + title.Ascent);

            pageGfx.DrawRectangle(headerBrush, PdfLayout.Margin, headerTop, plan.Widths.Sum(), headerHeight);
            DrawRow(pageGfx, headerLines, plan, headerTop, lineHeight, header);
            DrawRule(pageGfx, headerPen, plan, headerTop + headerHeight);

            y = firstRowTop;

            return pageGfx;
        }
    }

    private static int MaxLines(double space, double lineHeight, int reservedLines)
    {
        var lines = (int)Math.Floor((space - 2 * RowPadding) / lineHeight) - reservedLines;

        return Math.Max(1, lines);
    }

    private static double RowHeight(int lineCount, double lineHeight)
    {
        return lineCount * lineHeight + 2 * RowPadding;
    }

    private static void DrawRule(XGraphics gfx, XPen pen, PdfTablePlan plan, double y)
    {
        gfx.DrawLine(pen, PdfLayout.Margin, y, PdfLayout.Margin + plan.Widths.Sum(), y);
    }

    private TextMetrics CreateMetrics(bool bold)
    {
        var font = PdfFonts.Create(MeasureSize, bold);
        var fallback = PdfFonts.CreateFallback(MeasureSize, bold);

        return new TextMetrics(c => Advance(c, PdfFonts.IsFallback(c) ? fallback : font));
    }

    /// <summary>
    ///     The advance at 1 pt. A character the font has no glyph for is drawn by PDF viewers at the default width of
    ///     1 em, not at the .notdef advance PDFsharp measures, so it counts as 1 em. A surrogate pair draws as one glyph
    ///     no wider than that, so its high half carries 1 em and its low half nothing.
    /// </summary>
    private double Advance(char c, XFont font)
    {
        if (char.IsLowSurrogate(c))
        {
            return 0;
        }

        if (char.IsHighSurrogate(c) || GlyphHelper.GlyphIndexFromCodePoint(c, font) == 0)
        {
            return 1;
        }

        return _measure.MeasureString(c.ToString(), font).Width / MeasureSize;
    }

    private List<string>[] NewLineBuffers()
    {
        return _columns.Select(_ => new List<string>()).ToArray();
    }

    private string FitTitle(PdfTablePlan plan)
    {
        var lines = new List<string>(1);
        Bold.Wrap(_title, PdfLayout.UsableWidth(plan.Page) / TitleSize, 1, lines);

        return lines[0];
    }

    /// <summary>Wraps every cell of a row into its column's buffer and returns the row's line count.</summary>
    private int Wrap(string[] cells, List<string>[] linesByColumn, PdfTablePlan plan, TextMetrics metrics, int maxLines)
    {
        var lineCount = 1;

        for (var c = 0; c < _columns.Count; c++)
        {
            var lines = linesByColumn[c];
            lines.Clear();

            var contentWidth = (plan.Widths[c] - 2 * PdfLayout.CellPadding) / plan.FontSize;
            metrics.Wrap(cells[c], contentWidth, maxLines, lines);

            lineCount = Math.Max(lineCount, lines.Count);
        }

        return lineCount;
    }

    private void DrawRow(XGraphics gfx,
        List<string>[] linesByColumn,
        PdfTablePlan plan,
        double top,
        double lineHeight,
        TextStyle style)
    {
        var left = PdfLayout.Margin;
        var firstBaseline = top + RowPadding + (lineHeight - style.Height) / 2 + style.Ascent;

        for (var c = 0; c < _columns.Count; c++)
        {
            var width = plan.Widths[c];
            var baseline = firstBaseline;

            foreach (var line in linesByColumn[c])
            {
                var x = _columns[c].RightAligned
                    ? left + width - PdfLayout.CellPadding - style.Width(line)
                    : left + PdfLayout.CellPadding;

                style.Draw(gfx, line, x, baseline);
                baseline += lineHeight;
            }

            left += width;
        }
    }

    private void DrawPageNumbers(PdfDocument document, PdfTablePlan plan)
    {
        var footer = new TextStyle(PdfFonts.Create(FooterSize, false), PdfFonts.CreateFallback(FooterSize, false), Regular);
        var total = document.PageCount;
        var totalText = total.ToString(CultureInfo.InvariantCulture);

        for (var i = 0; i < total; i++)
        {
            var text = $"{(i + 1).ToString(CultureInfo.InvariantCulture)} / {totalText}";

            using var gfx = XGraphics.FromPdfPage(document.Pages[i], XGraphicsPdfPageOptions.Append);
            footer.Draw(gfx, text, (plan.Page.Width - footer.Width(text)) / 2, plan.Page.Height - FooterBaseline);
        }
    }

    /// <summary>
    ///     One size and weight of text. Runs holding the fallback characters are drawn in the fallback font, all on the
    ///     primary font's baseline, at offsets taken from the same metrics the layout used.
    /// </summary>
    private sealed class TextStyle(XFont font, XFont fallback, TextMetrics metrics)
    {
        public double Ascent { get; } = font.Size * font.Metrics.Ascent / font.Metrics.UnitsPerEm;

        public double Height { get; } =
            font.Size * (font.Metrics.Ascent + font.Metrics.Descent) / font.Metrics.UnitsPerEm;

        public double Width(string text)
        {
            return metrics.Width(text) * font.Size;
        }

        public void Draw(XGraphics gfx, string text, double x, double baseline)
        {
            if (text.Length == 0)
            {
                return;
            }

            if (!PdfFonts.NeedsFallback(text))
            {
                gfx.DrawString(text, font, Ink, x, baseline, OnBaseline);
                return;
            }

            foreach (var (run, isFallback) in PdfFonts.SplitRuns(text))
            {
                gfx.DrawString(run, isFallback ? fallback : font, Ink, x, baseline, OnBaseline);
                x += Width(run);
            }
        }
    }
}
