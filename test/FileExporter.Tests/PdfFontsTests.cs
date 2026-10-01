using FileExporter.Exporters;
using FileExporter.Pdf;
using PdfSharp.Fonts;

namespace FileExporter.Tests;

public class PdfFontsTests
{
    private static readonly char[] Fallback = ['ՠ', 'ֈ', '֍', '֎', '֏'];

    [Theory]
    [InlineData("DejaVuSansCondensed.ttf")]
    [InlineData("DejaVuSansCondensed-Bold.ttf")]
    [InlineData("NotoSansArmenian-Regular.ttf")]
    [InlineData("NotoSansArmenian-Bold.ttf")]
    public void The_fonts_are_embedded_under_their_logical_names(string fileName)
    {
        using var stream = typeof(PdfFonts).Assembly.GetManifestResourceStream(PdfFonts.ResourcePrefix + fileName);

        Assert.NotNull(stream);

        var header = new byte[4];
        stream.ReadExactly(header);

        Assert.Equal(new byte[] { 0x00, 0x01, 0x00, 0x00 }, header);
    }

    [Fact]
    public void No_font_file_ships_outside_the_assembly()
    {
        var directory = Path.GetDirectoryName(typeof(PdfFonts).Assembly.Location)!;

        Assert.Empty(Directory.EnumerateFiles(directory, "*.ttf", SearchOption.AllDirectories));
    }

    [Fact]
    public void Exactly_the_five_characters_dejavu_lacks_fall_back()
    {
        for (var c = '԰'; c <= '֏'; c++)
        {
            Assert.Equal(Fallback.Contains(c), PdfFonts.IsFallback(c));
        }

        Assert.False(PdfFonts.IsFallback('A'));
        Assert.False(PdfFonts.IsFallback('Ж'));
        Assert.False(PdfFonts.IsFallback('€'));
    }

    [Fact]
    public void Runs_route_the_fallback_characters_to_the_fallback_font()
    {
        var runs = PdfFonts.SplitRuns("Total 12 500 ֏, ՠֈ end").ToList();

        Assert.Equal(
        [
            ("Total 12 500 ", false),
            ("֏", true),
            (", ", false),
            ("ՠֈ", true),
            (" end", false)
        ], runs);
    }

    [Fact]
    public void Plain_text_is_one_run()
    {
        const string text = "Hello Բարեւ Привет";

        Assert.Equal([(text, false)], PdfFonts.SplitRuns(text).ToList());
        Assert.False(PdfFonts.NeedsFallback(text));
        Assert.True(PdfFonts.NeedsFallback("12 500 ֏"));
    }

    [Fact]
    public void The_embedded_fonts_carry_private_names()
    {
        // PDFsharp caches fonts process-wide by name: the public DejaVu and Noto names would clash with a host's own
        // build of either font and break its PDFs or ours.
        Assert.Equal("FileExporter Sans Condensed", PdfFonts.Create(8, false).FontFamily.Name);
        Assert.Equal("FileExporter Sans Condensed", PdfFonts.Create(8, true).FontFamily.Name);
        Assert.True(PdfFonts.Create(8, true).Bold);
        Assert.Equal("FileExporter Sans Armenian", PdfFonts.CreateFallback(8, false).FontFamily.Name);
        Assert.Equal("FileExporter Sans Armenian", PdfFonts.CreateFallback(8, true).FontFamily.Name);
    }

    [Fact]
    public async Task An_export_never_sets_the_global_font_resolver()
    {
        await PdfExporter.ExportAsync(Order.Sample(3),
            new OrderExportRule(),
            null,
            TestContext.Current.CancellationToken);

        // No test assigns a resolver, so this holds whatever ran before.
        Assert.Null(GlobalFontSettings.FontResolver);
    }
}
