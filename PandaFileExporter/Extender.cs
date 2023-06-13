using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;

namespace ExcelExporter
{
    public static class Extender
    {
        public static byte[] ToByteArray(this string text)
        {
            using MemoryStream ms = new MemoryStream();
            using StreamWriter sw = new StreamWriter(ms);
            sw.Write(text);

            return ms.GetBuffer();
        }

        public static byte[] ToByteArray<T>(this IQueryable<T> array) where T : class
        {
            using MemoryStream ms = new MemoryStream();
            using StreamWriter sw = new StreamWriter(ms);
            foreach (object obj in array)
            {
                sw.Write(obj);
            }
            return ms.GetBuffer();
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> data, string name)
        {
            DataTable table = new(name);

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                table.Columns.Add(property.GetDisplayName(), typeof(string));
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();

                foreach (var prop in properties)
                    row[prop.GetDisplayName()] = prop.GetValue(item)?.ToString() ?? "";

                table.Rows.Add(row);
            }

            return table;
        }

        public static string GetDisplayName(this PropertyInfo propertyInfo)
        {
            var atts = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (atts.Length == 0)
                return propertyInfo.Name;
            return (atts[0] as DisplayNameAttribute)!.DisplayName;
        }

        public static string GetDisplayName<T>(this T model) where T : class
        {
            if (model.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).Any())
            {
                var attr = TypeDescriptor.GetAttributes(model.GetType())[2] as DisplayNameAttribute;
                return attr!.DisplayName;
            }

            return model.GetType().Name;
        }

        public static string GetDisplayName<T>(this DbSet<T> model) where T : class
        {
            if (model.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).Any())
            {
                var attr = TypeDescriptor.GetAttributes(model.GetType())[2] as DisplayNameAttribute;
                return attr!.DisplayName;
            }

            return model.GetType().Name;
        }

        public static string GetString(this byte[] data)
        {
            return System.Text.Encoding.Default.GetString(data);
        }
    }
}

