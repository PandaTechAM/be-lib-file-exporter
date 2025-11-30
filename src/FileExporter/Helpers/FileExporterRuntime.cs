using System;
using FileExporter.Rules;

namespace FileExporter.Helpers;

internal static class FileExporterRuntime
{
   private static IExportRuleRegistry? _registry;

   public static void Initialize(IExportRuleRegistry registry)
   {
      _registry = registry ?? throw new ArgumentNullException(nameof(registry));
   }

   public static IExportRuleRegistry Registry
   {
      get
      {
         return _registry ??
                throw new InvalidOperationException(
                   "FileExporter is not initialized. Call builder.AddFileExporter(...) at application startup.");
      }
   }
}