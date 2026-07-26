using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FileExporter.Dtos;
using FileExporter.Helpers;
using FileExporter.Rules;

namespace FileExporter.Exporters;

internal static class CsvExporter
{
    public static ExportFile Export<T>(IEnumerable<T> data, ExportRule<T> rule, ExportOptions? options = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(rule);

        var columns = ExportColumnBuilder.Build(rule, options);
        var baseName = NamingHelper.ResolveFileName(options?.FileName, rule.FileNameTemplate);
        var fileName = NamingHelper.EnsureExtension(baseName, MimeTypes.Csv.Extension);
        var enumLabelResolver = options?.EnumLabelResolver;

        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, new UTF8Encoding(true), leaveOpen: true))
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var csv = new CsvWriter(writer, config);

            foreach (var column in columns)
            {
                csv.WriteField(column.Header);
            }

            csv.NextRecord();

            foreach (var item in data)
            {
                foreach (var column in columns)
                {
                    var raw = column.Property.GetValue(item);
                    var formatted = ValueFormatter.FormatForCsv(raw,
                        column.Rule,
                        CultureInfo.InvariantCulture,
                        enumLabelResolver);
                    csv.WriteField(formatted);
                }

                csv.NextRecord();
            }
        }

        var bytes = ms.ToArray();

        if (bytes.Length < ExportLimits.ZipThresholdBytes)
        {
            return new ExportFile(fileName, MimeTypes.Csv, bytes);
        }

        // Use baseName (without extension) for zip entry naming
        return ZipHelper.CreateZip(baseName, MimeTypes.Csv, [bytes]);
    }
}
