using System;
using System.Globalization;

namespace FileExporter.Helpers;

internal static class NamingHelper
{
   private const string DateTimePlaceholder = "{DateTime}";
   private const int MaxNameLength = 30;

   internal static string GetDisplayName<T>()
   {
      var type = typeof(T);
      var displayName = type.Name + " " + DateTimePlaceholder;
      var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
      displayName = displayName.Replace(DateTimePlaceholder, now);
      return displayName.ToValidName(MaxNameLength);
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