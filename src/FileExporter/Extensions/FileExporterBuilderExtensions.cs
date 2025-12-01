using System;
using System.Reflection;
using FileExporter.Helpers;
using FileExporter.Rules;
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
            Assembly.GetEntryAssembly()
            ?? Assembly.GetExecutingAssembly()
         ];

      var registry = ExportRuleConfigurationLoader.LoadFromAssemblies(assembliesToScan);

      FileExporterRuntime.Initialize(registry);

      builder.Services.AddSingleton<IExportRuleRegistry>(_ => registry);
      return builder;
   }
}