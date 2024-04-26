namespace FileExporter
{
    public class MimeTypes
    {
        public string Value { get; init; }
        public string Extension { get; init; }

        private MimeTypes(string value, string fileExtension)
        {
            Value = value;
            Extension = fileExtension;
        }

        public static implicit operator string(MimeTypes mimeType) => mimeType.Value;

        public override string ToString() => Value;

        public static readonly MimeTypes CSV = new("application/csv", ".csv");
        public static readonly MimeTypes PDF = new("application/pdf", ".pdf");
        public static readonly MimeTypes XLSX = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
        public static readonly MimeTypes ZIP = new("application/zip", ".zip");


    }
}
