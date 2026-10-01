using PdfSharp.Drawing;

namespace FileExporter.Pdf;

/// <summary>
///     Pure layout decisions, after CSS automatic table layout: a column never gets less than its widest unbreakable
///     token, the page is the first that comfortably holds what the columns would like (A2 landscape when none does),
///     and the rest of the width is shared in proportion to how much each column would still grow.
/// </summary>
internal static class PdfLayout
{
    public const double Margin = 24;
    public const double CellPadding = 2.5;

    private const int SampleSize = 5_000;
    private const double MaxMinWidth = 110;
    private const double MaxPreferredWidth = 200;
    private const double HeaderShare = 0.6;
    private const double ComfortShare = 0.8;
    private const double Percentile = 0.95;

    /// <summary>Pages in order of preference, from A4 portrait to A2 landscape, each with its body font size.</summary>
    private static readonly (XSize Page, double FontSize)[] Candidates =
    [
        (new XSize(595, 842), 8),
        (new XSize(842, 595), 8),
        (new XSize(1191, 842), 7),
        (new XSize(1684, 1191), 7)
    ];

    /// <summary>The rows to measure: all of them up to 5,000, else every k-th row.</summary>
    public static IEnumerable<int> SampleIndexes(int rowCount)
    {
        var step = Math.Max(1, (rowCount + SampleSize - 1) / SampleSize);

        for (var i = 0; i < rowCount; i += step)
        {
            yield return i;
        }
    }

    /// <summary>
    ///     Each column's widths at 1 pt, before the page and the font size are known. The header is measured bold and
    ///     counts like any cell for the widest token. Empty cells count as zero width, so a column filled in only a few
    ///     rows does not take the width of its rare values.
    /// </summary>
    public static ColumnMeasure[] Measure(IReadOnlyList<PdfColumn> columns,
        IEnumerable<string[]> sample,
        TextMetrics regular,
        TextMetrics bold)
    {
        var tokens = new double[columns.Count];
        var cellWidthsByColumn = new List<double>[columns.Count];

        for (var c = 0; c < columns.Count; c++)
        {
            tokens[c] = Widest(columns[c].Header, bold).Token;
            cellWidthsByColumn[c] = [];
        }

        foreach (var row in sample)
        {
            for (var c = 0; c < columns.Count; c++)
            {
                var (token, line) = Widest(row[c], regular);
                tokens[c] = Math.Max(tokens[c], token);
                cellWidthsByColumn[c].Add(line);
            }
        }

        var measures = new ColumnMeasure[columns.Count];

        for (var c = 0; c < columns.Count; c++)
        {
            var header = Widest(columns[c].Header, bold).Line;
            var content = Math.Max(Percentile95(cellWidthsByColumn[c]), HeaderShare * header);
            var hint = columns[c].WidthHint * regular.Advance('0');

            measures[c] = new ColumnMeasure(tokens[c], content, hint);
        }

        return measures;
    }

    /// <summary>
    ///     The first page that holds the larger of the minimum widths and 80% of the preferred widths. When none does,
    ///     A2 landscape: the table wraps least there, and a wider table never lands on a smaller page.
    /// </summary>
    public static PdfTablePlan Plan(IReadOnlyList<ColumnMeasure> measures)
    {
        var index = Array.FindIndex(Candidates, candidate => Holds(candidate, measures));

        if (index < 0)
        {
            index = Candidates.Length - 1;
        }

        var (page, fontSize) = Candidates[index];
        var (min, preferred) = Resolve(measures, fontSize);

        return new PdfTablePlan(page, fontSize, Distribute(min, preferred, UsableWidth(page)));
    }

    public static double UsableWidth(XSize page)
    {
        return page.Width - 2 * Margin;
    }

    private static bool Holds((XSize Page, double FontSize) candidate, IReadOnlyList<ColumnMeasure> measures)
    {
        var (min, preferred) = Resolve(measures, candidate.FontSize);

        return UsableWidth(candidate.Page) >= Math.Max(min.Sum(), ComfortShare * preferred.Sum());
    }

    /// <summary>Minimum and preferred widths in points at a font size, padding included.</summary>
    private static (double[] Min, double[] Preferred) Resolve(IReadOnlyList<ColumnMeasure> measures, double fontSize)
    {
        var min = new double[measures.Count];
        var preferred = new double[measures.Count];

        for (var c = 0; c < measures.Count; c++)
        {
            var measure = measures[c];
            min[c] = Math.Min(measure.Token * fontSize, MaxMinWidth) + 2 * CellPadding;

            var wanted = measure.Hint is { } hint
                ? hint * fontSize
                : Math.Min(measure.Content * fontSize, MaxPreferredWidth);

            preferred[c] = Math.Max(min[c], wanted + 2 * CellPadding);
        }

        return (min, preferred);
    }

    /// <summary>
    ///     Fills the usable width: preferred widths scaled up when they all fit, else the minimums plus a share of the
    ///     rest in proportion to how much each column would still grow. Only when even the minimums overflow is their
    ///     content shrunk, padding kept, and long tokens then break by character.
    /// </summary>
    private static double[] Distribute(double[] min, double[] preferred, double usable)
    {
        if (min.Length == 0)
        {
            return [];
        }

        var sumMin = min.Sum();
        var sumPreferred = preferred.Sum();

        if (sumPreferred <= usable)
        {
            return preferred.Select(width => width * usable / sumPreferred).ToArray();
        }

        if (sumMin <= usable)
        {
            var share = (usable - sumMin) / (sumPreferred - sumMin);

            return min.Select((width, c) => width + (preferred[c] - width) * share).ToArray();
        }

        var padding = 2 * CellPadding;
        var minContent = sumMin - padding * min.Length;
        var scale = minContent > 0 ? Math.Max(0, usable - padding * min.Length) / minContent : 0;

        return min.Select(width => padding + (width - padding) * scale).ToArray();
    }

    /// <summary>The widest space-separated token and the widest line of a text, at 1 pt.</summary>
    private static (double Token, double Line) Widest(string text, TextMetrics metrics)
    {
        var token = 0d;
        var line = 0d;
        var widestToken = 0d;
        var widestLine = 0d;

        foreach (var c in text)
        {
            if (c == '\n')
            {
                widestToken = Math.Max(widestToken, token);
                widestLine = Math.Max(widestLine, line);
                token = 0;
                line = 0;
                continue;
            }

            var width = metrics.Advance(c);
            line += width;

            if (c == ' ')
            {
                widestToken = Math.Max(widestToken, token);
                token = 0;
            }
            else
            {
                token += width;
            }
        }

        return (Math.Max(widestToken, token), Math.Max(widestLine, line));
    }

    private static double Percentile95(List<double> widths)
    {
        if (widths.Count == 0)
        {
            return 0;
        }

        widths.Sort();

        return widths[(int)Math.Ceiling(widths.Count * Percentile) - 1];
    }

    /// <summary>
    ///     A column's widths at 1 pt: its widest unbreakable token, the width its content would like (the
    ///     95th-percentile cell, or most of the header), and the rule's <c>HasWidth</c> hint as that many zeros.
    /// </summary>
    internal sealed record ColumnMeasure(double Token, double Content, double? Hint);
}
