using System.Buffers;

namespace FileExporter.Pdf;

/// <summary>
///     Text width and line wrapping for one font, from per-character advances at 1 pt. The embedded fonts are drawn
///     without kerning, so the width of a run is exactly the sum of its advances and scales linearly with the size.
///     Widths taken and returned here are all at 1 pt.
/// </summary>
internal sealed class TextMetrics(Func<char, double> advance)
{
    private const char Ellipsis = (char)0x2026;
    private const char NextLineControl = (char)0x0085;
    private const char LineSeparator = (char)0x2028;
    private const char ParagraphSeparator = (char)0x2029;

    private static readonly SearchValues<char> ControlChars = SearchValues.Create(Enumerable.Range(0, 0xA0)
        .Where(i => char.IsControl((char)i))
        .Select(i => (char)i)
        .Concat([LineSeparator, ParagraphSeparator])
        .ToArray());

    private readonly Dictionary<char, double> _advanceByChar = new();

    /// <summary>
    ///     Makes text drawable glyph by glyph: line breaks (CR, LF, NEL and the Unicode line and paragraph separators)
    ///     become <c>\n</c>, which <see cref="Wrap" /> honours, and other control characters, which would print as empty
    ///     boxes, become spaces.
    /// </summary>
    public static string Normalize(string text)
    {
        if (text.AsSpan().IndexOfAny(ControlChars) < 0)
        {
            return text;
        }

        return string.Create(text.Length - CountCrLf(text),
            text,
            static (span, source) =>
            {
                var j = 0;

                for (var i = 0; i < source.Length; i++)
                {
                    var c = source[i];

                    if (c == '\r' && i + 1 < source.Length && source[i + 1] == '\n')
                    {
                        continue;
                    }

                    span[j++] = c switch
                    {
                        '\n' or '\r' or NextLineControl or LineSeparator or ParagraphSeparator => '\n',
                        _ when char.IsControl(c) => ' ',
                        _ => c
                    };
                }
            });
    }

    public double Advance(char c)
    {
        if (!_advanceByChar.TryGetValue(c, out var width))
        {
            width = advance(c);
            _advanceByChar[c] = width;
        }

        return width;
    }

    public double Width(ReadOnlySpan<char> text)
    {
        var width = 0d;

        foreach (var c in text)
        {
            width += Advance(c);
        }

        return width;
    }

    /// <summary>
    ///     Appends the lines <paramref name="text" /> takes at <paramref name="maxWidth" />: greedy on spaces, with a hard
    ///     break at every <c>\n</c>, and inside a token only when the token alone is wider than the line. Text past
    ///     <paramref name="maxLines" /> is cut, and the last line kept ends in an ellipsis. A wrapped line never starts
    ///     with spaces. Only the final line strings are allocated, and a text that fits is added as it is.
    /// </summary>
    public void Wrap(string text, double maxWidth, int maxLines, List<string> lines)
    {
        if (text.Length == 0 || (!text.Contains('\n') && Width(text) <= maxWidth))
        {
            lines.Add(text);
            return;
        }

        var start = 0;
        var count = 0;

        while (true)
        {
            start = SkipSpaces(text, start);

            var (end, next) = NextLine(text, start, maxWidth);

            if (next >= text.Length)
            {
                lines.Add(text[start..end]);
                return;
            }

            if (++count >= maxLines)
            {
                lines.Add(CutWithEllipsis(text, start, maxWidth));
                return;
            }

            lines.Add(text[start..end]);
            start = next;
        }
    }

    private static int SkipSpaces(string text, int start)
    {
        while (start < text.Length && text[start] == ' ')
        {
            start++;
        }

        return start;
    }

    private static int CountCrLf(string text)
    {
        var count = 0;
        var index = text.IndexOf("\r\n", StringComparison.Ordinal);

        while (index >= 0)
        {
            count++;
            index = text.IndexOf("\r\n", index + 2, StringComparison.Ordinal);
        }

        return count;
    }

    /// <summary>
    ///     The greedy line starting at <paramref name="start" />: where it ends (trailing spaces dropped) and where the
    ///     next one starts. Spaces may hang past the width; any other character that does not fit ends the line at the
    ///     last space, or right before it when the line has no space to break at.
    /// </summary>
    private (int End, int Next) NextLine(string text, int start, double maxWidth)
    {
        var width = 0d;
        var lastSpace = -1;

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '\n')
            {
                return (TrimEnd(text, start, i), i + 1);
            }

            var charWidth = Advance(c);

            if (c == ' ')
            {
                lastSpace = i;
                width += charWidth;
                continue;
            }

            if (width + charWidth > maxWidth && i > start)
            {
                if (lastSpace > start)
                {
                    return (TrimEnd(text, start, lastSpace), lastSpace + 1);
                }

                var end = BreakInsideToken(text, start, i);

                return (end, end);
            }

            width += charWidth;
        }

        return (TrimEnd(text, start, text.Length), text.Length);
    }

    /// <summary>A break at <paramref name="index" /> that never splits a surrogate pair and always makes progress.</summary>
    private static int BreakInsideToken(string text, int start, int index)
    {
        if (!char.IsLowSurrogate(text[index]))
        {
            return index;
        }

        return index - 1 > start ? index - 1 : index + 1;
    }

    private static int TrimEnd(string text, int start, int end)
    {
        while (end > start && text[end - 1] == ' ')
        {
            end--;
        }

        return end;
    }

    private string CutWithEllipsis(string text, int start, double maxWidth)
    {
        var available = maxWidth - Advance(Ellipsis);
        var width = 0d;
        var end = start;

        while (end < text.Length && text[end] != '\n')
        {
            var charWidth = Advance(text[end]);

            if (width + charWidth > available)
            {
                break;
            }

            width += charWidth;
            end++;
        }

        if (end > start && char.IsHighSurrogate(text[end - 1]))
        {
            end--;
        }

        return string.Concat(text.AsSpan(start, TrimEnd(text, start, end) - start), Ellipsis.ToString());
    }
}
