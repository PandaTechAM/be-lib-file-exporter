using ClosedXML.Excel;
using ExcelExporter;
using Gehtsoft.PDFFlow.Builder;
using System.Net;
using Gehtsoft.PDFFlow.Models.Enumerations;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Formats.Asn1;
using System.IO;
using CsvHelper;

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
            var table = source.ToDataTable(nameof(T));

            // Create a new workbook from the byte array
            using var workbook = new XLWorkbook();
            // Access the worksheet or perform any required operations
            workbook.Worksheets.Add(table).ColumnsUsed().AdjustToContents();

            // Convert the workbook to a memory stream
            using var memoryStream = new MemoryStream();
            // Save workbook into memory stream
            workbook.SaveAs(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Create a response message with the file content
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(memoryStream);
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

    //public static HttpResponseMessage ExportToPdf<T>(IList<T> source) where T : class
    //{
    //    try
    //    {
    //        var response = new HttpResponseMessage(HttpStatusCode.OK);

    //        // Set PDF documnet
    //        MemoryStream memoryStream = new MemoryStream();
    //        WriterProperties props = new WriterProperties()
    //            .SetStandardEncryption(Encoding.ASCII.GetBytes("reader_password"), Encoding.ASCII.GetBytes("permission_password"), EncryptionConstants.ALLOW_PRINTING,
    //                EncryptionConstants.ENCRYPTION_AES_128 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
    //        PdfWriter writer = new PdfWriter(memoryStream, props);
    //        writer.SetCloseStream(false);
    //        PdfDocument pdf = new PdfDocument(writer.SetSmartMode(true));
    //        Document document = new Document(pdf, PageSize.A4);

    //        // Header
    //        Paragraph header = new Paragraph(typeof(T).ToString())
    //           .SetTextAlignment(TextAlignment.CENTER)
    //           .SetFontSize(20);

    //        // New line
    //        Paragraph newline = new Paragraph(new Text("\n"));

    //        document.Add(newline);
    //        document.Add(header);

    //        // Line separator
    //        LineSeparator ls = new LineSeparator(new SolidLine());
    //        document.Add(ls);

    //        // Table
    //        Table table = new Table(2, false);

    //        // Table Headers
    //        var firstItem = source.FirstOrDefault();
    //        foreach (var item in firstItem.GetType().GetProperties())
    //        {
    //            var cell = new Cell(1, 1)
    //               .SetTextAlignment(TextAlignment.CENTER)
    //               .Add(new Paragraph(nameof(item)));
    //            table.AddCell(cell);
    //        }

    //        // Table rows
    //        for (int i = 0; i < source.Count(); i++)
    //        {
    //            var data = source[i];
    //            foreach (var item in data.GetType().GetProperties())
    //            {
    //                var cell = new Cell(1, 1)
    //                   .SetTextAlignment(TextAlignment.CENTER)
    //                   .Add(new Paragraph(item.GetValue(item).ToString()));
    //                table.AddCell(cell);
    //            }
    //        }

    //        document.Add(newline);
    //        document.Add(table);

    //        // Page numbers
    //        int n = source.Count();
    //        for (int i = 1; i <= n; i++)
    //        {
    //            //document.ShowTextAligned(new Paragraph(String
    //            //   .Format("page" + i + " of " + n)),
    //            //   559, 806, i, TextAlignment.RIGHT,
    //            //   VerticalAlignment.TOP, 0);
    //        }

    //        // Close document
    //        document.Close();

    //        response.Content = new StreamContent(memoryStream);
    //        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
    //        response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
    //        response.Content.Headers.ContentDisposition.FileName = "export.pdf";

    //        return response;

    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception("PDF file export failed!");
    //    }
    //}

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
        catch (Exception)
        {
            throw new Exception("Export failed!");
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
        catch (Exception)
        {
            throw new Exception("Export failed!");
        }
    }

    public static byte[] ToCsvArray<T>(List<T> source)
    {
        try
        {
            // Setup new StringBuilder for csv generation
            var stringBuiklder = new StringBuilder();

            // Get headers
            foreach (var item in typeof(T).GetProperties())
            {
                stringBuiklder.Append($"{item.GetDisplayName()};");
            }

            // Add data rows
            if (source.Count > 0)
            {
                for (int i = 0; i < source.Count(); i++)
                {
                    stringBuiklder.AppendLine();

                    foreach (var item in source[i].GetType().GetProperties())
                    {
                        stringBuiklder.Append($"{item.GetDisplayName()},");
                    }
                }
            }

            return Encoding.UTF8.GetBytes(stringBuiklder.ToString());
        }
        catch (Exception)
        {
            throw new Exception("Export failed!");
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
        catch (Exception ex)
        {
            throw new Exception("Export failed!");
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
        catch (Exception ex)
        {
            throw new Exception("Export failed!");
        }
    }

}
