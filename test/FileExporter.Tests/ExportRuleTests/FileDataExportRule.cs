using FileExporter.Rules;

namespace FileExporter.Tests.ExportRuleTests;

public class FileDataExportRule : ExportRule<FileData>
{
   public FileDataExportRule() : base("File Data")
   {
      GenerateRules();

      RuleFor(x => x.Description)
         .WriteToColumn("Description")
         .WithDefaultValue("Default text here");
      RuleFor(x => x.CreatedAt)
         .WriteToColumn("Creation date")
         .WithDefaultValue("22/02/2022");
   }
}