namespace FileExporter.Rules;

public interface IPropertyRule
{
   public string PropertyName();
   public string ColumnName();
   public string? DefaultColumnValue();
}