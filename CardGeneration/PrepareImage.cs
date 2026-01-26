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
			var relativePath = Path.Combine("Card Data", "-Images", "icon.png");
            var fullPath = PathHelper.GetFullPath(relativePath);
			if (!File.Exists(fullPath))
			{
				relativePath = Path.Combine("Card Data", "-Images", "icon.jpg");
				fullPath = PathHelper.GetFullPath(relativePath);
			}
			if (!File.Exists(fullPath))
			{
				relativePath = Path.Combine("Card Data", "-Images", "icon.jpeg");
				fullPath = PathHelper.GetFullPath(relativePath);
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

			using Bitmap b = new(newWidth, newHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			// Draw image with new width and height
			g.DrawImage(icon, 0, 0, newWidth, newHeight);

			// Ensure output directory exists and save resized PNG
			var relativeOutDir = Path.Combine("temp", "ImageIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "icon.png");
			b.Save(outpath, ImageFormat.Png);
		}

		/// <summary>
		/// Produce a properly sized card image, ready to be inserted onto card, from any size
		/// </summary>
		/// <param name="imageName">The name of the image file, not including extension</param>
		/// <meta>Original code from https://www.c-sharpcorner.com/UploadFile/ishbandhu2009/resize-an-image-in-C-Sharp/</meta>
		public static void SizeCardImage(String imageName)
		{
			// Get image into an object - look for png first, then jpg, then jpeg
			var relativePath = Path.Combine("Card Data", "-Images", imageName + ".png");
            var fullPath = PathHelper.GetFullPath(relativePath);
			if (!File.Exists(fullPath))
			{
				relativePath = Path.Combine("Card Data", "-Images", imageName + ".jpg");
                fullPath = PathHelper.GetFullPath(relativePath);
			}
			if (!File.Exists(fullPath))
			{
				relativePath = Path.Combine("Card Data", "-Images", imageName + ".jpeg");
                fullPath = PathHelper.GetFullPath(relativePath);
			}
			// If no image is found, continue on with black background
			if (!File.Exists(fullPath))
			{
				relativePath = Path.Combine("assets", "black_bg.png");
				fullPath = PathHelper.GetFullPath(relativePath);
			}
			using Image img = Image.FromFile(fullPath);

			float targetHeight = float.Parse(ConfigHelper.GetConfigValue("card", "imageAreaHeight"), CultureInfo.InvariantCulture);
			float ratio = targetHeight / img.Height;
			int newWidth = Math.Max(1, (int)Math.Round(ratio * img.Width));
			int newHeight = Math.Max(1, (int)Math.Round(ratio * img.Height));

			using Bitmap b = new(newWidth, newHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			// Draw image with new width and height
			g.DrawImage(img, 0, 0, newWidth, newHeight);

			// Ensure output directory exists and save resized PNG
			var relativeOutDir = Path.Combine("temp", "ImageIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{imageName}.png");
			b.Save(outpath, ImageFormat.Png);
		}

		/// <summary>
		/// Puts together all the images in the intermediary directories
		/// and saves the result in -Exports.
		/// </summary>
		/// <param name="cardTitle">The title of the card to be combined</param>
		/// <param name="deck">What deck the card belongs to (Villain, Fate, etc.)</param>
		public static void CombineImages(string cardTitle, string deck)
		{
			var imageIntermediaryPath = PathHelper.GetFullPath(Path.Combine("temp", "ImageIntermediary"));
			var textIntermediaryPath = PathHelper.GetFullPath(Path.Combine("temp", "TextIntermediary"));
			var layoutPath = PathHelper.GetFullPath(Path.Combine("Card Data", "-Layout"));
			var assetsPath = PathHelper.GetFullPath("assets");

			// Possible necessary elements: image, Title, Ability, Type,
			// Cost, Strength, TopRight, BottomRight
			var imagePath = Path.Combine(imageIntermediaryPath, cardTitle + ".png");
			var altImagePath = Path.Combine(assetsPath, "black_bg.png");
			var titlePath = Path.Combine(textIntermediaryPath, "Title.png");
			var abilityPath = Path.Combine(textIntermediaryPath, "Ability.png");
			var typePath = Path.Combine(textIntermediaryPath, "Type.png");
			var costPath = Path.Combine(textIntermediaryPath, "Cost.png");
			var strengthPath = Path.Combine(textIntermediaryPath, "Strength.png");
			var topRightElementPath = Path.Combine(textIntermediaryPath, "TopRight.png");
			var iconPath = Path.Combine(imageIntermediaryPath, "icon.png");
			var bottomRightElementPath = Path.Combine(textIntermediaryPath, "BottomRight.png");

			int cardWidth = int.Parse(ConfigHelper.GetConfigValue("card", "w"));
			int cardHeight = int.Parse(ConfigHelper.GetConfigValue("card", "h"));
			using Bitmap b = new(cardWidth, cardHeight);
			using Graphics g = Graphics.FromImage(b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.CompositingQuality = CompositingQuality.HighQuality;

			// Only draw any elements if all of the necessary elements are present
			// Necessary elements: Title, Ability, Type
			if (!(File.Exists(titlePath) && File.Exists(abilityPath) && File.Exists(typePath)))
			{
				Console.WriteLine($"Missing elements for {cardTitle}!");
				return;
			}

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
			using (Image titleImg = Image.FromFile(titlePath))
			{
				g.DrawImage(titleImg, (cardWidth - titleImg.Width) / 2, 1152 - titleImg.Height / 2);
			}
			
			// Ability
			using (Image abilityImg = Image.FromFile(abilityPath))
			{
				g.DrawImage(abilityImg, (cardWidth - abilityImg.Width) / 2, 1618 - abilityImg.Height / 2);
			}

			// Type
			using (Image typeImg = Image.FromFile(typePath))
			{
				g.DrawImage(typeImg, (cardWidth - typeImg.Width) / 2, 1986 - typeImg.Height / 2);
			}
			
			// Cost
			if (File.Exists(costPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Cost.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image costImg = Image.FromFile(costPath))
				{
					int costX = int.Parse(ConfigHelper.GetConfigValue("layout", "costCenterX")) - costImg.Width / 2;
					int costY = int.Parse(ConfigHelper.GetConfigValue("layout", "costCenterY")) - costImg.Height / 2;
					g.DrawImage(costImg, costX, costY);
				}
			}
			
			// Strength
			if (File.Exists(strengthPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Strength.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image strImg = Image.FromFile(strengthPath))
				{
					int strX = int.Parse(ConfigHelper.GetConfigValue("layout", "strengthCenterX")) - strImg.Width / 2;
					int strY = cardHeight - int.Parse(ConfigHelper.GetConfigValue("layout", "strengthCenterY")) - strImg.Height / 2;
					g.DrawImage(strImg, strX, strY);
				}
			}

			// Top Right Element
			if (File.Exists(topRightElementPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "TopRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image topRightImg = Image.FromFile(topRightElementPath))
				{
					int topRightX = cardWidth - int.Parse(ConfigHelper.GetConfigValue("layout", "topRightCenterX")) - topRightImg.Width / 2;
					int topRightY = int.Parse(ConfigHelper.GetConfigValue("layout", "topRightCenterY")) - topRightImg.Height / 2;
					g.DrawImage(topRightImg, topRightX, topRightY);
				}
			}

			// Bottom Right Element
			// ToonVil: if exists, use value given. otherwise, use icon.png
			if (File.Exists(bottomRightElementPath))
			{
				g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "BottomRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
				using (Image bottomRightImg = Image.FromFile(bottomRightElementPath))
				{
					int bottomRightX = cardWidth - int.Parse(ConfigHelper.GetConfigValue("layout", "bottomRightCenterX")) - bottomRightImg.Width / 2;
					int bottomRightY = cardHeight - int.Parse(ConfigHelper.GetConfigValue("layout", "bottomRightCenterY")) - bottomRightImg.Height / 2;
					g.DrawImage(bottomRightImg, bottomRightX, bottomRightY);
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
			var relativeOutDir = Path.Combine("Card Data", "-Exports");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{cardTitle}.png");
			b.Save(outpath, ImageFormat.Png);
			Console.WriteLine($"Image saved: {cardTitle}");
		}

		/// <summary>
		/// Deletes all files in each intermediary directory except for the icon.
		/// </summary>
		public static void CleanIntermediaries()
		{
			var imageIntermediaryPath = PathHelper.GetFullPath(Path.Combine("temp", "ImageIntermediary"));
			var textIntermediaryPath = PathHelper.GetFullPath(Path.Combine("temp", "TextIntermediary"));

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
		/// Cleans all images in the image intermediary, including the icon.
		/// </summary>
		public static void CleanImageIntermediaryFinal()
		{
			var relativePath = Path.Combine("temp", "ImageIntermediary");
			var imageIntermediaryPath = PathHelper.GetFullPath(relativePath);

			var imageIntermediaryDI = new DirectoryInfo(imageIntermediaryPath);
			foreach (FileInfo fi in imageIntermediaryDI.EnumerateFiles())
			{
				if (fi.Extension != ".txt") fi.Delete();
			}
		}
	}
}