using FileExporter.Demo.Models;
using FileExporter.Enums;
using FileExporter.Rules;

namespace FileExporter.Demo.ExportRules;

public class DummyExportRule : ExportRule<DummyTable>
{
   public DummyExportRule()
   {
      RuleFor(x => x.RelatedId)
         .WriteToColumn("Related Id")
         .HasOrder(1);
      
      RuleFor(x => x.Id)
         .HasOrder(2);

      RuleFor(x => x.Name)
         .WriteToColumn("Custom Name - No Description")
         .HasOrder(7)
         .WithDefaultValue("TEST");

      RuleFor(x => x.CreationDate)
         .WriteToColumn("Created At")
         .HasWidth(1)
         .HasFormat(ColumnFormatType.DateTime);

      RuleFor(x => x.Dto)
         .WriteToColumn("Dto")
         .WithDefaultValue("Default value");

      RuleFor(x => x.IntEnum)
         .WithEnumFormat(EnumFormatMode.Int);

      RuleFor(x => x.StringEnum)
         .WithEnumFormat(EnumFormatMode.Name);

      RuleFor(x => x.SomeRate)
         .HasFormat(ColumnFormatType.Percentage)
         .HasPrecision(1);
      
      RuleFor(x => x.SomeDollars)
         .HasFormat(ColumnFormatType.Currency)
         .HasPrecision(2);
   }
}