using System;

namespace FileExporter;

public class Constants
{
    public const int NameLength = 30;
    public const int CsvLinesCount = 1_00; //todo :: change
    public const int ExcelLinesCount = 1_0; //todo :: change
    public const int PdfLinesCount = 100; //todo :: change
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
