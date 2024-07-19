using BaseConverter.Attributes;

namespace FileExporter.Demo.Models;

public class DTO
{
    [PropertyBaseConverter]
    public long Id { get; set; }
    public string Name { get; set; } = null!;

    public override string ToString() => Name;
}