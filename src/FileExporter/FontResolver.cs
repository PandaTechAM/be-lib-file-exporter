using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExporter
{
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
}
