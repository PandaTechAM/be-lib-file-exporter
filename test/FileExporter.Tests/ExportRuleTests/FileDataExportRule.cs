namespace FileExporter.Tests.ExportRuleTests;

public class FileDataExportRule : ExportRule<FileData>
{
    public FileDataExportRule() : base("File Data")
    {
        RuleFor(x => x.Id);
        RuleFor(x => x.Name);
        RuleFor(x => x.Description).WithDefaultValue("Default text here");
        RuleFor(x => x.CreatedAt).WriteToColumn("Creation date")
            .WithDefaultValue("22/02/2022");
        RuleFor(x => x.Comment);
    }
}