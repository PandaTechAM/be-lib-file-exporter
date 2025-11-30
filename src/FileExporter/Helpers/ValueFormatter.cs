using System;
using System.Globalization;
using FileExporter.Enums;
using FileExporter.Rules;

namespace FileExporter.Helpers;

internal static class ValueFormatter
{
   public static string FormatForCsv(object? value, IPropertyRule rule, CultureInfo culture)
   {
      if (value == null)
      {
         return rule.DefaultValue ?? string.Empty;
      }

      var type = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

      if (type.IsEnum)
      {
         return FormatEnumCsv(value, rule);
      }

      switch (value)
      {
         case bool b:
            return b ? "Yes" : "No";
         case DateTime dt:
            return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
      }

      if (!IsNumeric(type))
      {
         return Convert.ToString(value, culture) ?? string.Empty;
      }

      var precision = rule.Precision ?? 2;

      switch (value)
      {
         case decimal dec:
         {
            var rounded = Math.Round(dec, precision);
            return rounded.ToString($"F{precision}", culture);
         }
         case double dbl:
         {
            var rounded = Math.Round(dbl, precision);
            return rounded.ToString($"F{precision}", culture);
         }
         case float fl:
         {
            var rounded = MathF.Round(fl, precision);
            return rounded.ToString($"F{precision}", culture);
         }
      }

      return Convert.ToString(value, culture) ?? string.Empty;
   }

   public static object? FormatForXlsx(object? value, IPropertyRule rule)
   {
      if (value == null)
      {
         return rule.DefaultValue;
      }

      var type = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

      if (type.IsEnum)
      {
         return FormatEnumXlsx(value, rule);
      }

      if (!IsNumeric(type))
      {
         return value;
      }

      var precision = rule.Precision ?? 2;

      return value switch
      {
         decimal dec => Math.Round(dec, precision),
         double dbl => Math.Round(dbl, precision),
         float fl => MathF.Round(fl, precision),
         _ => value
      };
   }

   private static string FormatEnumCsv(object value, IPropertyRule rule)
   {
      var intValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);
      var name = Enum.GetName(value.GetType(), value) ?? intValue.ToString(CultureInfo.InvariantCulture);

      return rule.EnumFormat switch
      {
         EnumFormatMode.Int => intValue.ToString(CultureInfo.InvariantCulture),
         EnumFormatMode.Name => name,
         _ => $"{intValue} - {name}"
      };
   }

   private static object FormatEnumXlsx(object value, IPropertyRule rule)
   {
      return rule.EnumFormat switch
      {
         EnumFormatMode.Int => Convert.ToInt64(value, CultureInfo.InvariantCulture),
         EnumFormatMode.Name => Enum.GetName(value.GetType(), value)
                                ?? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                                          .ToString(CultureInfo.InvariantCulture),
         _ => $"{Convert.ToInt64(value, CultureInfo.InvariantCulture)} - {Enum.GetName(value.GetType(), value)}"
      };
   }

   private static bool IsNumeric(Type type)
   {
      return type == typeof(decimal)
             || type == typeof(double)
             || type == typeof(float)
             || type == typeof(int)
             || type == typeof(long)
             || type == typeof(short)
             || type == typeof(byte)
             || type == typeof(uint)
             || type == typeof(ulong)
             || type == typeof(ushort)
             || type == typeof(sbyte);
   }
}