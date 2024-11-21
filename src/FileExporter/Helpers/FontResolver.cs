using System;
using System.IO;
using PdfSharpCore.Fonts;

namespace FileExporter.Helpers;

internal class FontResolver : IFontResolver
{
   public string DefaultFontName => Constants.DefaultFontName;

   public byte[] GetFont(string faceName)
   {
      var fontFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", faceName);

      if (!File.Exists(fontFilePath))
      {
         throw new InvalidOperationException($"Font '{fontFilePath}' not found.");
      }

      // Read font file as byte array
      return File.ReadAllBytes(fontFilePath);
   }

   public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
   {
      var fontsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts");

      var fontFileName = isBold ? $"{familyName}bd.ttf" : isItalic ? $"{familyName}i.ttf" : $"{familyName}.ttf";

      var fontFilePath = Path.Combine(fontsPath, fontFileName);

      if (!File.Exists(fontFilePath))
      {
         throw new InvalidOperationException($"Font '{fontFilePath}' not found.");
      }

      return new FontResolverInfo(fontFileName, isBold, isItalic);
   }
}