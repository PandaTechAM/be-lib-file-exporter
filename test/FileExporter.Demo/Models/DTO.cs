namespace FileExporter.Demo.Models;

public class DTO
{
   public long Id { get; set; }
   public string Name { get; set; } = null!;

   public override string ToString()
   {
      return Name;
   }
}