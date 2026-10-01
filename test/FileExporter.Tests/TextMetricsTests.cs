using FileExporter.Pdf;

namespace FileExporter.Tests;

public class TextMetricsTests
{
    // Every character is one unit wide, so widths are lengths.
    private readonly TextMetrics _metrics = new(_ => 1);

    [Fact]
    public void Width_sums_advances()
    {
        Assert.Equal(3, _metrics.Width("abc"));
        Assert.Equal(0, _metrics.Width(""));
        Assert.Equal(7, new TextMetrics(c => c == 'w' ? 2.5 : 1).Width("wwab"));
    }

    [Fact]
    public void Advances_are_measured_once_per_character()
    {
        var calls = 0;
        var metrics = new TextMetrics(_ =>
        {
            calls++;
            return 1;
        });

        metrics.Width("aaaa bbbb");
        metrics.Width("abba");

        Assert.Equal(3, calls);
    }

    [Fact]
    public void Text_that_fits_stays_one_line_and_is_not_copied()
    {
        const string text = "abc def";

        var lines = Wrap(text, 10);

        Assert.Equal([text], lines);
        Assert.Same(text, lines[0]);
    }

    [Fact]
    public void Wraps_on_spaces()
    {
        Assert.Equal(["aaa bbb", "ccc"], Wrap("aaa bbb ccc", 7));
        Assert.Equal(["aaa", "bbb", "ccc"], Wrap("aaa bbb ccc", 5));
    }

    [Fact]
    public void Spaces_at_a_break_are_dropped()
    {
        Assert.Equal(["aaa", "bbb"], Wrap("aaa    bbb", 5));
        Assert.Equal(["aaaa", "bbbb"], Wrap("aaaa bbbb", 4));
    }

    [Fact]
    public void A_wrapped_line_never_starts_with_spaces()
    {
        Assert.Equal(["abcde"], Wrap(" abcde", 5));
        Assert.Equal(["abcd"], Wrap("  abcd", 5));
        Assert.Equal(["x", "abcd"], Wrap("x\n  abcd", 5));
    }

    [Fact]
    public void A_long_token_breaks_by_character()
    {
        Assert.Equal(["abcd", "efgh", "ij"], Wrap("abcdefghij", 4));
        Assert.Equal(["a", "bcde", "fgh"], Wrap("a bcdefgh", 4));
    }

    [Fact]
    public void A_character_wider_than_the_line_still_takes_a_line()
    {
        Assert.Equal(["a", "b"], Wrap("ab", 0.5));
    }

    [Fact]
    public void Empty_text_gives_one_empty_line()
    {
        Assert.Equal([""], Wrap("", 5));
    }

    [Fact]
    public void Line_breaks_are_hard_breaks()
    {
        Assert.Equal(["one", "two", "", "three"], Wrap("one\ntwo\n\nthree", 50));
        Assert.Equal(["one"], Wrap("one\n", 50));
    }

    [Fact]
    public void Text_past_the_line_limit_ends_in_an_ellipsis()
    {
        Assert.Equal(["aaa bbb", "ccc dd…"], Wrap("aaa bbb ccc ddd eee", 7, 2));
        Assert.Equal(["abc…"], Wrap("abcdefgh", 4, 1));
        Assert.Equal(["one…"], Wrap("one\ntwo", 50, 1));
    }

    [Fact]
    public void Text_that_exactly_fills_the_line_limit_is_not_cut()
    {
        Assert.Equal(["aaa bbb", "ccc ddd"], Wrap("aaa bbb ccc ddd", 7, 2));
    }

    [Fact]
    public void A_surrogate_pair_is_never_split()
    {
        var lines = Wrap("ab\U0001F600cd", 3);

        Assert.Equal(["ab", "\U0001F600c", "d"], lines);
        Assert.All(lines, line => Assert.False(char.IsHighSurrogate(line[^1]) || char.IsLowSurrogate(line[0])));
    }

    [Fact]
    public void Normalize_unifies_line_breaks_and_blanks_other_control_characters()
    {
        Assert.Equal("a\nb\nc d e", TextMetrics.Normalize("a\r\nb\rc\td\u0001e"));
        Assert.Equal("x\n\ny", TextMetrics.Normalize("x\r\n\r\ny"));
        // NEL, the Unicode line and paragraph separators, a C1 control and DEL.
        var text = string.Concat("a", (char)0x85, "b", (char)0x2028, "c", (char)0x2029, "d", (char)0x93, "e",
            (char)0x7F, "f");

        Assert.Equal("a\nb\nc\nd e f", TextMetrics.Normalize(text));
    }

    [Fact]
    public void Normalize_returns_clean_text_as_it_is()
    {
        const string text = "Բարեւ, Привет, hello";

        Assert.Same(text, TextMetrics.Normalize(text));
    }

    private List<string> Wrap(string text, double width, int maxLines = int.MaxValue)
    {
        List<string> lines = [];
        _metrics.Wrap(text, width, maxLines, lines);

        return lines;
    }
}
