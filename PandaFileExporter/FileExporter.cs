using ClosedXML.Excel;
using ExcelExporter;
using Gehtsoft.PDFFlow.Builder;
using System.Net;
using Gehtsoft.PDFFlow.Models.Enumerations;
using System.Text;

public static class FileExporter
{
    public static HttpResponseMessage ExportToXlsx<T>(IQueryable<T> source) where T : class
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
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "export.xlsx";

            // Return the response message from the API endpoint
            return response;
        }
        catch (Exception)
        {
            throw new Exception("Excel file export failed!");
        }
    }

    public static HttpResponseMessage ExportToXls<T>(IQueryable<T> source) where T : class
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
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "export.xls";

            // Return the response message from the API endpoint
            return response;
        }
        catch (Exception)
        {
            throw new Exception("Excel file export failed!");
        }
    }

    public static HttpResponseMessage ExportToCsv<T>(IQueryable<T> source) where T : class
    {
        try
        {
            // Setup new StringBuilder for csv generation
            var stringBuiklder = new StringBuilder();

            // Get headers
            foreach (var item in typeof(T).GetProperties())
            {
                stringBuiklder.Append($"{item.GetDisplayName()},");
            }

            // Add data rows
            if (source.Count() > 0)
            {
                var data = source.ToList();

                for (int i = 0; i < source.Count(); i++)
                {
                    stringBuiklder.AppendLine();

                    foreach (var item in data[i].GetType().GetProperties())
                    {
                        stringBuiklder.Append($"{item.GetValue(data[i])},");
                    }
                }
            }

            // Create a response message with the file content
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.Content = new StreamContent(memoryStream);
            response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(stringBuiklder.ToString()));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "export.csv";

            // Return the response message from the API endpoint
            return response;
        }
        catch (Exception)
        {
            throw new Exception("Excel file export failed!");
        }
    }

    public static HttpResponseMessage ExportToPdf<T>(IQueryable<T> source) where T : class
    {
        try
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            using var memoryStream = new MemoryStream();

            // Source: https://csharpforums.net/threads/the-easiest-way-to-create-pdf-documents-in-c.7246/
            DocumentBuilder builder = DocumentBuilder.New();
            var section = builder.AddSection();

            var table = section.AddTable();

            // Table Headers
            var firstItem = source.FirstOrDefault();
            for (int i = 0; i < firstItem.GetType().GetProperties().Count(); i++)
            {
                table.AddColumnToTable();
            }

            // Add header row with names
            var headerRow = table.AddRow();
            foreach (var item in firstItem.GetType().GetProperties())
            {
                headerRow.AddCell(item.Name);
            }
            headerRow.SetHorizontalAlignment(HorizontalAlignment.Center).SetBold().ToTable();

            // Add data rows with values
            for (int i = 0; i < source.Count(); i++)
            {
                var dataRow = table.AddRow();
                foreach (var item in firstItem.GetType().GetProperties())
                {
                    dataRow.AddCellToRow(item.GetValue(firstItem).ToString());
                }
                dataRow.SetHorizontalAlignment(HorizontalAlignment.Center).ToTable();
            }

            section.ToDocument();

            builder.Build(memoryStream);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "downloaded.pdf";

            return response;

        }
        catch (Exception ex)
        {
            throw new Exception("PDF file export failed!");
        }
    }



    public static byte[] ToExcelArray<T>(IQueryable<T> source) where T : class
    {
        try
        {
            // Convert source into data table
            var table = source.ToDataTable(typeof(T).GetDisplayName());

            // Create a new workbook
            using var workbook = new XLWorkbook();
            // Create new worksheet and align
            workbook.Worksheets.Add(table).ColumnsUsed().AdjustToContents();

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

    public static byte[] ToExcelArray<T>(List<T> source) where T : class
    {
        try
        {
            // Convert source into data table
            var table = source.ToDataTable(typeof(T).GetDisplayName());

            // Create a new workbook
            using var workbook = new XLWorkbook();
            // Create new worksheet and align
            workbook.Worksheets.Add(table).ColumnsUsed().AdjustToContents();

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

    public static byte[] ToCsvArray<T>(List<T> source)
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
            if (source.Count > 0)
            {
                for (int i = 0; i < source.Count(); i++)
                {
                    stringBuilder.AppendLine();

                    foreach (var item in source[i].GetType().GetProperties())
                    {
                        stringBuilder.Append($"{item.GetValue(source[i])},");
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

    public static byte[] ToPdfArray<T>(IQueryable<T> source) where T : class
    {
        try
        {
            // Create Memory Stream
            using var memoryStream = new MemoryStream();

            // Build document
            DocumentBuilder builder = DocumentBuilder.New();
            var section = builder.AddSection();

            // Setup PDF
            section
                .SetSize(PaperSize.A4)
                .SetOrientation(PageOrientation.Landscape);

            // Create table
            var table = section.AddTable();

            // Table Headers
            var firstItem = source.FirstOrDefault();
            for (int i = 0; i < typeof(T).GetProperties().Count(); i++)
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

            if (source.Count() > 0)
            {
                // Add data rows with values
                var list = source.ToList();
                for (int i = 0; i < source.Count(); i++)
                {
                    var data = list[i];
                    var dataRow = table.AddRow();
                    foreach (var item in data.GetType().GetProperties())
                    {
                        dataRow.AddCellToRow(item.GetValue(data)?.ToString());
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

    public static byte[] ToPdfArray<T>(List<T> source) where T : class
    {
        try
        {
            // Create Memory Stream
            using var memoryStream = new MemoryStream();

            // Build document
            DocumentBuilder builder = DocumentBuilder.New();
            var section = builder.AddSection();

            // Setup PDF
            section
                .SetSize(PaperSize.A4)
                .SetOrientation(PageOrientation.Landscape);

            // Create table
            var table = section.AddTable();

            // Table Headers
            var firstItem = source.FirstOrDefault();
            for (int i = 0; i < typeof(T).GetProperties().Count(); i++)
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

            if (source.Count > 0)
            {
                // Add data rows with values
                var list = source.ToList();
                for (int i = 0; i < source.Count(); i++)
                {
                    var data = list[i];
                    var dataRow = table.AddRow();
                    foreach (var item in data.GetType().GetProperties())
                    {
                        var row = dataRow.AddCellToRow(item.GetValue(data)?.ToString());
                        row.SetFont(GetArialUtf8Font(12));
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


    private static FontBuilder GetArialUtf8Font(int fontSize = 9, bool bold = false)
    {
        var fontLoader = FontBuilder.New().FromFile("Fonts/ARIAL.TTF", fontSize).SetBold(bold);

        return fontLoader;
    }

}
