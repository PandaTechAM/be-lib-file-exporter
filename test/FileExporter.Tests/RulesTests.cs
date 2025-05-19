using System.IO.Compression;
using System.Text.RegularExpressions;
using FileExporter.Dtos;
using FileExporter.Tests.ExportRuleTests;

namespace FileExporter.Tests;

public sealed class FileDataExportRuleTests
{
   private readonly FileDataExportRule _rule = new();
   private const int SampleSize = 10;
   private static readonly List<FileData> Sample = GenerateData(SampleSize);

   private static List<FileData> GenerateData(int rows, int textLen = 32_000)
   {
      var txt = new string('X', textLen);
      var list = new List<FileData>(rows);

      for (var i = 0; i < rows; i++)
         list.Add(new FileData
         {
            Id = i,
            Name = $"Test{i}",
            Description = txt,
            CreatedAt = DateTime.UtcNow,
            Comment = txt
         });

      return list;
   }

   #region basic (≤10 MB)

   [Fact]
   public void ToCsv_WithData_Returns_File() => AssertValid(_rule.ToCsv(Sample), MimeTypes.Csv, ".csv");

   [Fact]
   public void ToXlsx_WithData_Returns_File() => AssertValid(_rule.ToXlsx(Sample), MimeTypes.Xlsx, ".xlsx");

   [Fact]
   public void ToPdf_WithData_Returns_File() => AssertValid(_rule.ToPdf(Sample), MimeTypes.Pdf, ".pdf");

   [Fact]
   public void ToCsv_WithEmptyList_Returns_HeaderOnly_File() => AssertValid(_rule.ToCsv([]), MimeTypes.Csv, ".csv");

   [Fact]
   public void ToCsv_WithNull_Throws() => Assert.Throws<ArgumentNullException>(() => _rule.ToCsv(null!));

   [Fact]
   public void ToXlsx_WithNull_Throws() => Assert.Throws<ArgumentNullException>(() => _rule.ToXlsx(null!));

   [Fact]
   public void ToPdf_WithNull_Throws() => Assert.Throws<ArgumentNullException>(() => _rule.ToPdf(null!));

   #endregion

   #region zip (>10 MB)

   [Fact]
   public void ToCsv_BigData_Returns_Zip()
   {
      var big = GenerateData(1000, 100_000); // ~25 MB+
      var zip = _rule.ToCsv(big);

      
      AssertZip(zip, ".csv");
   }

   #region ZIP – file size > 10 MB

   [Fact]
   public void ToXlsx_When_FileIsBiggerThan10Mb_Returns_Zip()
   {
      var plenty = GenerateData(500_000);

      var file = _rule.ToXlsx(plenty);

      AssertZip(file, ".xlsx");
   }

   #endregion

   #endregion

   private static void AssertValid(ExportFile file, string mime, string ext)
   {
      Assert.NotNull(file);
      Assert.Equal(mime, file.Type);
      Assert.False(string.IsNullOrWhiteSpace(file.Name));
      Assert.Matches(new Regex($"{Regex.Escape(ext)}$", RegexOptions.IgnoreCase), file.Name);
      Assert.NotEmpty(file.Data);
   }

   private static void AssertZip(ExportFile file, string innerExt)
   {
      AssertValid(file, MimeTypes.Zip, ".zip");

      using var ms = new MemoryStream(file.Data);
      using var za = new ZipArchive(ms, ZipArchiveMode.Read, true);

      Assert.NotEmpty(za.Entries);
      foreach (var e in za.Entries)
         Assert.EndsWith(innerExt, e.Name, StringComparison.OrdinalIgnoreCase);
   }
}