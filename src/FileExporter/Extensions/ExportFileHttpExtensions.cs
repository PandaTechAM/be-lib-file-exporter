using FileExporter.Dtos;
using Microsoft.AspNetCore.Http;

namespace FileExporter.Extensions;

/// <summary>Extension methods that turn an <see cref="ExportFile" /> into a minimal API result.</summary>
public static class ExportFileHttpExtensions
{
    /// <summary>Wraps the exported file as an <see cref="IResult" /> file download with the correct MIME type and name.</summary>
    public static IResult ToFileResult(this ExportFile file)
    {
        return Results.File(
            file.Content,
            file.MimeType.Value,
            file.Name);
    }
}
