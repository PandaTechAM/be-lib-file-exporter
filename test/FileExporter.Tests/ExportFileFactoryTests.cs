using System.IO.Compression;
using FileExporter.Dtos;
using FileExporter.Helpers;

namespace FileExporter.Tests;

public class ExportFileFactoryTests
{
    private const int ZipThreshold = 10 * 1024 * 1024;

    [Theory]
    [InlineData(100)]
    [InlineData(ZipThreshold - 1)]
    public void Below_10_mb_the_file_is_returned_with_its_extension(int size)
    {
        var bytes = new byte[size];

        var file = ExportFileFactory.Create("Report", MimeTypes.Pdf, bytes);

        Assert.Equal("Report.pdf", file.Name);
        Assert.Same(MimeTypes.Pdf, file.MimeType);
        Assert.Same(bytes, file.Content);
    }

    [Fact]
    public void At_10_mb_the_file_is_zipped_under_its_own_name()
    {
        var bytes = new byte[ZipThreshold];
        bytes[^1] = 42;

        var file = ExportFileFactory.Create("Report", MimeTypes.Pdf, bytes);

        Assert.Equal("Report.zip", file.Name);
        Assert.Same(MimeTypes.Zip, file.MimeType);

        using var zip = new ZipArchive(new MemoryStream(file.Content), ZipArchiveMode.Read);
        var entry = Assert.Single(zip.Entries);
        Assert.Equal("Report.pdf", entry.FullName);

        using var content = new MemoryStream();
        entry.Open().CopyTo(content);
        Assert.Equal(bytes, content.ToArray());
    }

    [Fact]
    public void An_existing_extension_is_not_doubled()
    {
        var file = ExportFileFactory.Create("Report.csv", MimeTypes.Csv, [1, 2, 3]);

        Assert.Equal("Report.csv", file.Name);
    }
}
