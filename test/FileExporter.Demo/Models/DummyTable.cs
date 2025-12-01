namespace FileExporter.Demo.Models;

public class DummyTable
{
   public int Id { get; set; }
   public int RelatedId { get; set; }
   public string Name { get; set; } = string.Empty;
   public string Comment { get; set; } = string.Empty;
   public DateTime CreationDate { get; set; }
   public DateTime ExpirationDate { get; set; }
   public string? Dto { get; set; }

   public DefaultEnum DefaultEnum { get; set; } = DefaultEnum.Vardan;
   public IntEnum IntEnum { get; set; } = IntEnum.Vazgen;
   public StringEnum StringEnum { get; set; } = StringEnum.Vazgen;

   public decimal SomeRate { get; set; } = 0.5532m;
   public decimal SomeDollars { get; set; } = 1234.5623m;
}