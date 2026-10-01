namespace FileExporter.Helpers;

internal static class ExportLimits
{
    public const int MaxXlsxRowsPerFile = 1_048_575;
    public const int MaxPdfRows = 100_000;
    public const int MaxSheetNameLength = 31;
    public const long ZipThresholdBytes = 10 * 1024 * 1024; // 10 MB
}
