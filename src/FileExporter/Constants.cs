using System;

namespace FileExporter;

public class Constants
{
    public const int NameLength = 30;
    public const int CsvLinesCount = 1_00;
    public const int ExcelLinesCount = 1_0;
    public const int PdfLinesCount = 10_000;
    public const string DateTimePlaceHolder = "{DateTime}";

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
