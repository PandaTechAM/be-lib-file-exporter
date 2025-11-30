using FileExporter.Demo.Models;
using FileExporter.Enums;
using FileExporter.Rules;

namespace FileExporter.Demo.ExportRules;

public class WideRowExportRule : ExportRule<WideRow>
{
   public WideRowExportRule()
   {
      RuleFor(x => x.Id)
         .WriteToColumn("ID")
         .HasWidth(8)
         .HasFormat(ColumnFormatType.Integer);

      RuleFor(x => x.ShortText)
         .HasWidth(12);

      RuleFor(x => x.MediumText)
         .HasWidth(25);

      RuleFor(x => x.LongText)
         .HasWidth(40);

      RuleFor(x => x.VeryLongText)
         .HasWidth(60);

      RuleFor(x => x.HugeText)
         .HasWidth(80);

      RuleFor(x => x.Amount)
         .WriteToColumn("Amount")
         .HasFormat(ColumnFormatType.Decimal)
         .HasPrecision(2)
         .HasWidth(18);

      RuleFor(x => x.LargeAmount)
         .WriteToColumn("Large Amount")
         .HasFormat(ColumnFormatType.Decimal)
         .HasPrecision(4)
         .HasWidth(20);

      RuleFor(x => x.CreatedAt)
         .WriteToColumn("Created")
         .HasFormat(ColumnFormatType.DateTime);

      RuleFor(x => x.UpdatedAt)
         .WriteToColumn("Updated")
         .HasFormat(ColumnFormatType.DateTime);
   }
}