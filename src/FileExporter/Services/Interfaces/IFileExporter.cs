using System.Collections.Generic;
using FileExporter.Dtos;

namespace FileExporter.Services.Interfaces;

public interface IFileExporter
{
   ExportFile ExportToCsv<T>(IEnumerable<T> data) where T : class;
   ExportFile ExportToXlsx<T>(IEnumerable<T> data) where T : class;
}