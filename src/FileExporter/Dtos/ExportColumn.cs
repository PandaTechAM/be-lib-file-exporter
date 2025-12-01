using System.Reflection;
using FileExporter.Rules;

namespace FileExporter.Dtos;

internal sealed class ExportColumn
{
   public required PropertyInfo Property { get; init; }
   public required IPropertyRule Rule { get; init; }
}