using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class PrepareImage
	{
		/// <summary>
		/// Produce a properly sized image, ready to be inserted onto card, from any size
		/// </summary>
		/// <param name="imageName">The name of the image file, not including extension</param>
		/// <meta>Original code from https://www.c-sharpcorner.com/UploadFile/ishbandhu2009/resize-an-image-in-C-Sharp/</meta>
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

		public static void CombineImages(string cardTitle, string deck)
		{
			var combineBaseDir = AppContext.BaseDirectory;
			var imageIntermediaryPath = Path.GetFullPath(Path.Combine(combineBaseDir, "..", "..", "..", "ImageIntermediary"));
			var textIntermediaryPath = Path.GetFullPath(Path.Combine(combineBaseDir, "..", "..", "..", "TextIntermediary"));
			var layoutPath = Path.GetFullPath(Path.Combine(combineBaseDir, "..", "..", "..", "-Layout"));
			var assetsPath = Path.GetFullPath(Path.Combine(combineBaseDir, "..", "..", "..", "assets"));

			// Possible necessary elements: image, Title, Ability, Type,
			// Cost, Strength, TopRightElement, BottomRightElement
			var imagePath = Path.Combine(imageIntermediaryPath, cardTitle + ".png");
			var altImagePath = Path.Combine(assetsPath, "black_bg.png");
			var titlePath = Path.Combine(textIntermediaryPath, "Title.png");
			var abilityPath = Path.Combine(textIntermediaryPath, "Ability.png");
			var typePath = Path.Combine(textIntermediaryPath, "Type.png");
			var costPath = Path.Combine(textIntermediaryPath, "Cost.png");
			var strengthPath = Path.Combine(textIntermediaryPath, "Strength.png");
			var topRightElementPath = Path.Combine(textIntermediaryPath, "TopRightElement.png");
			var BottomRightElementPath = Path.Combine(textIntermediaryPath, "BottomRightElement.png");

			int cardWidth = int.Parse(ConfigHelper.GetConfigValue("card-config.txt", "w"));
			int cardHeight = int.Parse(ConfigHelper.GetConfigValue("card-config.txt", "h"));
			using Bitmap b = new Bitmap(cardWidth, cardHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			string capitalizedDeck = MiscHelper.Capitalize(deck);
			g.DrawImage(Image.FromFile(File.Exists(imagePath) ? imagePath : altImagePath), 0, 0);
			g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Deck.png")), 0, 0);

			// Ensure output directory exists and save completed card
			var baseDir = AppContext.BaseDirectory;
            var outDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Exports"));
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{cardTitle}.png");
			b.Save(outpath, ImageFormat.Png);
			Console.WriteLine($"Card saved: {outpath}");
		}
	}
}