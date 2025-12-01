using System;

namespace FileExporter.Helpers;

internal static class ColumnWidthDefaults
{
   // Global bounds
   public const double MinWidth = 8.0;
   public const double MaxWidth = 40.0;

   // Heuristics
   public const double TextPadding = 2.0;

   // Type-specific defaults
   public const double DateTimeWidth = 19;
   public const double DateWidth = 12.0;
   public const double BooleanWidth = 8.0;
   public const double IntegerWidth = 12.0;

   // Decimal: base width + precision * factor
   public const double DecimalBaseWidth = 12.0;
   public const double DecimalPerDigit = 0.5;

   public static double FromHeader(string? header)
   {
      var headerLength = string.IsNullOrEmpty(header) ? 10 : header.Length;
      var width = headerLength + TextPadding;
      return Math.Clamp(width, MinWidth, MaxWidth);
   }

   public static double ForDecimal(int? precision)
   {
      var p = precision.GetValueOrDefault(2);
      var width = DecimalBaseWidth + p * DecimalPerDigit;
      return Math.Clamp(width, MinWidth, MaxWidth);
   }
}