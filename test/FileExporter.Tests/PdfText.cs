using UglyToad.PdfPig;

namespace FileExporter.Tests;

/// <summary>
///     Reads a generated PDF back. Text is compared with all whitespace removed, so where a cell wraps can never break
///     an assertion.
/// </summary>
internal static class PdfText
{
    public static List<string> Pages(byte[] pdf)
    {
        using var document = PdfDocument.Open(pdf);

        return document.GetPages().Select(page => Strip(page.Text)).ToList();
    }

    public static string Strip(string text)
    {
        return string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
    }
}
