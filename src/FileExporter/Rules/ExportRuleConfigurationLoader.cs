using System.Reflection;

namespace FileExporter.Rules;

internal static class ExportRuleConfigurationLoader
{
   public static ExportRuleRegistry LoadFromAssemblies(params Assembly[] assemblies)
   {
      return new ExportRuleRegistry(assemblies);
   }
}