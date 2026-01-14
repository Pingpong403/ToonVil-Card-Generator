using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace ToonVil_Card_Generator.CardGeneration
{
    public static class FontLoader
    {
        // Cache font collections so fonts remain valid for the app lifetime
        private static readonly Dictionary<string, PrivateFontCollection> s_collections = new();

        // Loads an OTF/TTF from the project fonts folder and creates a Font
        public static Font GetFont(string fontFileName, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Pixel)
        {
            var relativePath = Path.Combine("fonts", fontFileName);
            var fontPath = PathHelper.GetFullPath(relativePath);

            if (!File.Exists(fontPath))
            {
                // Fallback to current working dir (e.g., when running with dotnet run from project root)
                fontPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "fonts", fontFileName));
            }

            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException($"Font file not found: {fontPath}");
            }

            if (!s_collections.TryGetValue(fontPath, out var collection))
            {
                collection = new PrivateFontCollection();
                collection.AddFontFile(fontPath);
                s_collections[fontPath] = collection;
            }

            var families = collection.Families;
            if (families.Length == 0)
            {
                throw new InvalidOperationException($"No font families loaded from: {fontPath}");
            }

            var family = families[0];
            return new Font(family, size, style, unit);
        }
    }
}
