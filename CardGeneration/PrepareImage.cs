using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class PrepareImage
	{
		/// <summary>
		/// Produce a properly sized image, ready to be inserted onto card, from any size
		/// </summary>
		/// <param name="imageName">The name of the image file, not including extension</param>
		public static void SizeImage(String imageName)
		{
			// Get image into an object - look for png first, then jpg, then jpeg
			var baseDir = AppContext.BaseDirectory;
            var fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Images", imageName + ".png"));
			if (!File.Exists(fullPath))
			{
				fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Images", imageName + ".jpg"));
			}
			if (!File.Exists(fullPath))
			{
				fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Images", imageName + ".jpeg"));
			}
			// If no image is found, continue on with black background
			if (!File.Exists(fullPath))
			{
				fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "assets", "black_bg.png"));
			}
			using Image img = Image.FromFile(fullPath);

			// Calculate new width and height using float math to avoid integer truncation
			float targetHeight = float.Parse(ConfigHelper.GetConfigValue("card-config.txt", "imageAreaHeight"), CultureInfo.InvariantCulture);
			float ratio = targetHeight / img.Height;
			int newWidth = Math.Max(1, (int)Math.Round(ratio * img.Width));
			int newHeight = Math.Max(1, (int)Math.Round(ratio * img.Height));

			using Bitmap b = new Bitmap(newWidth, newHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			// Draw image with new width and height
			g.DrawImage(img, 0, 0, newWidth, newHeight);

			// Ensure output directory exists and save resized PNG
			var imgBaseDir = AppContext.BaseDirectory;
            var outDir = Path.GetFullPath(Path.Combine(imgBaseDir, "..", "..", "..", "ImageIntermediary"));
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{imageName}.png");
			b.Save(outpath, ImageFormat.Png);
			Console.WriteLine($"Image resized and saved: {outpath}");
		}
	}
}