using FileExporter.Demo.Models;
using FileExporter.Rules;

namespace FileExporter.Demo.ExportRules;

public class DummyExportRule : ExportRule<DummyTable>
{
   public DummyExportRule()
   {
      RuleFor(x => x.RelatedId)
         .WriteToColumn("Related Id");

      RuleFor(x => x.Name)
         .WriteToColumn("Custom Name - No Description")
         .WithDefaultValue("TEST");

      RuleFor(x => x.CreationDate)
         .WriteToColumn("Created At");

      RuleFor(x => x.ExpirationDate)
         .WriteToColumn("Expires At");

      RuleFor(x => x.Dto)
         .WriteToColumn("Dto")
         .WithDefaultValue("Default value");
   }

   public DummyExportRule(string fileName) : base(fileName)
   {
      RuleFor(x => x.RelatedId)
         .WriteToColumn("Related Id");

      RuleFor(x => x.Name)
         .WriteToColumn("Custom Name - No Description")
         .WithDefaultValue("TEST");

      RuleFor(x => x.CreationDate)
         .WriteToColumn("Created At");

      RuleFor(x => x.ExpirationDate)
         .WriteToColumn("Expires At");

      RuleFor(x => x.Dto)
         .WriteToColumn("Dto")
         .WithDefaultValue("Default value");
   }
}