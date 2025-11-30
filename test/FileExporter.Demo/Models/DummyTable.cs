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
}