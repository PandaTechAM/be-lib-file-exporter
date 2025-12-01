namespace FileExporter.Demo.Models;

public class WideRow
{
   public int Id { get; set; }

   public string ShortText { get; set; } = string.Empty;
   public string MediumText { get; set; } = string.Empty;
   public string LongText { get; set; } = string.Empty;
   public string VeryLongText { get; set; } = string.Empty;
   public string HugeText { get; set; } = string.Empty;

   public decimal Amount { get; set; }
   public decimal LargeAmount { get; set; }

   public DateTime CreatedAt { get; set; }
   public DateTime? UpdatedAt { get; set; }
}