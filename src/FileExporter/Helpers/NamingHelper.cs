using System;
using System.Globalization;
using System.Text;

namespace FileExporter.Helpers;

internal static class NamingHelper
{
   private const string DateTimePlaceholder = "{DateTime}";
   private const int MaxNameLength = 30;

   internal static string GetDisplayName<T>()
   {
      var type = typeof(T);
      var typeName = ToDisplayTitle(type.Name);
      return BuildDisplayName(typeName);
   }

   internal static string BuildDisplayName(string baseName)
   {
      if (string.IsNullOrWhiteSpace(baseName))
      {
         throw new ArgumentException("Base name can not be null or empty.", nameof(baseName));
      }


      var name = ToDisplayTitle(baseName);

      if (!name.Contains(DateTimePlaceholder, StringComparison.Ordinal))
      {
         name = $"{name} {DateTimePlaceholder}";
      }

      var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
      name = name.Replace(DateTimePlaceholder, now, StringComparison.Ordinal);

      return name.ToValidName(MaxNameLength);
   }

   internal static string ToDisplayTitle(string? identifier)
   {
      if (string.IsNullOrWhiteSpace(identifier))
      {
         return string.Empty;
      }

      var s = identifier.Trim();
      var sb = new StringBuilder(s.Length + 4);

      for (var i = 0; i < s.Length; i++)
      {
         var c = s[i];
         var prev = i > 0 ? s[i - 1] : '\0';
         var next = i < s.Length - 1 ? s[i + 1] : '\0';

         var isUpper = char.IsUpper(c);
         var prevIsUpper = char.IsUpper(prev);
         var prevIsLetterOrDigit = char.IsLetterOrDigit(prev);


         var shouldInsertSpace =
            i > 0 &&
            (
               (!prevIsUpper && isUpper && prevIsLetterOrDigit) ||
               (char.IsDigit(prev) && char.IsLetter(c)) ||
               (prevIsUpper && isUpper && next != '\0' && char.IsLower(next))
            );

         if (shouldInsertSpace)
         {
            sb.Append(' ');
         }

         sb.Append(c);
      }

      return sb.ToString();
   }

   internal static string EnsureExtension(string name, string extension)
   {
      if (string.IsNullOrWhiteSpace(extension))
      {
         return name;
      }

      return name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
         ? name
         : name + extension;
   }
}