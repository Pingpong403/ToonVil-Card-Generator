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
			string capitalizedDeck = MiscHelper.Capitalize(deck.ToLower());
			if (!MiscHelper.DeckExists(capitalizedDeck))
			{
				Console.WriteLine("Card template missing for " + capitalizedDeck + " deck!");
			}
			else
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
					Point titleCenter = MiscHelper.GetElementPos("title");
					g.DrawImage(titleImg, titleCenter.X - titleImg.Width / 2, titleCenter.Y - titleImg.Height / 2);
				}
				
				// Ability
				using (Image abilityImg = Image.FromFile(abilityPath))
				{
					Point abilityCenter = MiscHelper.GetElementPos("ability");
					g.DrawImage(abilityImg, abilityCenter.X - abilityImg.Width / 2, abilityCenter.Y - abilityImg.Height / 2);
				}

				// Type
				using (Image typeImg = Image.FromFile(typePath))
				{
					Point typeCenter = MiscHelper.GetElementPos("type");
					g.DrawImage(typeImg, typeCenter.X - typeImg.Width / 2, typeCenter.Y - typeImg.Height / 2);
				}
				
				// Cost
				if (File.Exists(costPath))
				{
					g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Cost.png")), new Rectangle(0, 0, cardWidth, cardHeight));
					using (Image costImg = Image.FromFile(costPath))
					{
						Point costCenter = MiscHelper.GetElementPos("cost");
						g.DrawImage(costImg, costCenter.X - costImg.Width / 2, costCenter.Y - costImg.Height / 2);
					}
				}
				
				// Strength
				if (File.Exists(strengthPath))
				{
					g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "Strength.png")), new Rectangle(0, 0, cardWidth, cardHeight));
					using (Image strengthImg = Image.FromFile(strengthPath))
					{
						Point strengthCenter = MiscHelper.GetElementPos("strength");
						g.DrawImage(strengthImg, strengthCenter.X - strengthImg.Width / 2, strengthCenter.Y - strengthImg.Height / 2);
					}
				}

				// Top Right Element
				if (File.Exists(topRightElementPath))
				{
					g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "TopRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
					using (Image topRightImg = Image.FromFile(topRightElementPath))
					{
						Point topRightCenter = MiscHelper.GetElementPos("topRight");
						g.DrawImage(topRightImg, topRightCenter.X - topRightImg.Width / 2, topRightCenter.Y - topRightImg.Height / 2);
					}
				}

				// Bottom Right Element
				if (File.Exists(bottomRightElementPath))
				{
					g.DrawImage(Image.FromFile(Path.Combine(layoutPath, capitalizedDeck + "BottomRight.png")), new Rectangle(0, 0, cardWidth, cardHeight));
					using (Image bottomRightImg = Image.FromFile(bottomRightElementPath))
					{
						Point bottomRightCenter = MiscHelper.GetElementPos("bottomRight");
						g.DrawImage(bottomRightImg, bottomRightCenter.X - bottomRightImg.Width / 2, bottomRightCenter.Y - bottomRightImg.Height / 2);
					}
				}

				// Ensure output directory exists and save completed card
				bool saveByDeck = SettingsHelper.GetSettingsValue("Data", "exportByDeck") == "true";
				var relativeOutDir = Path.Combine("Card Data", "-Exports", saveByDeck ? deck : "");
				var outDir = PathHelper.GetFullPath(relativeOutDir);
				Directory.CreateDirectory(outDir);
				var outpath = Path.Combine(outDir, $"{cardTitle}.png");
				b.Save(outpath, ImageFormat.Png);
				Console.WriteLine($"Image saved: {cardTitle}");
			}
		}

		/// <summary>
		/// Deletes all files in each intermediary directory.
		/// </summary>
		public static void CleanIntermediaries()
		{
			var imageIntermediaryPath = PathHelper.GetFullPath(Path.Combine("temp", "ImageIntermediary"));
			var textIntermediaryPath = PathHelper.GetFullPath(Path.Combine("temp", "TextIntermediary"));

			var imageIntermediaryDI = new DirectoryInfo(imageIntermediaryPath);
			foreach (FileInfo fi in imageIntermediaryDI.EnumerateFiles())
			{
				if (fi.Extension == ".png") fi.Delete();
			}
			var textIntermediaryDI = new DirectoryInfo(textIntermediaryPath);
			foreach (FileInfo fi in textIntermediaryDI.EnumerateFiles())
			{
				if (fi.Extension == ".png") fi.Delete();
			}
		}
	}
}