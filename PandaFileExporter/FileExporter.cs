using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Enumerations;

namespace PandaFileExporter;

public static class FileExporter
{
    private static readonly int ExportSizeLimit =
        Convert.ToInt32(Environment.GetEnvironmentVariable("EXPORT_SIZE_LIMIT") ?? "10");
    
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
            DataTable table = DataTable.FromQueryable(source);
            
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.Content = new StreamContent(memoryStream);
            response.Content =
                new ByteArrayContent(table.ToCsv()); 
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
            var table = DataTable.FromQueryable(source);
            return table.ToXlsx();
            
            /*// Convert source into data table
            var table = source.ToDataTable(typeof(T).GetDisplayName());

            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "ARIAL.ttf");
            
            // Create a new workbook and setup ARIAL.TTF font to be used in workbook
            LoadOptions.DefaultGraphicEngine =
                DefaultGraphicEngine.CreateWithFontsAndSystemFonts(
                    new MemoryStream(File.ReadAllBytes(fontPath))); //Fonts/ARIAL.TTF
            //DefaultGraphicEngine.CreateOnlyWithFonts(new MemoryStream(File.ReadAllBytes("content/ARIAL.TTF")));
            var loadOptions = new LoadOptions();
            //{ GraphicEngine = new DefaultGraphicEngine("content/ARIAL.TTF") };
            using var workbook = new XLWorkbook(loadOptions);

            // Create new worksheet and align
            var worksheet = workbook.Worksheets.Add(table);

            if (source != null && source.Any())
            {
                worksheet.ColumnsUsed(); //.AdjustToContents();
            }

            // Convert the workbook to a memory stream
            using var memoryStream = new MemoryStream();
            // Save workbook into memory stream
            workbook.SaveAs(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Return the byte array from the API endpoint
            return memoryStream.ToArray();*/
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
            DataTable table = DataTable.FromQueryable(source);
            return table.ToCsv();
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
            var table = DataTable.FromQueryable(source);
            return table.ToPdf();
            
            
            /*
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
                        else if (prop.Name.ToLower().Contains("id") && prop.PropertyType.UnderlyingSystemType.Name.Contains("Int64"))
                        {
                            var row = dataRow.AddCellToRow(prop.GetValue(data)?.ToString().Base36String());
                            row.SetFont(GetArialUtf8Font(12));
                        }
                        else if (prop.Name.ToLower().Contains("id") &&
                                 prop.PropertyType.UnderlyingSystemType.GenericTypeArguments.Any(x =>
                                     x.AssemblyQualifiedName?.Contains("Int64") ?? false))
                        {
                            var row = dataRow.AddCellToRow(prop.GetValue(data)?.ToString().Base36String());
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

            return memoryStream.ToArray();*/
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
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "ARIAL.ttf");
        
        var fontLoader = FontBuilder.New().FromFile(fontPath, fontSize).SetBold(bold); //Fonts/ARIAL.TTF

        return fontLoader;
    }
    
    private static Task<ExportFileData> GetFileData<T>(IQueryable<T> source, ExportType exportType)
    {
        var data = exportType switch
        {
            ExportType.XLSX => new ExportFileData
            {
                Data = ToExcelArray(source),
                Type = MimeTypes.XLSX,
            },
            ExportType.CSV => new ExportFileData
            {
                Data = ToCsvArray(source),
                Type = MimeTypes.CSV,
            },
            ExportType.PDF => new ExportFileData
            {
                Data = ToPdfArray(source),
                Type = MimeTypes.PDF,
            },
            _ => throw new ArgumentException("Unsupported data file type")
        };

        data.Name = $"{typeof(T).Name/*.ToSnakeCase()*/}.{exportType.ToString().ToLower()}";
        
        if (data.Data.Length > ExportSizeLimit * 1024 * 1024)
        {
            data = new ExportFileData
            {
                Data = ToZipArray(data.Data, $"{typeof(T).Name/*.ToSnakeCase()*/}.{exportType.ToString().ToLower()}"),
                Type = MimeTypes.ZIP,
                Name = $"{typeof(T).Name/*.ToSnakeCase()*/}.zip"
            };
        }

        return Task.FromResult(data);
    }
}