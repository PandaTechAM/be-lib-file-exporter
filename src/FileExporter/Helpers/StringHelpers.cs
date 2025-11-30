using System.Linq;

namespace FileExporter.Helpers;

internal static class StringHelpers
{
   private static readonly char[] InvalidFileNameChars = "\0\u0003:\\/?*[]".ToCharArray();

   public static string ToValidName(this string name, int maxLength)
   {
      var validName = InvalidFileNameChars.Aggregate(name, (current, invalidChar) => current.Replace(invalidChar, '_'));

      if (validName.Length > maxLength)
      {
         validName = validName[..maxLength];
      }

      return validName;
   }

   public static string ToSpacedName(this string name)
   {
      if (string.IsNullOrWhiteSpace(name))
      {
         return name;
      }

      var result = new System.Text.StringBuilder(name.Length * 2);
      result.Append(name[0]);

      for (var i = 1; i < name.Length; i++)
      {
         var c = name[i];
         var prev = name[i - 1];

         if (char.IsUpper(c) && !char.IsWhiteSpace(prev) && prev != '_')
         {
            result.Append(' ');
         }

         result.Append(c == '_' ? ' ' : c);
      }

      return result.ToString();
   }
}