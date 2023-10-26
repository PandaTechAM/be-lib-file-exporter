using ClosedXML.Excel;
using ExcelExporter;
using Gehtsoft.PDFFlow.Builder;
using System.Net;
using Gehtsoft.PDFFlow.Models.Enumerations;
using System.Text;
using ClosedXML.Graphics;
using PandaFileExporter;
using System.IO.Compression;
using System.Text.Json;
using DocumentFormat.OpenXml.Vml.Office;

public static class FileExporter
{
    public static HttpResponseMessage ExportToXlsx<T>(IQueryable<T>? source)
    {
        try
        {
            // Convert source into data table
            var table = source.ToDataTable(nameof(T));

            // Create a new workbook
            using var workbook = new XLWorkbook();
            // Create new worksheet and align
            workbook.Worksheets.Add(table).ColumnsUsed().AdjustToContents();

            // Convert the workbook to a memory stream
            using var memoryStream = new MemoryStream();
            // Save workbook into memory stream
            workbook.SaveAs(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Create a response message with the file content
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "export.xlsx";

            // Return the response message from the API endpoint
            return response;
        }
        catch (Exception)
        {
            throw new Exception("Excel file export failed!");
        }
    }

    public static HttpResponseMessage ExportToCsv<T>(IQueryable<T>? source)
    {
        try
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.Content = new StreamContent(memoryStream);
            response.Content =
                new ByteArrayContent(ToCsvArray(source?.ToList())); // TODO: add ToCsvArray() method from IQueryable
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MimeTypes.CSV); // "text/csv"
            response.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "export.csv";

            // Return the response message from the API endpoint
            return response;
        }
        catch (Exception)
        {
            throw new Exception("Excel file export failed!");
        }
    }

    public static HttpResponseMessage ExportToPdf<T>(IQueryable<T>? source)
    {
        try
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(ToPdfArray(source)));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "downloaded.pdf";

            return response;
        }
        catch (Exception)
        {
            throw new Exception("PDF file export failed!");
        }
    }


    public static byte[] ToExcelArray<T>(IQueryable<T>? source)
    {
        try
        {
            // Convert source into data table
            var table = source.ToDataTable(typeof(T).GetDisplayName());

            // Create a new workbook and setup ARIAL.TTF font to be used in workbook
            LoadOptions.DefaultGraphicEngine =
                DefaultGraphicEngine.CreateWithFontsAndSystemFonts(
                    new MemoryStream(File.ReadAllBytes("Fonts/ARIAL.TTF")));
            //DefaultGraphicEngine.CreateOnlyWithFonts(new MemoryStream(File.ReadAllBytes("Fonts/ARIAL.TTF")));
            var loadOptions = new LoadOptions();
            //{ GraphicEngine = new DefaultGraphicEngine("Fonts/ARIAL.TTF") };
            using var workbook = new XLWorkbook(loadOptions);

            // Create new worksheet and align
            var worksheet = workbook.Worksheets.Add(table);

            if (source != null && source.Any())
            {
                worksheet.ColumnsUsed().AdjustToContents();
            }

            // Convert the workbook to a memory stream
            using var memoryStream = new MemoryStream();
            // Save workbook into memory stream
            workbook.SaveAs(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Return the byte array from the API endpoint
            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Export failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Export failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    public static byte[] ToExcelArray<T>(IEnumerable<T>? source)
    {
        return ToExcelArray(source?.AsQueryable());
    }
    
    public static byte[] ToExcelArray<T>(List<T>? source)
    {
        return ToExcelArray(source?.AsQueryable());
    }

    public static byte[] ToCsvArray<T>(IQueryable<T>? source)
    {
        try
        {
            // Setup new StringBuilder for csv generation
            var stringBuilder = new StringBuilder();

            // Get headers
            foreach (var item in typeof(T).GetProperties())
            {
                stringBuilder.Append($"{item.GetDisplayName()},");
            }

            // Add data rows
            if (source is not null && source.Any())
            {
                foreach (var item in source)
                {
                    stringBuilder.AppendLine();

                    foreach (var prop in item!.GetType().GetProperties())
                    {
                        if (prop.PropertyType.Name == "List`1")
                        {
                            var listItem = prop.GetValue(item);
                            var method =
                                typeof(Extender).GetMethod("ListAsString")!.MakeGenericMethod(
                                    prop.PropertyType.GetGenericArguments()[0]);

                            stringBuilder.Append($"{method.Invoke(null, new[]
                            {
                                listItem!,
                                "; "
                            }) as string ?? ""},");
                        }
                        else if (prop.PropertyType.IsArray && prop.PropertyType.Name != "String")
                        {
                            var listItem = prop.GetValue(item);

                            var method =
                                typeof(Extender).GetMethod("EnumAsString")!.MakeGenericMethod(
                                    listItem!.GetType().GetElementType()!);

                            stringBuilder.Append($"{method.Invoke(null, new[]
                            {
                                listItem!,
                                "; "
                            }) as string ?? ""},");
                        }
                        else
                        {
                            var value = prop.GetValue(item)?.ToString();
                            if (value != null)
                            {
                                // Check if the value contains commas or double quotes
                                if (value.Contains(',') || value.Contains('"'))
                                {
                                    // Escape double quotes by doubling them
                                    value = value.Replace("\"", "\"\"");

                                    // Enclose the value in double quotes
                                    value = $"\"{value}\"";
                                }
                                stringBuilder.Append($"{value},");
                            }
                            else
                            {
                                // Append the value to the CSV
                                stringBuilder.Append($"{prop.GetValue(item)},");
                            }
                        }
                    }
                }
            }

            var data = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            var result = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Export failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Export failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }
    
    public static byte[] ToCsvArray<T>(IEnumerable<T>? source)
    {
        return ToCsvArray(source?.AsQueryable());
    }
    
    public static byte[] ToCsvArray<T>(List<T>? source)
    {
        return ToCsvArray(source?.AsQueryable());
    }

    public static byte[] ToPdfArray<T>(IQueryable<T>? source)
    {
        try
        {
            // Create Memory Stream
            using var memoryStream = new MemoryStream();

            // Build document
            var builder = DocumentBuilder.New();
            var section = builder.AddSection();

            // Setup PDF
            section
                .SetSize(PaperSize.A4)
                .SetOrientation(PageOrientation.Landscape);

            // Create table
            var table = section.AddTable();

            // Table Headers
            for (var i = 0; i < typeof(T).GetProperties().Length; i++)
            {
                table.AddColumnToTable();
            }

            // Add header row with names
            var headerRow = table.AddRow();
            foreach (var item in typeof(T).GetProperties())
            {
                //headerRow.AddCell(item.Name);
                headerRow.AddCell(item.GetDisplayName());
            }

            headerRow.SetHorizontalAlignment(HorizontalAlignment.Center).SetBold().ToTable();

            if (source is not null && source.Any())
            {
                // Add data rows with values
                var list = source.ToList();
                for (var i = 0; i < source.Count(); i++)
                {
                    var data = list[i];
                    var dataRow = table.AddRow();
                    foreach (var prop in data.GetType().GetProperties())
                    {
                        if (prop.PropertyType.Name == "List`1")
                        {
                            var listItem = prop.GetValue(data);
                            var method =
                                typeof(Extender).GetMethod("ListAsString")!.MakeGenericMethod(
                                    prop.PropertyType.GetGenericArguments()[0]);

                            var row = dataRow.AddCellToRow(method.Invoke(null, new[]
                            {
                                listItem!,
                                "; "
                            }) as string ?? "");
                            row.SetFont(GetArialUtf8Font(12));
                        }
                        else if (prop.PropertyType.IsArray && prop.PropertyType.Name != "String")
                        {
                            var listItem = prop.GetValue(data);
                            var method =
                                typeof(Extender).GetMethod("EnumAsString")!.MakeGenericMethod(
                                    listItem!.GetType().GetElementType()!);

                            var row = dataRow.AddCellToRow(method.Invoke(null, new[]
                                        {
                                listItem!,
                                "; "
                            }) as string ?? "");
                            row.SetFont(GetArialUtf8Font(12));
                        }
                        else
                        {
                            var row = dataRow.AddCellToRow(prop.GetValue(data)?.ToString());
                            row.SetFont(GetArialUtf8Font(12));
                        }
                    }

                    dataRow.SetHorizontalAlignment(HorizontalAlignment.Center).ToTable();
                }
            }
            else
            {
                // Add empty row
                var dataRow = table.AddRow();
                foreach (var item in typeof(T).GetProperties())
                {
                    dataRow.AddCellToRow(string.Empty);
                }
            }


            // Generate Document
            section.ToDocument();

            // Build document
            builder.Build(memoryStream);

            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Export failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Export failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    public static byte[] ToPdfArray<T>(IEnumerable<T>? source)
    {
        return ToPdfArray(source?.AsQueryable());
    }
    
    public static byte[] ToPdfArray<T>(List<T>? source)
    {
        return ToPdfArray(source?.AsQueryable());
    }

    public static byte[] ToZipArray(byte[] source, string filename)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry(filename, CompressionLevel.Optimal);

                using var entryStream = entry.Open();
                entryStream.Write(source, 0, source.Length);
                entryStream.Close();
            }

            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(new Exception($"Zip failed with message: {e.Message}"));
            Console.WriteLine(new Exception($"Zip failed with inner message: {e.InnerException?.Message}"));
            throw;
        }
    }

    private static FontBuilder GetArialUtf8Font(int fontSize = 9, bool bold = false)
    {
        var fontLoader = FontBuilder.New().FromFile("Fonts/ARIAL.TTF", fontSize).SetBold(bold);

        return fontLoader;
    }
}