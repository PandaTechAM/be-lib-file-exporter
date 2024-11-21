namespace FileExporter.Tests.ExportRuleTests;

public class FileData
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Comment { get; set; }
}