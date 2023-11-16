using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using BaseConverter;
using Microsoft.EntityFrameworkCore;

namespace ExcelExporter
{
    public static class Extender
    {
        public static byte[] ToByteArray(this string text)
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(text);

            return ms.GetBuffer();
        }

        public static byte[] ToByteArray<T>(this IQueryable<T>? array) where T : class
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            if (array != null)
            {
                foreach (object obj in array)
                {
                    sw.Write(obj);
                }
            }

            return ms.GetBuffer();
        }

        /// <summary>
        /// Validate sheet name as character count MUST be greater than or equal to 1 and less than or equal to 31
        /// </summary>
        /// <param name="sheetName">Sheet name to validate</param>
        /// <returns></returns>
        public static string ValidateName(this string sheetName)
        {
            if (sheetName.Length == 0)
                return "Export";

            if (sheetName.Length > 31)
            {
                return sheetName.Substring(0, 30);
            }

            return sheetName;
        }
        
        public static DataTable ToDataTable<T>(this IEnumerable<T>? data, string name)
        {
            DataTable table = new(name);
            table.TableName = data.FirstOrDefault().GetDisplayNameFromAttribute();

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                table.Columns.Add(property.GetDisplayName(), typeof(string));
            }

            if (data != null)
            {
                foreach (var item in data)
                {
                    var row = table.NewRow();

                    foreach (var prop in properties)
                    {
                        if (prop.PropertyType.Name == "List`1")
                        {
                            var listItem = prop.GetValue(item);
                            var method =
                                typeof(Extender).GetMethod("ListAsString")!.MakeGenericMethod(
                                    prop.PropertyType.GetGenericArguments()[0]);

                            row[prop.GetDisplayName()] = method.Invoke(null, new[]
                            {
                                listItem!,
                                "; "
                            }) as string ?? "";
                        }
                        else if (prop.PropertyType.IsArray && prop.PropertyType.Name != "String")
                        {
                            var listItem = prop.GetValue(item);
                            var method =
                                typeof(Extender).GetMethod("EnumAsString")!.MakeGenericMethod(
                                    listItem!.GetType().GetElementType()!);

                            row[prop.GetDisplayName()] = method.Invoke(null, new[]
                            {
                                listItem!,
                                "; "
                            }) as string ?? "";
                        }
                        else if (prop.Name.ToLower().Contains("id") &&
                                 prop.PropertyType.UnderlyingSystemType.Name.Contains("Int64"))
                        {
                            row[prop.GetDisplayName()] = prop.GetValue(item)?.ToString().Base36String() ?? "";
                        }
                        else if (prop.Name.ToLower().Contains("id") &&
                                 prop.PropertyType.UnderlyingSystemType.GenericTypeArguments.Any(x =>
                                     x.AssemblyQualifiedName?.Contains("Int64") ?? false))
                        {
                            row[prop.GetDisplayName()] = prop.GetValue(item)?.ToString().Base36String() ?? "";
                        }
                        else
                            row[prop.GetDisplayName()] = prop.GetValue(item)?.ToString() ?? "";
                    }

                    table.Rows.Add(row);
                }
            }

            return table;
        }

        public static string GetDisplayName(this PropertyInfo propertyInfo)
        {
            var atts = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            return atts.Length == 0 ? propertyInfo.Name : (atts[0] as DisplayNameAttribute)!.DisplayName;
        }

        public static string GetDisplayName<T>(this T model) where T : class
        {
            //if (model.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).Any())
            //{
            //    var attr = TypeDescriptor.GetAttributes(model.GetType())[2] as DisplayNameAttribute;
            //    return attr!.DisplayName;
            //}

            var attrs = model.GetType().GetCustomAttributes(typeof(CustomDisplayName), true);

            if (attrs.Any())
            {
                return ((CustomDisplayName)attrs.First()).DisplayName;
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

        public static string ListAsString<T>(this List<T>? list, string separator = "; ")
        {
            var stringList = list?.Select(item => item?.ToString() ?? "").ToList() ?? new List<string>();

            return string.Join(separator, stringList);
        }

        public static string EnumAsString<T>(this T[]? list, string separator = "; ")
        {
            var stringList = list?.Select(item => item?.ToString() ?? "").ToList() ?? new List<string>();

            return string.Join(separator, stringList);
        }

        public static string Base36String(this object? value)
        {
            if (value is null) return "";

            _ = long.TryParse((string)value, out long convertedValue);

            return PandaBaseConverter.Base10ToBase36(convertedValue) ?? "";
        }

        public static string GetString(this byte[] data)
        {
            return Encoding.Default.GetString(data);
        }

        public static string GetDisplayNameFromAttribute<T>(this T source)
        {
            return typeof(T).GetCustomAttributes<DisplayNameAttribute>()
                .FirstOrDefault(_ => true)?.DisplayName ?? typeof(T).Name;
        }


        public static TValue? GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }

            return default(TValue);
        }
    }

    public class CustomDisplayName : Attribute
    {
        // Private fields.
        private string displayName;

        // This constructor defines two required parameters: name and level.

        public CustomDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        // Define Name property.
        // This is a read-only attribute.

        public virtual string DisplayName
        {
            get { return displayName; }
        }
    }
}