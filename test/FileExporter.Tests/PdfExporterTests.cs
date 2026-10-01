using System.Text;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exceptions;
using FileExporter.Exporters;
using FileExporter.Pdf;
using FileExporter.Rules;
using UglyToad.PdfPig;

namespace FileExporter.Tests;

public class PdfExporterTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string OrderHeaders = "ClientIdAmountStageCreatedAtNote";

    [Fact]
    public async Task Writes_a_pdf_named_after_the_rule()
    {
        var file = await PdfExporter.ExportAsync(Order.Sample(3), new OrderExportRule(), null, Ct);

        Assert.Equal("%PDF-", Encoding.ASCII.GetString(file.Content, 0, 5));
        Assert.Same(MimeTypes.Pdf, file.MimeType);
        Assert.Equal("application/pdf", file.MimeType.Value);
        Assert.StartsWith("Orders ", file.Name, StringComparison.Ordinal);
        Assert.EndsWith(".pdf", file.Name, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Headers_and_their_order_match_the_csv_header_line()
    {
        var rule = new OrderExportRule();
        var data = Order.Sample(5);
        var options = new ExportOptions
        {
            ColumnHeaders = new Dictionary<string, string>
            {
                [nameof(Order.Amount)] = "Գումար",
                [nameof(Order.Stage)] = "Статус"
            }
        };

        var csv = CsvExporter.Export(data, rule, options);
        var headers = Encoding.UTF8.GetString(csv.Content)
            .TrimStart((char)0xFEFF)
            .Split('\n')[0]
            .TrimEnd('\r')
            .Split(',');

        var pdf = await PdfExporter.ExportAsync(data, rule, options, Ct);

        Assert.Equal(["Client", "Id", "Գումար", "Статус", "Created At", "Note"], headers);
        Assert.StartsWith("Orders" + string.Concat(headers.Select(PdfText.Strip)), PdfText.Pages(pdf.Content)[0]);
    }

    [Fact]
    public async Task Empty_data_gives_one_page_with_the_title_and_headers()
    {
        var file = await PdfExporter.ExportAsync([], new OrderExportRule(), null, Ct);

        var page = Assert.Single(PdfText.Pages(file.Content));

        Assert.Equal("Orders" + OrderHeaders + "1/1", page);
    }

    [Fact]
    public async Task Every_page_repeats_the_title_and_headers_and_is_numbered()
    {
        var file = await PdfExporter.ExportAsync(Order.Sample(500), new OrderExportRule(), null, Ct);

        var pages = PdfText.Pages(file.Content);

        Assert.True(pages.Count > 3, $"{pages.Count} pages");

        for (var i = 0; i < pages.Count; i++)
        {
            Assert.StartsWith("Orders" + OrderHeaders, pages[i]);
            Assert.EndsWith($"{i + 1}/{pages.Count}", pages[i]);
        }

        Assert.Contains("Customer500", pages[^1]);
    }

    [Fact]
    public async Task Values_are_formatted_as_in_xlsx()
    {
        var file = await PdfExporter.ExportAsync(Order.Sample(4), new OrderExportRule(), null, Ct);

        var page = PdfText.Pages(file.Content)[0];

        // Client, Id, Amount (currency), Stage (enum), CreatedAt, Note (null, so its default value).
        Assert.Contains("Customer111,000.501-Draft2026-10-0112:01:00Note1", page);
        Assert.Contains("Customer444,002.002-Active2026-10-0112:04:00None", page);
    }

    [Fact]
    public async Task Armenian_and_cyrillic_text_round_trips()
    {
        List<Line> rows =
        [
            new() { Text = "Բարեւ աշխարհ" },
            new() { Text = "Привет, мир" },
            new() { Text = "Ողջույն ՠ ֈ ֍ ֎ ֏" }
        ];

        var file = await PdfExporter.ExportAsync(rows, new LineExportRule(), null, Ct);

        var page = Assert.Single(PdfText.Pages(file.Content));

        Assert.Equal("LinesTextԲարեւաշխարհПривет,мирՈղջույնՠֈ֍֎֏1/1", page);
    }

    [Fact]
    public async Task The_dram_sign_is_drawn_in_the_fallback_font()
    {
        List<Line> rows = [new() { Text = "12 500 ֏ Ք" }];

        var file = await PdfExporter.ExportAsync(rows, new LineExportRule(), null, Ct);

        using var document = PdfDocument.Open(file.Content);
        var letters = document.GetPage(1).Letters;

        var dram = Assert.Single(letters, letter => letter.Value == "֏");
        var keh = Assert.Single(letters, letter => letter.Value == "Ք");

        Assert.Contains("Armenian", dram.FontName, StringComparison.Ordinal);
        Assert.Contains("Condensed", keh.FontName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fallback_runs_are_placed_one_after_another()
    {
        List<Line> rows = [new() { Text = "12 500 ֏ and ՠ end" }];

        var file = await PdfExporter.ExportAsync(rows, new LineExportRule(), null, Ct);

        using var document = PdfDocument.Open(file.Content);
        var line = document.GetPage(1).Letters
            .Where(letter => letter.Value != " ")
            .GroupBy(letter => Math.Round(letter.StartBaseLine.Y, 1))
            .Select(group => group.ToList())
            .Single(group => group.Any(letter => letter.Value == "֏"));

        for (var i = 1; i < line.Count; i++)
        {
            Assert.True(line[i].StartBaseLine.X >= line[i - 1].EndBaseLine.X - 0.05,
                $"'{line[i].Value}' at {line[i].StartBaseLine.X} overlaps '{line[i - 1].Value}' ending at {line[i - 1].EndBaseLine.X}");
        }
    }

    [Fact]
    public async Task A_missing_glyph_never_fails_the_export()
    {
        List<Line> rows = [new() { Text = "Lari ₾, CJK 中文, emoji 😀, math 𝐀" }];

        var file = await PdfExporter.ExportAsync(rows, new LineExportRule(), null, Ct);

        Assert.Single(PdfText.Pages(file.Content));
    }

    [Fact]
    public async Task Three_columns_fit_a4_portrait()
    {
        var file = await PdfExporter.ExportAsync([new Contact { Id = 1, Name = "Anna", Phone = "+374 91 000000" }],
            new ConventionOnlyExportRule<Contact>(),
            null,
            Ct);

        AssertPageSize(file, 595, 842);
    }

    [Fact]
    public async Task Twenty_seven_columns_take_a2_landscape()
    {
        var file = await PdfExporter.ExportAsync(Wide.Sample(50), new WideExportRule(), null, Ct);

        AssertPageSize(file, 1684, 1191);
    }

    [Fact]
    public async Task Nothing_is_drawn_outside_the_margins()
    {
        var wide = await PdfExporter.ExportAsync(Wide.Sample(200), new WideExportRule(), null, Ct);

        // Glyphs neither font has (CJK, the lari, a check mark) are drawn 1 em wide by viewers.
        List<Line> lines =
        [
            new() { Text = new string('W', 2_000) },
            new() { Text = new string('中', 300) },
            new() { Text = string.Concat(Enumerable.Repeat("₾✅ ", 200)) },
            new() { Text = string.Join(' ', Enumerable.Repeat("word", 30_000)) }
        ];

        var text = await PdfExporter.ExportAsync(lines, new LineExportRule(), null, Ct);

        foreach (var file in new[] { wide, text })
        {
            using var document = PdfDocument.Open(file.Content);

            foreach (var page in document.GetPages())
            {
                // Everything but the page number sits inside the 24 pt margins; the number's baseline is 12 pt up.
                Assert.All(page.Letters.Where(letter => Math.Abs(letter.StartBaseLine.Y - 12) > 0.5),
                    letter =>
                    {
                        Assert.InRange(letter.StartBaseLine.X, 24 - 0.05, page.Width - 24 + 0.05);
                        Assert.InRange(letter.EndBaseLine.X, 24 - 0.05, page.Width - 24 + 0.05);
                        Assert.InRange(letter.StartBaseLine.Y, 24, page.Height - 24);
                    });
            }
        }

        // The 30,000-word cell is taller than a page, so it is cut and ends in an ellipsis.
        Assert.Contains("…", string.Concat(PdfText.Pages(text.Content)));
    }

    [Fact]
    public async Task Numbers_and_their_header_align_right()
    {
        List<Amount> rows = [new() { Value = 1m }, new() { Value = 1_000m }, new() { Value = 1_000_000m }];

        var file = await PdfExporter.ExportAsync(rows, new ConventionOnlyExportRule<Amount>(), null, Ct);

        using var document = PdfDocument.Open(file.Content);
        var page = document.GetPage(1);

        var rightEdges = page.Letters
            .GroupBy(letter => Math.Round(letter.StartBaseLine.Y, 1))
            .Select(line => line.OrderBy(letter => letter.StartBaseLine.X).ToList())
            .Where(line => line[0].StartBaseLine.X > page.Width / 2)
            .Select(line => line[^1].EndBaseLine.X)
            .ToList();

        Assert.Equal(4, rightEdges.Count);
        Assert.All(rightEdges, edge => Assert.InRange(edge, rightEdges[0] - 0.05, rightEdges[0] + 0.05));
    }

    [Fact]
    public async Task SheetName_titles_the_document_and_every_page()
    {
        var options = new ExportOptions { SheetName = "Պատվերներ 2026" };

        var file = await PdfExporter.ExportAsync(Order.Sample(500), new OrderExportRule(), options, Ct);

        using var document = PdfDocument.Open(file.Content);

        Assert.Equal("Պատվերներ 2026", document.Information.Title);
        Assert.Equal("PandaTech.FileExporter", document.Information.Creator);
        Assert.True(document.NumberOfPages > 1);
        Assert.All(document.GetPages(),
            page => Assert.StartsWith("Պատվերներ2026" + OrderHeaders, PdfText.Strip(page.Text)));
    }

    [Fact]
    public async Task Without_a_sheet_name_the_rule_name_titles_the_document()
    {
        var file = await PdfExporter.ExportAsync(Order.Sample(2), new OrderExportRule(), null, Ct);

        using var document = PdfDocument.Open(file.Content);

        Assert.Equal("Orders", document.Information.Title);
    }

    [Fact]
    public async Task More_than_100000_rows_are_rejected_after_reading_one_past_the_limit()
    {
        var read = 0;

        IEnumerable<Line> Source()
        {
            for (var i = 0; i < 150_000; i++)
            {
                read++;
                yield return new Line { Text = "x" };
            }
        }

        var exception = await Assert.ThrowsAsync<ExportRowLimitExceededException>(() =>
            PdfExporter.ExportAsync(Source(), new LineExportRule(), null, Ct));

        Assert.Equal(100_001, read);
        Assert.Equal(ExportFormat.Pdf, exception.Format);
        Assert.Equal(100_000, exception.Limit);
        Assert.Equal("Pdf exports are limited to 100000 rows.", exception.Message);
    }

    [Fact]
    public async Task A_list_over_the_limit_is_rejected_without_formatting_a_row()
    {
        var formatted = 0;
        var rows = Enumerable.Range(0, 100_001).Select(_ => new Order { Stage = Stage.Active }).ToList();
        var options = new ExportOptions
        {
            EnumLabelResolver = value =>
            {
                formatted++;
                return value.ToString();
            }
        };

        await Assert.ThrowsAsync<ExportRowLimitExceededException>(() =>
            PdfExporter.ExportAsync(rows, new OrderExportRule(), options, Ct));

        Assert.Equal(0, formatted);
    }

    [Fact]
    public async Task Exactly_100000_rows_are_exported()
    {
        var rows = Enumerable.Range(1, 100_000).Select(i => new Line { Text = $"Row {i}" }).ToList();

        var file = await PdfExporter.ExportAsync(rows, new LineExportRule(), null, Ct);

        Assert.Same(MimeTypes.Pdf, file.MimeType);

        using var document = PdfDocument.Open(file.Content);
        var pageCount = document.NumberOfPages;
        var last = PdfText.Strip(document.GetPage(pageCount).Text);

        Assert.EndsWith($"Row100000{pageCount}/{pageCount}", last);
    }

    [Fact]
    public async Task A_cancelled_token_stops_the_export()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            PdfExporter.ExportAsync(Order.Sample(10), new OrderExportRule(), null, cts.Token));
    }

    [Fact]
    public async Task Cancelling_while_rendering_stops_at_the_next_page()
    {
        using var cts = new CancellationTokenSource();
        var formatted = 0;
        var options = new ExportOptions
        {
            // Called once per Stage cell: 5,000 times while measuring, then again for each row drawn.
            EnumLabelResolver = value =>
            {
                if (++formatted == 6_000)
                {
                    cts.Cancel();
                }

                return value.ToString();
            }
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            PdfExporter.ExportAsync(Order.Sample(5_000), new OrderExportRule(), options, cts.Token));

        Assert.InRange(formatted, 6_000, 6_200);
    }

    [Fact]
    public async Task Eight_parallel_exports_succeed_and_agree()
    {
        var rule = new WideExportRule();
        var data = Wide.Sample(1_000);
        using var start = new Barrier(8);

        // Each export starts on its own thread at the same moment; the render gate decides how many overlap.
        var files = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => Task.Factory.StartNew(async () =>
                    {
                        start.SignalAndWait(Ct);
                        return await PdfExporter.ExportAsync(data, rule, null, Ct);
                    },
                    Ct,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)
                .Unwrap()));

        var pages = files.Select(file => PdfText.Pages(file.Content)).ToList();

        Assert.All(pages, document => Assert.Equal(pages[0], document));
    }

    [Fact]
    public async Task Eight_renders_at_once_share_no_state()
    {
        // The render gate admits ProcessorCount / 2 exports, one or two on a small CI runner, so render directly to
        // make eight renders overlap whatever the machine.
        PdfColumn[] columns = [new("Text", false, null), new("Amount", true, null)];
        var rows = Enumerable.Range(1, 2_000).Select(i => new[] { $"Row {i} Բարեւ ֏ Привет", $"{i},000.00" }).ToList();
        using var start = new Barrier(8);

        var renders = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => Task.Factory.StartNew(() =>
                {
                    using var renderer = new PdfTableRenderer("Parallel", columns);
                    var plan = PdfLayout.Plan(PdfLayout.Measure(columns, rows, renderer.Regular, renderer.Bold));

                    start.SignalAndWait(Ct);

                    return renderer.Render(plan, rows, Ct);
                },
                Ct,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default)));

        var pages = renders.Select(PdfText.Pages).ToList();

        Assert.All(pages, document => Assert.Equal(pages[0], document));
    }

    private static void AssertPageSize(ExportFile file, double width, double height)
    {
        using var document = PdfDocument.Open(file.Content);

        Assert.All(document.GetPages(), page =>
        {
            Assert.Equal(width, page.Width, 0);
            Assert.Equal(height, page.Height, 0);
        });
    }
}
