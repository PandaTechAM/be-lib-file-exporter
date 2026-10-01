using System.Buffers;
using PdfSharp.Drawing;

namespace FileExporter.Pdf;

/// <summary>
///     The fonts embedded in this assembly, loaded from memory. PDFsharp's global <c>FontResolver</c> is deliberately
///     left alone: PDFsharp ignores a second resolver of the same type and throws on any other once a font was loaded,
///     so installing one here would break the application's own resolver, or be broken by it. The embedded files carry
///     private names (FileExporter Sans Condensed, FileExporter Sans Armenian): PDFsharp caches fonts process-wide by
///     name, and the original names would collide with an application's own build of DejaVu or Noto.
/// </summary>
internal static class PdfFonts
{
    public const string ResourcePrefix = "FileExporter.Fonts.";

    /// <summary>
    ///     The Armenian characters DejaVu Sans Condensed has no glyph for, the dram sign U+058F among them. Noto Sans
    ///     Armenian draws these; DejaVu draws everything else.
    /// </summary>
    private static readonly SearchValues<char> FallbackChars =
        SearchValues.Create([(char)0x0560, (char)0x0588, (char)0x058D, (char)0x058E, (char)0x058F]);

    private static readonly Lazy<XGlyphTypeface> Regular = Load("DejaVuSansCondensed.ttf");
    private static readonly Lazy<XGlyphTypeface> Bold = Load("DejaVuSansCondensed-Bold.ttf");
    private static readonly Lazy<XGlyphTypeface> FallbackRegular = Load("NotoSansArmenian-Regular.ttf");
    private static readonly Lazy<XGlyphTypeface> FallbackBold = Load("NotoSansArmenian-Bold.ttf");

    public static XFont Create(double size, bool bold)
    {
        return Create(bold ? Bold : Regular, size);
    }

    public static XFont CreateFallback(double size, bool bold)
    {
        return Create(bold ? FallbackBold : FallbackRegular, size);
    }

    public static bool IsFallback(char c)
    {
        return FallbackChars.Contains(c);
    }

    public static bool NeedsFallback(ReadOnlySpan<char> text)
    {
        return text.IndexOfAny(FallbackChars) >= 0;
    }

    /// <summary>Splits text into runs that are each drawn with one font: the fallback characters, or everything else.</summary>
    public static IEnumerable<(string Text, bool IsFallback)> SplitRuns(string text)
    {
        var start = 0;

        while (start < text.Length)
        {
            var isFallback = IsFallback(text[start]);
            var end = start + 1;

            while (end < text.Length && IsFallback(text[end]) == isFallback)
            {
                end++;
            }

            yield return (text[start..end], isFallback);
            start = end;
        }
    }

    private static XFont Create(Lazy<XGlyphTypeface> typeface, double size)
    {
        return new XFont(typeface.Value, size, XPdfFontOptions.UnicodeDefault, null);
    }

    /// <summary>A failure is not cached, so one bad load cannot fail every later export in the process.</summary>
    private static Lazy<XGlyphTypeface> Load(string fileName)
    {
        return new Lazy<XGlyphTypeface>(() => new XGlyphTypeface(XFontSource.GetOrCreateFrom(ReadResource(fileName))),
            LazyThreadSafetyMode.PublicationOnly);
    }

    private static byte[] ReadResource(string fileName)
    {
        var name = ResourcePrefix + fileName;

        using var stream = typeof(PdfFonts).Assembly.GetManifestResourceStream(name)
                           ?? throw new InvalidOperationException(
                               $"The embedded font '{name}' is missing from the FileExporter assembly.");

        using var bytes = new MemoryStream((int)stream.Length);
        stream.CopyTo(bytes);

        return bytes.ToArray();
    }
}
