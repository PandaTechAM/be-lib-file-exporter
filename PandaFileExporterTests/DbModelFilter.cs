using PandaTech.IEnumerableFilters.Attributes;
using PandaTech.IEnumerableFilters.Converters;

namespace PandaFileExporterTests;

[MappedToClass(typeof(DbModel))]
public class DbModelFilter
{
    [MappedToProperty(nameof(DbModel.Id))]
    public int Id { get; set; }

    [MappedToProperty(nameof(DbModel.Name))]
    public string Name { get; set; } = null!;
}