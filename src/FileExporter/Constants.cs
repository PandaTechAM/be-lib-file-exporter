using System;

namespace FileExporter;

public class Constants
{
    public const int NameLength = 30;
    public const int CsvLinesCount = 1_000_000;
    public const int ExcelLinesCount = 1_000_000;
    public const int PdfLinesCount = 10_000; 
    public const int FileMaxSizeInBytes = 10 * 1024 * 1024; //10 MB
    public const string DateTimePlaceHolder = "{DateTime}";
    public const string DefaultFontName = "Arial";
    public const int DefaultFontSize = 10;

    public static readonly Type[] NumericTypesWithNullables =
                [
                        typeof(int), typeof(double), typeof(decimal), typeof(long),
                        typeof(short), typeof(sbyte), typeof(byte), typeof(ulong),
                        typeof(ushort), typeof(uint), typeof(float),
                        typeof(int?), typeof(double?), typeof(decimal?), typeof(long?),
                        typeof(short?), typeof(sbyte?), typeof(byte?), typeof(ulong?),
                        typeof(ushort?), typeof(uint?), typeof(float?)
                ];
}
