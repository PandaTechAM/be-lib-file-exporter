using FileExporter.Dtos;

namespace FileExporter.Helpers;

/// <summary>The one "file or zip" rule shared by every exporter.</summary>
internal static class ExportFileFactory
{
    public static ExportFile Create(string baseName, MimeTypes mimeType, byte[] bytes)
    {
        if (bytes.Length < ExportLimits.ZipThresholdBytes)
        {
            return new ExportFile(NamingHelper.EnsureExtension(baseName, mimeType.Extension), mimeType, bytes);
        }

        return ZipHelper.CreateZip(baseName, mimeType, [bytes]);
    }
}
