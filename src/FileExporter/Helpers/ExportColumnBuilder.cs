using FileExporter.Dtos;
using FileExporter.Rules;

namespace FileExporter.Helpers;

/// <summary>
///     Resolves a rule's property rules against the model's actual properties, applying any per-request header
///     overrides. Shared by both exporters so CSV and XLSX cannot disagree about columns or headers.
/// </summary>
internal static class ExportColumnBuilder
{
    public static List<ExportColumn> Build<T>(ExportRule<T> rule, ExportOptions? options)
        where T : class
    {
        var properties = typeof(T)
            .GetProperties()
            .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

        var headerOverrides = options?.ColumnHeaders;
        var columns = new List<ExportColumn>();

        foreach (var propertyRule in rule.Rules)
        {
            if (!properties.TryGetValue(propertyRule.PropertyName, out var property))
            {
                continue;
            }

            columns.Add(new ExportColumn
            {
                Property = property,
                Rule = propertyRule,
                Header = ResolveHeader(propertyRule, headerOverrides)
            });
        }

        return columns;
    }

    private static string ResolveHeader(IPropertyRule rule, IReadOnlyDictionary<string, string>? overrides)
    {
        if (overrides is not null
            && overrides.TryGetValue(rule.PropertyName, out var custom)
            && !string.IsNullOrWhiteSpace(custom))
        {
            return custom;
        }

        return rule.ColumnName;
    }
}
