using System.Globalization;
using FileExporter.Enums;
using FileExporter.Rules;

namespace FileExporter.Helpers;

internal static class ValueFormatter
{
    public static string FormatForCsv(object? value,
        IPropertyRule rule,
        CultureInfo culture,
        Func<Enum, string>? enumLabelResolver = null)
    {
        if (rule.CustomTransform is not null)
        {
            value = rule.CustomTransform(value);
        }

        if (value == null)
        {
            return rule.DefaultValue ?? string.Empty;
        }

        var type = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

        if (type.IsEnum)
        {
            return FormatEnumAsText(value, rule, enumLabelResolver);
        }

        switch (value)
        {
            case bool b:
                return FormatBooleanAsText(b);
            case DateTime dt:
                return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        if (!IsNumeric(type))
        {
            return Convert.ToString(value, culture) ?? string.Empty;
        }

        var precision = rule.Precision ?? 2;

        // A percentage column holds a fraction, exactly as Excel's "0.00%" format assumes: 0.5532 is 55.32%. CSV used
        // to print the unscaled number with a '%' glued on, so the same data read 0.55% in CSV and 55.32% in XLSX.
        if (rule.FormatType == ColumnFormatType.Percentage)
        {
            var scaled = Math.Round(Convert.ToDecimal(value, CultureInfo.InvariantCulture) * 100M, precision);

            return $"{scaled.ToString($"F{precision}", culture)}%";
        }

        var numeric = value switch
        {
            decimal dec => Math.Round(dec, precision)
                .ToString($"F{precision}", culture),
            double dbl => Math.Round(dbl, precision)
                .ToString($"F{precision}", culture),
            float fl => MathF.Round(fl, precision)
                .ToString($"F{precision}", culture),
            _ => Convert.ToString(value, culture) ?? string.Empty
        };

        return rule.FormatType switch
        {
            ColumnFormatType.Currency => Equals(culture, CultureInfo.InvariantCulture)
                ? numeric
                : $"{culture.NumberFormat.CurrencySymbol}{numeric}",

            _ => numeric
        };
    }

    public static object? FormatForXlsx(object? value,
        IPropertyRule rule,
        Func<Enum, string>? enumLabelResolver = null)
    {
        if (rule.CustomTransform is not null)
        {
            value = rule.CustomTransform(value);
        }

        if (value == null)
        {
            return rule.DefaultValue;
        }

        var type = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

        // Written as text, not as a logical cell: Excel renders TRUE/FALSE in the viewer's UI language and no number
        // format overrides that, so the same file read "TRUE" for one operator and "ИСТИНА" for the next.
        if (value is bool flag)
        {
            return FormatBooleanAsText(flag);
        }

        if (!type.IsEnum)
        {
            return value;
        }

        // Int mode stays a real number so the cell sorts and filters numerically; every other mode is text.
        return rule.EnumFormat == EnumFormatMode.Int
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : FormatEnumAsText(value, rule, enumLabelResolver);
    }

    /// <summary>
    ///     One implementation for both formats, so CSV and XLSX cannot drift apart again.
    /// </summary>
    private static string FormatBooleanAsText(bool value)
    {
        return value ? "Yes" : "No";
    }

    /// <summary>
    ///     One implementation for both formats, so CSV and XLSX cannot drift apart again.
    /// </summary>
    private static string FormatEnumAsText(object value, IPropertyRule rule, Func<Enum, string>? enumLabelResolver)
    {
        var number = Convert.ToInt64(value, CultureInfo.InvariantCulture)
            .ToString(CultureInfo.InvariantCulture);

        if (rule.EnumFormat == EnumFormatMode.Int)
        {
            return number;
        }

        var label = ResolveEnumLabel(value, enumLabelResolver);

        // No name and no label means the value is not a declared member. "7" is the whole truth; the old code wrote
        // "7 - " in XLSX and "7 - 7" in CSV.
        if (label.Length == 0)
        {
            return number;
        }

        return rule.EnumFormat == EnumFormatMode.Name ? label : $"{number} - {label}";
    }

    private static string ResolveEnumLabel(object value, Func<Enum, string>? enumLabelResolver)
    {
        if (enumLabelResolver is not null && value is Enum member)
        {
            var resolved = enumLabelResolver(member);

            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }
        }

        return Enum.GetName(value.GetType(), value) ?? string.Empty;
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(decimal)
               || type == typeof(double)
               || type == typeof(float)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(short)
               || type == typeof(byte)
               || type == typeof(uint)
               || type == typeof(ulong)
               || type == typeof(ushort)
               || type == typeof(sbyte);
    }
}
