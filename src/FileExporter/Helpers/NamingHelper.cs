using System.Globalization;
using System.Text;

namespace FileExporter.Helpers;

internal static class NamingHelper
{
    private const string DateTimePlaceholder = "{DateTime}";
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    ///     Cap for a resolved file name. It has to hold a readable base name plus a 19-character timestamp, so the
    ///     old value of 30 truncated almost every real name mid-word.
    /// </summary>
    private const int MaxNameLength = 100;

    /// <summary>
    ///     Turns a rule's configured name into a template: title-cased, with a <c>{DateTime}</c> placeholder appended
    ///     when the caller did not place one. Nothing is substituted here — see <see cref="Stamp" />.
    /// </summary>
    internal static string BuildTemplate(string baseName)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            throw new ArgumentException("Base name can not be null or empty.", nameof(baseName));
        }

        // A name that positions the placeholder itself is taken exactly as written. Title-casing it would rewrite
        // "{DateTime}" to "{Date Time}" — ToDisplayTitle reads it as two words — and Stamp could never match it again.
        if (baseName.Contains(DateTimePlaceholder, StringComparison.Ordinal))
        {
            return baseName.Trim();
        }

        return $"{ToDisplayTitle(baseName)} {DateTimePlaceholder}";
    }

    /// <summary>The template with its placeholder removed, for naming a worksheet.</summary>
    internal static string WithoutTimestamp(string template)
    {
        var words = template
            .Replace(DateTimePlaceholder, " ", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(' ', words);
    }

    /// <summary>
    ///     Substitutes <c>{DateTime}</c> with the current UTC time and sanitizes the result. Called at export time,
    ///     never at rule construction: an <c>ExportRule</c> is a boot-time singleton, so stamping it in the constructor
    ///     froze every download at the pod's start time.
    /// </summary>
    internal static string Stamp(string template)
    {
        var now = DateTime.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture);

        return template
            .Replace(DateTimePlaceholder, now, StringComparison.Ordinal)
            .ToValidName(MaxNameLength);
    }

    /// <summary>
    ///     Resolves the base file name for one export. An explicit per-request name wins and is used verbatim; only
    ///     the rule's own name carries an automatic timestamp.
    /// </summary>
    internal static string ResolveFileName(string? requested, string ruleTemplate)
    {
        return Stamp(string.IsNullOrWhiteSpace(requested) ? ruleTemplate : requested);
    }

    internal static string ToDisplayTitle(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return string.Empty;
        }

        var s = identifier.Trim();
        var sb = new StringBuilder(s.Length + 4);

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            var prev = i > 0 ? s[i - 1] : '\0';
            var next = i < s.Length - 1 ? s[i + 1] : '\0';

            var isUpper = char.IsUpper(c);
            var prevIsUpper = char.IsUpper(prev);
            var prevIsLetterOrDigit = char.IsLetterOrDigit(prev);


            var shouldInsertSpace =
                i > 0 &&
                (
                    (!prevIsUpper && isUpper && prevIsLetterOrDigit) ||
                    (char.IsDigit(prev) && char.IsLetter(c)) ||
                    (prevIsUpper && isUpper && next != '\0' && char.IsLower(next))
                );

            if (shouldInsertSpace)
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    internal static string EnsureExtension(string name, string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return name;
        }

        return name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? name
            : name + extension;
    }
}
