namespace FileExporter.Dtos;

public class MimeTypes
{
   public static readonly MimeTypes Csv = new("application/csv", ".csv");
   public static readonly MimeTypes Pdf = new("application/pdf", ".pdf");

   public static readonly MimeTypes Xlsx = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      ".xlsx");

   public static readonly MimeTypes Zip = new("application/zip", ".zip");

   private MimeTypes(string value, string fileExtension)
   {
      Value = value;
      Extension = fileExtension;
   }

   public string Value { get; init; }
   public string Extension { get; init; }

   public static implicit operator string(MimeTypes mimeType)
   {
      return mimeType.Value;
   }

   public override string ToString()
   {
      return Value;
   }
}