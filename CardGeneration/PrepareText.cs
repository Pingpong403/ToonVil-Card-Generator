using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class PrepareText
	{
		/// <summary>
		/// Converting text to image (png).
		/// </summary>
		/// <param name="text">text to convert</param>
		/// <param name="font">Font to use</param>
		/// <param name="textColor">text color</param>
		/// <param name="maxWidth">max width of the image</param>
		/// <meta>Original code from https://gist.github.com/naveedmurtuza/6600103</meta>
		public static void DrawText(String text, Font font, Color textColor, int maxWidth, String element, bool centerAlign = true)
		{
			// First, if it is a Title, capitalize the text
			if (string.Equals(element.ToLower(), "title"))
			{
				text = text.ToUpper();
			}

			// set the stringformat flags to rtl
			StringFormat sf = new StringFormat();
			// uncomment the next line for right to left languages
			//sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
			sf.Trimming = StringTrimming.Word;
			if (centerAlign)
			{
				sf.Alignment = StringAlignment.Center;
			}

			// create a dummy bitmap just to get a graphics object
			Image img = new Bitmap(1, 1);
			Graphics drawing = Graphics.FromImage(img);
			// measure the string to see how big the image needs to be
			SizeF textSize = drawing.MeasureString(text, font, maxWidth, sf);
			img.Dispose();
			drawing.Dispose();

			// create a new image of the right size with minimal padding
			int width = (int)System.Math.Ceiling(textSize.Width);
			int height = (int)System.Math.Ceiling(textSize.Height);
			img = new Bitmap(width, height);

			drawing = Graphics.FromImage(img);
			// Adjust for high quality
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// paint the background
			drawing.Clear(Color.Transparent);

			// create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			// Draw text with tight bounds (no extra padding rectangle)
			if (centerAlign)
			{
				// For center alignment, use full width
				drawing.DrawString(text, font, textBrush, new RectangleF(3, 8, width, height), sf);
			}
			else
			{
				// For left alignment, draw at (0,0) without bounding rectangle
				drawing.DrawString(text, font, textBrush, new PointF(0, 0));
			}

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var baseDir = AppContext.BaseDirectory;
            var outDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "TextIntermediary"));
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{element}.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
			Console.WriteLine($"Image saved: {outpath}");
		}
	}
}