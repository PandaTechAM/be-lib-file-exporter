using FileExporter.Helpers;
using FileExporter.Rules;

namespace FileExporter.Tests;

/// <summary>Registers this assembly's rules, as <c>AddFileExporter</c> does at startup, for tests of the public surface.</summary>
public sealed class RegistryFixture
{
    public RegistryFixture()
    {
        FileExporterRuntime.Initialize(ExportRuleConfigurationLoader.LoadFromAssemblies(typeof(RegistryFixture).Assembly));
    }
}
