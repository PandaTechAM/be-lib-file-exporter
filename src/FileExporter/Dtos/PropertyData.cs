using System.Reflection;

namespace FileExporter.Dtos;

internal class PropertyData
{
   public required PropertyInfo Property { get; internal set; }
   public bool HasBaseConverter { get; internal set; }
   public required string Name { get; internal set; }
   public string ModelPropertyName => Property.Name;
}