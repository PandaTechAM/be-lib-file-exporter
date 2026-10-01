using FileExporter.Pdf;
using static FileExporter.Pdf.PdfLayout;

namespace FileExporter.Tests;

public class PdfLayoutTests
{
    private const double A4PortraitWidth = 595;
    private const double A4LandscapeWidth = 842;
    private const double A2LandscapeWidth = 1684;

    [Fact]
    public void A_narrow_table_takes_a4_portrait()
    {
        var plan = Plan(Columns(3, 3, 8));

        AssertPage(plan, 595, 842, 8);
        AssertFillsAndKeepsMinimums(plan, Columns(3, 3, 8));
    }

    [Fact]
    public void A_medium_table_takes_a4_landscape()
    {
        var measures = Columns(10, 5, 9);
        var plan = Plan(measures);

        AssertPage(plan, 842, 595, 8);
        AssertFillsAndKeepsMinimums(plan, measures);
    }

    [Fact]
    public void A_wider_table_takes_a3_landscape()
    {
        var measures = Columns(14, 6, 10);
        var plan = Plan(measures);

        AssertPage(plan, 1191, 842, 7);
        AssertFillsAndKeepsMinimums(plan, measures);
    }

    [Fact]
    public void A_payment_register_takes_a2_landscape()
    {
        var measures = Columns(27, 6, 10);
        var plan = Plan(measures);

        AssertPage(plan, 1684, 1191, 7);
        AssertFillsAndKeepsMinimums(plan, measures);
    }

    [Fact]
    public void A_table_wider_than_a2_shrinks_its_minimums_onto_a2()
    {
        var plan = Plan(Columns(60, 10, 10));

        AssertPage(plan, 1684, 1191, 7);
        Assert.Equal(UsableWidth(new(A2LandscapeWidth, 1191)), plan.Widths.Sum(), 6);
        Assert.All(plan.Widths, width => Assert.Equal(plan.Widths[0], width, 6));
        Assert.True(plan.Widths[0] < Math.Min(10 * 7, 110) + 2 * CellPadding);
    }

    [Fact]
    public void Without_a_comfortable_page_the_table_takes_a2_and_never_a_smaller_page_than_a_narrower_table()
    {
        // Short tokens but long content: no page holds 80% of what 10 such columns would like; 9 fit A2 comfortably.
        var ten = Plan(Columns(10, 3, 1000));
        var nine = Plan(Columns(9, 3, 1000));

        AssertPage(ten, 1684, 1191, 7);
        AssertPage(nine, 1684, 1191, 7);
        AssertFillsAndKeepsMinimums(ten, Columns(10, 3, 1000));
    }

    [Fact]
    public void Shrinking_past_the_minimums_keeps_every_column_padding()
    {
        ColumnMeasure[] measures =
        [
            new(0.5, 0.5, null),
            .. Enumerable.Range(0, 99).Select(_ => new ColumnMeasure(15, 15, null))
        ];

        var plan = Plan(measures);

        AssertPage(plan, 1684, 1191, 7);
        Assert.Equal(UsableWidth(plan.Page), plan.Widths.Sum(), 6);
        Assert.All(plan.Widths, width => Assert.True(width > 2 * CellPadding, $"{width}"));
    }

    [Fact]
    public void Preferred_widths_scale_up_to_the_page_when_they_all_fit()
    {
        var plan = Plan([new ColumnMeasure(2, 10, null), new ColumnMeasure(2, 20, null)]);

        Assert.Equal(UsableWidth(new(A4PortraitWidth, 842)), plan.Widths.Sum(), 6);
        Assert.Equal((10 * 8 + 5) / (20 * 8 + 5d), plan.Widths[0] / plan.Widths[1], 6);
    }

    [Fact]
    public void Content_widths_are_capped_at_200_points()
    {
        var plan = Plan([new ColumnMeasure(2, 10, null), new ColumnMeasure(2, 1_000, null)]);

        Assert.Equal((10 * 8 + 5) / (200 + 5d), plan.Widths[0] / plan.Widths[1], 6);
    }

    [Fact]
    public void A_width_hint_replaces_the_preferred_width_and_is_not_capped()
    {
        // 40 zeros at 8 pt is 320 pt, past the 200 pt cap on content widths.
        var hinted = Plan([new ColumnMeasure(2, 10, 40), new ColumnMeasure(2, 10, null)]);

        Assert.Equal((40 * 8 + 5) / (10 * 8 + 5d), hinted.Widths[0] / hinted.Widths[1], 6);
    }

    [Fact]
    public void A_width_hint_never_goes_below_the_widest_token()
    {
        var plan = Plan([new ColumnMeasure(10, 10, 1), new ColumnMeasure(10, 10, null)]);

        Assert.Equal(plan.Widths[0], plan.Widths[1], 6);
    }

    [Fact]
    public void No_columns_gives_an_empty_a4_plan()
    {
        var plan = Plan([]);

        AssertPage(plan, 595, 842, 8);
        Assert.Empty(plan.Widths);
    }

    [Fact]
    public void Measure_takes_the_widest_token_with_the_header_in_bold()
    {
        var regular = new TextMetrics(_ => 1);
        var bold = new TextMetrics(_ => 2);
        PdfColumn[] columns = [new("Name", false, null), new("Id", true, 10)];

        var measures = Measure(columns, [["abc def", "123456"], ["", "7"]], regular, bold);

        Assert.Equal(8, measures[0].Token);
        Assert.Equal(6, measures[1].Token);
        Assert.Null(measures[0].Hint);
        Assert.Equal(10, measures[1].Hint);
    }

    [Fact]
    public void Measure_prefers_the_95th_percentile_cell_or_most_of_the_header()
    {
        var regular = new TextMetrics(_ => 1);
        var cells = Enumerable.Range(1, 100).Select(n => new[] { new string('x', n), "" });
        PdfColumn[] columns = [new("A", false, null), new("A long header", false, null)];

        var measures = Measure(columns, cells, regular, regular);

        Assert.Equal(95, measures[0].Content);
        Assert.Equal(0.6 * 13, measures[1].Content, 6);
    }

    [Fact]
    public void A_column_filled_in_few_rows_is_not_sized_by_its_rare_values()
    {
        var regular = new TextMetrics(_ => 1);
        var cells = Enumerable.Range(0, 100).Select(n => new[] { n < 3 ? new string('x', 60) : "" });

        var measures = Measure([new PdfColumn("Reason", false, null)], cells, regular, regular);

        Assert.Equal(0.6 * 6, measures[0].Content, 6);
        Assert.Equal(60, measures[0].Token);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 3)]
    [InlineData(5_000, 5_000)]
    [InlineData(5_001, 2_501)]
    [InlineData(100_000, 5_000)]
    public void The_sample_is_every_row_up_to_5000_then_every_kth(int rowCount, int expected)
    {
        var indexes = SampleIndexes(rowCount).ToList();

        Assert.Equal(expected, indexes.Count);
        Assert.All(indexes, index => Assert.InRange(index, 0, rowCount - 1));
    }

    private static ColumnMeasure[] Columns(int count, double token, double content)
    {
        return Enumerable.Range(0, count).Select(_ => new ColumnMeasure(token, content, null)).ToArray();
    }

    private static void AssertPage(PdfTablePlan plan, double width, double height, double fontSize)
    {
        Assert.Equal(width, plan.Page.Width);
        Assert.Equal(height, plan.Page.Height);
        Assert.Equal(fontSize, plan.FontSize);
    }

    private static void AssertFillsAndKeepsMinimums(PdfTablePlan plan, ColumnMeasure[] measures)
    {
        Assert.Equal(UsableWidth(plan.Page), plan.Widths.Sum(), 6);

        for (var c = 0; c < measures.Length; c++)
        {
            var min = Math.Min(measures[c].Token * plan.FontSize, 110) + 2 * CellPadding;
            Assert.True(plan.Widths[c] >= min - 1e-9, $"column {c}: {plan.Widths[c]} < {min}");
        }
    }
}
