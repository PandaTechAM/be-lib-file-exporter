using System.Reflection;

namespace FileExporter.Dtos;

internal class PropertyData
{
    public PropertyInfo Property { get; internal set; }
    public bool HasBaseConverter { get; internal set; }
    public string Name { get; internal set; }
}