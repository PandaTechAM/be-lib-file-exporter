using System.Runtime.CompilerServices;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exceptions;
using FileExporter.Extensions;

namespace FileExporter.Tests;

public class ExportExtensionsTests : IClassFixture<RegistryFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ToPdfAsync_and_ToFileFormatAsync_give_the_same_file()
    {
        var options = new ExportOptions { FileName = "Orders export" };

        var viaPdf = await Source(Order.Sample(3)).ToPdfAsync(options, Ct);
        var viaFormat = await Source(Order.Sample(3)).ToFileFormatAsync(ExportFormat.Pdf, options, Ct);
        var viaList = await Order.Sample(3).ToFileFormatAsync(ExportFormat.Pdf, options, Ct);

        foreach (var file in new[] { viaPdf, viaFormat, viaList })
        {
            Assert.Equal("Orders export.pdf", file.Name);
            Assert.Same(MimeTypes.Pdf, file.MimeType);
            Assert.Equal(PdfText.Pages(viaPdf.Content), PdfText.Pages(file.Content));
        }
    }

    [Fact]
    public async Task The_default_name_is_the_rule_name_and_a_timestamp()
    {
        var file = await Source(Order.Sample(1)).ToPdfAsync(Ct);

        Assert.Matches(@"^Orders \d{4}-\d{2}-\d{2} \d{2}_\d{2}_\d{2}\.pdf$", file.Name);
    }

    [Fact]
    public async Task All_three_formats_share_one_base_name()
    {
        var options = new ExportOptions { FileName = "Q1 Orders" };
        var data = Order.Sample(2);

        var csv = await data.ToFileFormatAsync(ExportFormat.Csv, options, Ct);
        var xlsx = await data.ToFileFormatAsync(ExportFormat.Xlsx, options, Ct);
        var pdf = await data.ToFileFormatAsync(ExportFormat.Pdf, options, Ct);

        Assert.Equal("Q1 Orders.csv", csv.Name);
        Assert.Equal("Q1 Orders.xlsx", xlsx.Name);
        Assert.Equal("Q1 Orders.pdf", pdf.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task Undefined_formats_are_rejected(int format)
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            Order.Sample(1).ToFileFormatAsync((ExportFormat)format, Ct));

        Assert.Contains("Csv (1), Xlsx (2) or Pdf (3)", exception.Message, StringComparison.Ordinal);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            Source(Order.Sample(1)).ToFileFormatAsync((ExportFormat)format, Ct));
    }

    [Fact]
    public async Task An_async_source_is_read_only_one_row_past_the_pdf_limit()
    {
        var viaPdf = new StrongBox<int>();
        var viaFormat = new StrongBox<int>();

        await Assert.ThrowsAsync<ExportRowLimitExceededException>(() => Count(150_000, viaPdf).ToPdfAsync(Ct));
        await Assert.ThrowsAsync<ExportRowLimitExceededException>(() =>
            Count(150_000, viaFormat).ToFileFormatAsync(ExportFormat.Pdf, Ct));

        Assert.Equal(100_001, viaPdf.Value);
        Assert.Equal(100_001, viaFormat.Value);
    }

    [Fact]
    public async Task Csv_still_reads_an_async_source_to_the_end()
    {
        var read = new StrongBox<int>();

        var file = await Count(120_000, read).ToFileFormatAsync(ExportFormat.Csv, Ct);

        Assert.Equal(120_000, read.Value);
        Assert.Same(MimeTypes.Csv, file.MimeType);
    }

    private static async IAsyncEnumerable<T> Source<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<Line> Count(int total, StrongBox<int> read)
    {
        for (var i = 0; i < total; i++)
        {
            read.Value++;
            yield return new Line { Text = "x" };
        }

        await Task.CompletedTask;
    }
}
