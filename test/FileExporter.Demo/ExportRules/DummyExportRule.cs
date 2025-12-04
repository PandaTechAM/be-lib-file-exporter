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
         .HasOrder(3)
         .WithDefaultValue("TEST");

      RuleFor(x => x.CreationDate)
         .WriteToColumn("Created At")
         .HasWidth(4)
         .HasFormat(ColumnFormatType.DateTime);

      RuleFor(x => x.Dto)
         .WriteToColumn("Dto")
         .HasOrder(5)
         .WithDefaultValue("Default value");


      RuleFor(x => x.IntEnum)
         .HasOrder(6)
         .WithEnumFormat(EnumFormatMode.Int);

      RuleFor(x => x.StringEnum)
         .HasOrder(7)
         .WithEnumFormat(EnumFormatMode.Name);

      RuleFor(x => x.SomeRate)
         .HasOrder(8)
         .HasFormat(ColumnFormatType.Percentage)
         .HasPrecision(1);

      RuleFor(x => x.SomeDollars)
         .HasOrder(11)
         .HasFormat(ColumnFormatType.Currency)
         .HasPrecision(2);
   }
}