using PandaTech.IEnumerableFilters.Attributes;

namespace FileExporter.Tests;

[MappedToClass(typeof(DbModel))]
public class DbModelFilter
{
    [MappedToProperty(nameof(DbModel.Id))]
    public int Id { get; set; }

    [MappedToProperty(nameof(DbModel.Name))]
    public string Name { get; set; } = null!;
}