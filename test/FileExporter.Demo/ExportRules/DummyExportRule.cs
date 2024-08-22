using FileExporter.Demo.Models;

namespace FileExporter.Demo.ExportRules;

public class DummyExportRule : ExportRule<DummyTable>
{
    public DummyExportRule()
    {
        GenerateRules();

        RuleFor(x => x.Id).WriteToColumn("Id");
        RuleFor(x => x.RelatedId).WriteToColumn("Related Id");
        RuleFor(x => x.Name).WriteToColumn("Custom Name - No Description").WithDefaultValue("TEST");
        RuleFor(x => x.Status).WriteToColumn("Status");
        RuleFor(x => x.Price).WriteToColumn("Price");
        RuleFor(x => x.Count).WriteToColumn("Count");
        RuleFor(x => x.Min).WriteToColumn("Min");
        RuleFor(x => x.Average).WriteToColumn("Average");
        RuleFor(x => x.Max).WriteToColumn("Max");
        RuleFor(x => x.Description).WriteToColumn("Description");
        RuleFor(x => x.CreationDate).WriteToColumn("Created At");
        RuleFor(x => x.ExpirationDate).WriteToColumn("Expires At");
        RuleFor(x => x.Comment).WriteToColumn("Comment");
        RuleFor(x => x.Version).WriteToColumn("Version");

        RuleFor(x => x.DTO).WriteToColumn("Dto").WithDefaultValue("Default value");
    }
}