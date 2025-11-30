using System;
using System.Reflection;
using FileExporter.Helpers;
using FileExporter.Rules;
using FileExporter.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FileExporter.Extensions;

public static class FileExporterBuilderExtensions
{
   public static WebApplicationBuilder AddFileExporter(this WebApplicationBuilder builder,
      params Assembly[] assemblies)
   {
      ArgumentNullException.ThrowIfNull(builder);

      var assembliesToScan = assemblies is { Length: > 0 }
         ? assemblies
         :
         [
            Assembly.GetEntryAssembly()!
         ];

      var registry = ExportRuleConfigurationLoader.LoadFromAssemblies(assembliesToScan);

      // Initialize static runtime for extension methods
      FileExporterRuntime.Initialize(registry);

      // Register registry + exporter for DI (optional but useful)
      builder.Services.AddSingleton<IExportRuleRegistry>(_ => registry);
      builder.Services.AddSingleton<IFileExporter, Services.Implementations.FileExporter>();

      return builder;
   }
}