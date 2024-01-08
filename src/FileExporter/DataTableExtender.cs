using System.Collections.Generic;
using System.Linq;
using BaseConverter;

namespace FileExporter;

public static class DataTableExtender
{
    public static DataTable ToDataTable<T>(this IEnumerable<T>? data, string name)
    {
        return DataTable.FromQueryable(data?.AsQueryable(), name);
    }

    public static DataTable ToDataTable<T>(this IEnumerable<T>? data)
    {
        return DataTable.FromQueryable(data?.AsQueryable());
    }

    public static string Base36String(this object? value)
    {
        if (value is null) return "";

        _ = long.TryParse((string)value, out var convertedValue);

        return PandaBaseConverter.Base10ToBase36(convertedValue) ?? "";
    }
}