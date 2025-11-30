namespace FileExporter.Rules;

internal interface IExportRuleRegistry
{
   ExportRule<T> GetRule<T>() where T : class;
   bool TryGetRule<T>(out ExportRule<T>? rule) where T : class;
}