using FileExporter.Dtos;

namespace FileExporter.Tests.ExportRuleTests;

public class RulesTests
{
   private static List<FileData> GenerateData(int capacity)
   {
      var data = new List<FileData>(capacity);
      for (var i = 0; i < data.Count; i++)
      {
         data.Add(new FileData
         {
            Id = i,
            Name = $"Test{i}",
            Description = $"Test{i} Description",
            CreatedAt = DateTime.UtcNow,
            Comment = $"Test{i} Comment"
         });
      }

      return data;
   }

   [Fact]
   public void Export_To_Xlsx_Via_Rules()
   {
      var data = GenerateData(10);

      var rule = new FileDataExportRule();
      var result = rule.ToXlsx(data);

      Assert.NotEmpty(result.Data);
      Assert.Equal(MimeTypes.Xlsx, result.Type);
      Assert.NotEmpty(result.Name);
   }

   [Fact]
   public void Export_To_Csv_Via_Rules()
   {
      var data = GenerateData(10);

      var rule = new FileDataExportRule();
      var result = rule.ToCsv(data);

      Assert.NotEmpty(result.Data);
      Assert.Equal(MimeTypes.Csv, result.Type);
      Assert.NotEmpty(result.Name);
   }

   [Fact]
   public void Export_To_Pdf_Via_Rules()
   {
      var data = GenerateData(10);

      var rule = new FileDataExportRule();
      var result = rule.ToPdf(data);

      Assert.NotEmpty(result.Data);
      Assert.Equal(MimeTypes.Pdf, result.Type);
      Assert.NotEmpty(result.Name);
   }
}