namespace FileExporter.Helpers;

internal static class ExportLimits
{
   public const int MaxXlsxRowsPerFile = 1_000_000;
   public const long ZipThresholdBytes = 10 * 1024 * 1024; // 10 MB
}