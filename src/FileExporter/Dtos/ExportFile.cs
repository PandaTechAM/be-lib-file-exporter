namespace FileExporter.Dtos;

/// <summary>An exported file: its download name, MIME type, and raw byte content.</summary>
/// <param name="name">Download file name, including extension.</param>
/// <param name="mimeType">MIME type describing the content.</param>
/// <param name="content">Raw file bytes.</param>
public sealed class ExportFile(string name, MimeTypes mimeType, byte[] content)
{
    /// <summary>Download file name, including extension.</summary>
    public string Name { get; } = name;

    /// <summary>MIME type describing the file content.</summary>
    public MimeTypes MimeType { get; } = mimeType;

    /// <summary>Raw file bytes.</summary>
    public byte[] Content { get; } = content;
}
