using System;

namespace FileExporter;

public interface IPropertyRule
{
    public string PropertyName();
    public string ColumnName();
    public string? DefaultColumnValue();
}