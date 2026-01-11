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
		public static void SizeIcon()
		{
			// Get image into an object - look for png first, then jpg, then jpeg
			var baseDir = AppContext.BaseDirectory;
            var fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Images", "icon.png"));
			if (!File.Exists(fullPath))
			{
				fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Images", "icon.jpg"));
			}
			if (!File.Exists(fullPath))
			{
				fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Images", "icon.jpeg"));
			}
			// If no image is found, exit
			if (!File.Exists(fullPath))
			{
				return;
			}
			using Image icon = Image.FromFile(fullPath);

			// Calculate new width and height using float math to avoid integer truncation
			float targetWidthHeight = 149;
			float widthRatio = targetWidthHeight / icon.Width;
			float heightRatio = targetWidthHeight / icon.Height;
			int newWidth = Math.Max(1, (int)Math.Round(widthRatio * icon.Width));
			int newHeight = Math.Max(1, (int)Math.Round(heightRatio * icon.Height));

			using Bitmap b = new Bitmap(newWidth, newHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			// Draw image with new width and height
			g.DrawImage(icon, 0, 0, newWidth, newHeight);

			// Ensure output directory exists and save resized PNG
			var imgBaseDir = AppContext.BaseDirectory;
            var outDir = Path.GetFullPath(Path.Combine(imgBaseDir, "..", "..", "..", "ImageIntermediary"));
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "icon.png");
			b.Save(outpath, ImageFormat.Png);
			Console.WriteLine($"Icon resized and saved: {outpath}");
		}

		/// <summary>
		/// Produce a properly sized card image, ready to be inserted onto card, from any size
		/// </summary>
		/// <param name="imageName">The name of the image file, not including extension</param>
		/// <meta>Original code from https://www.c-sharpcorner.com/UploadFile/ishbandhu2009/resize-an-image-in-C-Sharp/</meta>
		public static void SizeCardImage(String imageName)
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
			float targetHeight = float.Parse(ConfigHelper.GetConfigValue("card", "imageAreaHeight"), CultureInfo.InvariantCulture);
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

		/// <summary>
		/// Puts together all the images in the intermediary directories
		/// and saves the result in -Exports.
		/// </summary>
		/// <param name="cardTitle">The title of the card to be combined</param>
		/// <param name="deck">What deck the card belongs to (Villain, Fate, etc.)</param>
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
			var iconPath = Path.Combine(imageIntermediaryPath, "icon.png");
			var bottomRightElementPath = Path.Combine(textIntermediaryPath, "BottomRightElement.png");

			int cardWidth = int.Parse(ConfigHelper.GetConfigValue("card", "w"));
			int cardHeight = int.Parse(ConfigHelper.GetConfigValue("card", "h"));
			using Bitmap b = new Bitmap(cardWidth, cardHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			string capitalizedDeck = MiscHelper.Capitalize(deck);
			// Card image
			bool imageExists = File.Exists(imagePath);
			using (Image cardImg = Image.FromFile(imageExists ? imagePath : altImagePath))
			{
				// get image width
				int imgW = cardImg.Width;
				// set xOffset to appropriate value
				int xOffset = (cardWidth - imgW) / 2;
				g.DrawImage(cardImg, xOffset, 0);
			}
			
			// Deck background
			using (Image bgImg = Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Deck.png")))
			{
				g.DrawImage(bgImg, new Rectangle(0, 0, cardWidth, cardHeight));
			}

			// Title
			using (Image titleImg = Image.FromFile(Path.Combine(textIntermediaryPath, "Title.png")))
			{
				g.DrawImage(titleImg, cardWidth / 2 - titleImg.Width / 2, 1152 - titleImg.Height / 2);
			}
			
			// Ability
			using (Image abilityImg = Image.FromFile(Path.Combine(textIntermediaryPath, "Ability.png")))
			{
				g.DrawImage(abilityImg, cardWidth / 2 - abilityImg.Width / 2, 1541 - abilityImg.Height / 2);
			}
			
			// Strength
			if (File.Exists(strengthPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Strength.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image strImg = Image.FromFile(strengthPath))
				{
					g.DrawImage(strImg, 139 - strImg.Width / 2, cardHeight - 139 - strImg.Height / 2);
				}
			}
			
			// Cost
			if (File.Exists(costPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Cost.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image costImg = Image.FromFile(costPath))
				{
					g.DrawImage(costImg, 187 - costImg.Width / 2, 191 - costImg.Height / 2);
				}
			}

			// Top Right Element
			if (File.Exists(topRightElementPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "TopRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image topRightImg = Image.FromFile(topRightElementPath))
				{
					g.DrawImage(topRightImg, cardWidth - 186 - topRightImg.Width / 2, 195 - topRightImg.Height / 2);
				}
			}

			// Bottom Right Element
			// if exists, use value given. otherwise, use icon.png
			if (File.Exists(bottomRightElementPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "BottomRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image bottomRightImg = Image.FromFile(bottomRightElementPath))
				{
					g.DrawImage(bottomRightImg, cardWidth - 138 - bottomRightImg.Width / 2, cardHeight - 139 - bottomRightImg.Height / 2);
				}
			}
			else if (File.Exists(iconPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "BottomRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image iconImg = Image.FromFile(iconPath))
				{
					g.DrawImage(iconImg, cardWidth - 213, cardHeight - 214);
				}
			}

			// Ensure output directory exists and save completed card
			var baseDir = AppContext.BaseDirectory;
            var outDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "-Exports"));
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{cardTitle}.png");
			b.Save(outpath, ImageFormat.Png);
			Console.WriteLine($"Card saved: {outpath}");
		}

		/// <summary>
		/// Deletes all files in each intermediary directory except for the icon.
		/// </summary>
		public static void CleanIntermediaries()
		{
			var cleanBaseDir = AppContext.BaseDirectory;
			var imageIntermediaryPath = Path.GetFullPath(Path.Combine(cleanBaseDir, "..", "..", "..", "ImageIntermediary"));
			var textIntermediaryPath = Path.GetFullPath(Path.Combine(cleanBaseDir, "..", "..", "..", "TextIntermediary"));

			var imageIntermediaryDI = new DirectoryInfo(imageIntermediaryPath);
			foreach (FileInfo fi in imageIntermediaryDI.EnumerateFiles())
			{
				if (fi.Extension == ".png" && fi.Name != "icon.png") fi.Delete();
			}
			var textIntermediaryDI = new DirectoryInfo(textIntermediaryPath);
			foreach (FileInfo fi in textIntermediaryDI.EnumerateFiles())
			{
				if (fi.Extension == ".png") fi.Delete();
			}
		}

		/// <summary>
		/// Cleans all images in the intermediaries, including the icon.
		/// </summary>
		public static void CleanIntermediariesFinal()
		{
			var cleanBaseDir = AppContext.BaseDirectory;
			var imageIntermediaryPath = Path.GetFullPath(Path.Combine(cleanBaseDir, "..", "..", "..", "ImageIntermediary"));

			var imageIntermediaryDI = new DirectoryInfo(imageIntermediaryPath);
			foreach (FileInfo fi in imageIntermediaryDI.EnumerateFiles())
			{
				if (fi.Extension != ".txt") fi.Delete();
			}
		}
	}
}