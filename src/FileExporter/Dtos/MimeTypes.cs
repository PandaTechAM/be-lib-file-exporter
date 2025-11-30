namespace FileExporter.Dtos;

public sealed class MimeTypes
{
   public static readonly MimeTypes Csv = new("text/csv", ".csv");

   public static readonly MimeTypes Xlsx =
      new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");

   public static readonly MimeTypes Zip = new("application/zip", ".zip");

   private MimeTypes(string value, string extension)
   {
      Value = value;
      Extension = extension;
   }

   public string Value { get; }
   public string Extension { get; }

   public static implicit operator string(MimeTypes mimeType)
   {
      return mimeType.Value;
   }

   public override string ToString()
   {
      return Value;
   }
}