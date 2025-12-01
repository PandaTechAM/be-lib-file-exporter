using System;
using FileExporter.Enums;

namespace FileExporter.Rules;

public interface IPropertyRule
{
   string PropertyName { get; }
   string ColumnName { get; }
   string? DefaultValue { get; }

   int? Order { get; }
   bool IsIgnored { get; }

   ColumnFormatType FormatType { get; }
   int? Precision { get; }
   int? ColumnWidth { get; }
   EnumFormatMode EnumFormat { get; }
   Func<object?, object?>? CustomTransform { get; }
}