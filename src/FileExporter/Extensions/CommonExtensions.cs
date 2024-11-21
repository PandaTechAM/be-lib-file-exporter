using System;
using FileExporter.Helpers;

namespace FileExporter.Extensions;

public static class CommonExtensions
{
   public static string ToValidName(this string name)
   {
      var invalidChars = "\0\u0003:\\/?*[]".ToCharArray();

      var validName = name;
      foreach (var invalidChar in invalidChars)
      {
         validName = validName.Replace(invalidChar, '_');
      }

      return validName[..Math.Min(validName.Length, Constants.NameLength)];
   }
}