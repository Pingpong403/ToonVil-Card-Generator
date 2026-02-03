using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Reflection.Metadata;
using System.Diagnostics.Tracing;
using System.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class PrepareText
	{
		/// <summary>
		/// Creates an image in TextIntermediary that contains the title.
		/// </summary>
		/// <param name="text">title text</param>
		/// <param name="font">font to draw the title in</param>
		/// <param name="textColor">color to draw the title in</param>
		/// <param name="maxWidth">maximum width the title is allowed to take up</param>
		/// <param name="maxHeight">maximum height the title is allowed to take up</param>
		public static void DrawTitle(string text, Font font, Color textColor, int maxWidth, int maxHeight)
		{
			// For titles, capitalize the text
			text = text.ToUpper();

			// Set the stringformat flags for center alignment and no trimming
			StringFormat sf = StringFormat.GenericTypographic;
			sf.Trimming = StringTrimming.None;
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// Create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			// One line maximum
			float textHeight = (float)Math.Ceiling(drawing.MeasureString(text, font, 100000, sf).Height);

			// Find the proper squish ratio for given title
			float textFullWidth = (float)drawing.MeasureString(text, font, 100000, sf).Width;
			if (textFullWidth > maxWidth)
			{
				float horizontalSquish = (float)maxWidth / textFullWidth;
				drawing.ScaleTransform(horizontalSquish, 1.0F);
				maxWidth = (int)textFullWidth;
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textBrush, font, null);

			// Set up variables
			float startY = (maxHeight - textHeight) / 2;

			// Draw title word by word
			float currentX = (maxWidth - textFullWidth) / 2;
			foreach (CardWord word in words)
			{
				float wordWidth = word.GetSizeF(drawing, maxWidth, sf).Width;
				float wordHeight = word.GetSizeF(drawing, maxWidth, sf).Height;
				drawing.DrawString(word.GetText(), word.GetTextFont(), word.GetTextBrush(), new RectangleF(currentX, startY, wordWidth, wordHeight), sf);
				currentX += wordWidth;
			}

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "Title.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Creates an image in TextIntermediary that contains all of the text/assets needed for an ability.
		/// </summary>
		/// <param name="ability">ability of the card</param>
		/// <param name="activateAbility">ability on activate of the card</param>
		/// <param name="activateCost">cost to activate the card</param>
		/// <param name="gainsAction">symbol for the action gained</param>
		/// <param name="font">font to draw the text with</param>
		/// <param name="textColor">base color to draw the text with</param>
		/// <param name="maxWidth">maximum width this text can take</param>
		/// <param name="maxHeight">maximum height this text can take</param>
		/// <param name="keywordsAndColors">a dictionary to compare every word to to determine bolding and coloring</param>
		public static void DrawAbility(string ability, string activateAbility, string activateCost, string gainsAction, Font font, Color textColor, int maxWidth, int maxHeight, Dictionary<string, string> keywordsAndColors)
		{
			// Set up variables we'll potentially need
			float dividingLineLines = float.Parse(ConfigHelper.GetConfigValue("asset", "dividingLineLines"));
			float actionSymbolLines = float.Parse(ConfigHelper.GetConfigValue("asset", "actionSymbolLines"));
			float lineSpacing = float.Parse(ConfigHelper.GetConfigValue("text", "lineSpacingFactor"));

			// Set the stringformat flags for center alignment and no trimming
			StringFormat sf = StringFormat.GenericTypographic;
			sf.Trimming = StringTrimming.None;
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// Create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			// Naively find the maximum allowable font size
			float lineHeight;
			float abilityHeight;
			float activateAbilityHeight;
			float gainsActionHeight;
			float textHeight;
			float granularity = 0.5F;
			do
			{
				// Combine every given ability into one metric
				lineHeight = drawing.MeasureString("Tq", font, maxWidth, sf).Height * lineSpacing;
				abilityHeight = ability == "" ? 0 : MeasureWordByWord(GetCardWords(ability, textBrush, font, keywordsAndColors), drawing, sf, maxWidth, lineHeight);
				activateAbilityHeight = 0;
				if (activateAbility != "")
				{
					if (ability == "" || activateCost != "") // If there is no ability or there is an activate cost, measure normally
					{
						activateAbilityHeight += actionSymbolLines * lineHeight + MeasureWordByWord(GetCardWords(activateAbility, textBrush, font, keywordsAndColors), drawing, sf, maxWidth, lineHeight);
					}
					else
					{
						activateAbilityHeight += Math.Max(actionSymbolLines * lineHeight, MeasureWordByWord(GetCardWords(activateAbility, textBrush, font, keywordsAndColors), drawing, sf, maxWidth, lineHeight));
					}
				}
				gainsActionHeight = 0;
				if (gainsAction != "")
				{
					gainsActionHeight = actionSymbolLines * lineHeight;
					if (activateAbility != "" && ability != "" && activateCost == "") // Special case where we need to draw extra text
					{
						gainsActionHeight += lineHeight;
					}
				}
				textHeight = abilityHeight + activateAbilityHeight + gainsActionHeight;

				if (textHeight > maxHeight) font = new Font(font.Name, font.SizeInPoints - granularity, font.Style);
			} while (textHeight > maxHeight);

			// Drawing variables
			float currentY = (maxHeight - textHeight) / 2;
			List<CardWord> words;

			// Draw the ability first
			if (ability != "")
			{
				words = GetCardWords(ability, textBrush, font, keywordsAndColors);

				currentY = DrawWordByWord(words, drawing, font, sf, maxWidth, lineHeight, currentY);
			}

			// Then draw the activate symbol, cost, and ability
			if (activateAbility != "" || activateCost != "")
			{
				// Symbol
				string activateSymbolPath = PathHelper.GetFullPath(Path.Combine("assets", "Activate.png"));
				Image activateSymbol = Image.FromFile(activateSymbolPath);
				float resizing = actionSymbolLines * lineHeight / activateSymbol.Height;
				float centerX = maxWidth / 2;
				if (activateCost != "")
				{
					centerX -= 250;
				}
				DrawSymbol(activateSymbol, drawing, centerX, currentY + lineHeight * 1.5F, resizing);
				
				// Cost, if any
				if (activateCost != "")
				{
					words = GetCardWords(activateCost, textBrush, font, keywordsAndColors);
					float xOffset = 300F;
					float activateCostWidth = maxWidth / 2;
					float activateCostHeight = drawing.MeasureString(GetCleanText(activateCost), font, (int)activateCostWidth, sf).Height;
					float activateCostY = currentY + (3 * lineHeight - activateCostHeight) / 2; // maximum of 3 lines for clarity
					DrawWordByWord(words, drawing, font, sf, activateCostWidth, lineHeight, activateCostY, xOffset);
				}
				currentY += actionSymbolLines * lineHeight;

				// Ability, if any
				if (activateAbility != "")
				{
					words = GetCardWords(activateAbility, textBrush, font, keywordsAndColors);
					currentY = DrawWordByWord(words, drawing, font, sf, maxWidth, lineHeight, currentY);
				}
			}

			// Finally, draw the gained action
			if (gainsAction != "" && AssetHelper.AssetExists(gainsAction, true))
			{
				string assetName = AssetHelper.GetAssetName(gainsAction, true);
				string gainPowerAmt = AssetHelper.GainPowerAmount(assetName);
				if (gainPowerAmt != "")
				{
					assetName = "GainPower";
				}
				string gainsSymbolPath = PathHelper.GetFullPath(Path.Combine("assets", assetName + ".png"));
				Image gainsSymbol = Image.FromFile(gainsSymbolPath);
				float resizing = actionSymbolLines * lineHeight / gainsSymbol.Height;
				DrawSymbol(gainsSymbol, drawing, maxWidth / 2, currentY + actionSymbolLines * lineHeight / 2, resizing);

				// If this was a Gain Power action, draw the amount to be gained
				if (gainPowerAmt != "")
				{
					Font gainPowerFont = FontLoader.GetFont(
						ConfigHelper.GetConfigValue("text", "elementFont"),
						float.Parse(ConfigHelper.GetConfigValue("text", "costFontSize")) * resizing
					);
					float textW = drawing.MeasureString(gainPowerAmt, gainPowerFont, 100000, sf).Width;
					float textH = drawing.MeasureString(gainPowerAmt, gainPowerFont, 100000, sf).Height;
					PointF gainPowerPos = new(
						maxWidth / 2,
						currentY + actionSymbolLines * lineHeight / 2
					);
					drawing.DrawString(gainPowerAmt, gainPowerFont, textBrush, gainPowerPos, sf);
				}
			}

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "Ability.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Creates an image in TextIntermediary that contains the specified type.
		/// </summary>
		/// <param name="text">text to be drawn</param>
		/// <param name="font">font to draw the text in</param>
		/// <param name="textColor">color to draw the text in</param>
		/// <param name="maxWidth">maximum width the text is allowed to take up</param>
		/// <param name="maxHeight">maximum height the text is allowed to take up</param>
		/// <param name="keywordsAndColors">a dictionary containing all the possible keywords and their colors</param>
		public static void DrawType(string text, Font font, Color textColor, int maxWidth, int maxHeight, Dictionary<string, string> keywordsAndColors)
		{
			// For types, capitalize the text
			text = text.ToUpper();

			// Set the stringformat flags for center alignment and no trimming
			StringFormat sf = StringFormat.GenericTypographic;
			sf.Trimming = StringTrimming.None;
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// Create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			// One line maximum
			int textHeight = (int)Math.Ceiling(drawing.MeasureString(text, font, 100000, sf).Height);

			// Find the proper squish ratio for given title
			int textFullWidth = (int)drawing.MeasureString(text, font, 100000, sf).Width;
			if (textFullWidth > maxWidth)
			{
				float horizontalSquish = (float)maxWidth / textFullWidth;
				drawing.ScaleTransform(horizontalSquish, 1.0F);
				maxWidth = textFullWidth;
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textBrush, font, keywordsAndColors, true);

			// Set up variables
			int startY = (maxHeight - textHeight) / 2;

			// Draw title word by word
			int currentX = (maxWidth - textFullWidth) / 2;
			foreach (CardWord word in words)
			{
				int wordWidth = (int)Math.Ceiling(word.GetSizeF(drawing, maxWidth, sf).Width);
				int wordHeight = (int)Math.Ceiling(word.GetSizeF(drawing, maxWidth, sf).Height);
				drawing.DrawString(word.GetText(), word.GetTextFont(), word.GetTextBrush(), new RectangleF(currentX, startY, wordWidth, wordHeight), sf);
				currentX += wordWidth;
			}

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "Type.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Creates an image in TextIntermediary that contains the specified corner element.
		/// </summary>
		/// <param name="text">text to include in the element</param>
		/// <param name="font">font to draw the text with</param>
		/// <param name="textColor">color to draw the text in</param>
		/// <param name="element">which corner element this is (e.g. "Cost", "Strength", "TopRight", or "BottomRight")</param>
		/// <param name="maxWidth">maximum width this element can take up</param>
		/// <param name="maxHeight">maximum height this element can take up</param>
		public static void DrawCornerElement(string text, Font font, Color textColor, string element, int maxWidth, int maxHeight)
		{
			// Set the stringformat flags for center alignment and no trimming
			StringFormat sf = StringFormat.GenericTypographic;
			sf.Trimming = StringTrimming.None;
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// Create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			// One line maximum
			int textHeight = (int)Math.Ceiling(drawing.MeasureString(text, font, 100000, sf).Height);

			// Find the proper squish ratio for given corner element
			int textFullWidth = (int)drawing.MeasureString(text, font, 100000, sf).Width;
			if (textFullWidth > maxWidth)
			{
				float horizontalSquish = (float)maxWidth / textFullWidth;
				drawing.ScaleTransform(horizontalSquish, 1.0F);
				maxWidth = textFullWidth;
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textBrush, font, null);

			// Set up variables
			int startY = (maxHeight - textHeight) / 2;

			// Draw title word by word
			int currentX = (maxWidth - textFullWidth) / 2;
			foreach (CardWord word in words)
			{
				int wordWidth = (int)Math.Ceiling(word.GetSizeF(drawing, maxWidth, sf).Width);
				int wordHeight = (int)Math.Ceiling(word.GetSizeF(drawing, maxWidth, sf).Height);
				drawing.DrawString(word.GetText(), word.GetTextFont(), word.GetTextBrush(), new RectangleF(currentX, startY, wordWidth, wordHeight), sf);
				currentX += wordWidth;
			}

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, element + ".png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Goes letter by letter to build words based on formatting rules.
		/// </summary>
		/// <param name="text">text to be converted</param>
		/// <param name="defaultBrush">default text brush</param>
		/// <param name="defaultFont">default text font</param>
		/// <param name="keywordData">a dictionary of keywords and their associated colors</param>
		/// <param name="isType">whether or not this is the type element</param>
		/// <returns>a list of all words as CardWord objects</returns>
		public static List<CardWord> GetCardWords(string text, Brush defaultBrush, Font defaultFont, Dictionary<string, string>? keywordData, bool isType = false)
		{
			// char italicSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "italicCharacter"));
			char escapeSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "escapeCharacter"));
			char newlineSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "newlineCharacter"));

			// Font italicFont = new Font(defaultFont, FontStyle.Italic);
			Font boldFont = new(defaultFont, FontStyle.Bold);

			List<CardWord> cardWords = [];

			// bool italicsOpen = false;
			bool escapeNext = false;
			bool ignoreFormatting = false;
			string builtWord = "";
			foreach (char letter in text)
			{
				if (letter == ' ' || (isType && letter == '/'))
				{
					// End of word
					if (builtWord != "")
					{
						bool isKeyword = keywordData != null && keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
						CardWord word = new(
							builtWord,
							isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord], 16))) : defaultBrush,
							isKeyword && !ignoreFormatting ? boldFont : defaultFont
						);
						word.SetType(isType);
						cardWords.Add(word);
					}
					if (!isType) cardWords.Add(new CardWord(" ", defaultBrush, defaultFont));
					else if (letter == '/') cardWords.Add(new CardWord("/", defaultBrush, defaultFont));
					builtWord = "";
					ignoreFormatting = false;
				}
				else if (escapeNext)
				{
					// If the next was a symbol, then we escape it
					if (/*letter == italicSymbol ||*/
						letter == escapeSymbol ||
						letter == newlineSymbol
						)
					{
						cardWords.Add(new CardWord(Convert.ToString(letter)));
					}

					// Otherwise, this means the next word should not be formatted
					else
					{
						if (builtWord == "") ignoreFormatting = true;
						builtWord += letter;
					}
					escapeNext = false;
				}
				else
				{
					/*if (letter == italicSymbol && italicsOpen)
					{
						italicsOpen = true;
					}
					else */if (letter == escapeSymbol)
					{
						escapeNext = true;
					}
					else if (letter == newlineSymbol || letter == '\n')
					{
						if (builtWord != "")
						{
							bool isKeyword = keywordData != null && keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
							CardWord word = new(
								builtWord,
								isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord], 16))) : defaultBrush,
								isKeyword && !ignoreFormatting ? boldFont : defaultFont
							);
							cardWords.Add(word);
							builtWord = "";
						}
						cardWords.Add(new CardWord("\n", defaultBrush, defaultFont));
						ignoreFormatting = false;
					}
					else
					{
						if (MiscHelper.IsPunctuation(Convert.ToString(letter)) && builtWord != "")
						{
							bool isKeyword = keywordData != null && keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
							CardWord word = new(
								builtWord,
								isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord], 16))) : defaultBrush,
								isKeyword && !ignoreFormatting ? boldFont : defaultFont
							);
							cardWords.Add(word);
							cardWords.Add(new CardWord(Convert.ToString(letter), defaultBrush, defaultFont));
							builtWord = "";
							ignoreFormatting = false;
						}
						// Most generic case - add a letter to builtWord
						else
						{
							builtWord += letter;
						}
					}
				}
			}
			if (builtWord != "")
			{
				bool isKeyword = keywordData != null && keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
				CardWord word = new(
					builtWord,
					isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord], 16))) : defaultBrush,
					isKeyword && !ignoreFormatting ? boldFont : defaultFont
				);
				word.SetType(isType);
				cardWords.Add(word);
			}

			return cardWords;
		}

		/// <summary>
		/// Takes in a list of words and measures how much vertical space they will need.
		/// </summary>
		/// <param name="words">the list of CardWord objects to measure</param>
		/// <param name="g">a Graphics object to measure with</param>
		/// <param name="sf">the StringFormat used to draw the words</param>
		/// <param name="maxW">the maximum width the words can take up</param>
		/// <param name="lineHeight">the height of each line</param>
		/// <returns>the vertical space needed by these words</returns>
		public static float MeasureWordByWord(List<CardWord> words, Graphics g, StringFormat sf, float maxW, float lineHeight)
		{
			float actionSymbolLines = float.Parse(ConfigHelper.GetConfigValue("asset", "actionSymbolLines"));
			float dividingLineLines = float.Parse(ConfigHelper.GetConfigValue("asset", "dividingLineLines"));

			float textHeight = 0;
			float lineWidth = 0;
			bool space = false;
			float spaceWidth = 0;
			foreach (CardWord word in words)
			{
				// Keywords: add the amount of vertical space they take up
				if (AssetHelper.AssetExists(word.GetText()))
				{
					if (lineWidth > 0) // Check if some words have already been added to line
					{
						textHeight += lineHeight;
						lineWidth = 0;
						space = false;
					}
					if (word.GetText() == "DividingLine_") textHeight += dividingLineLines * lineHeight;
					else textHeight += actionSymbolLines * lineHeight;
				}
				// Spaces: set flag
				else if (word.GetText() == " ")
				{
					space = true;
					spaceWidth = word.GetSizeF(g, (int)maxW, sf).Width;
				}
				// Newlines: add a line
				else if (word.GetText() == "\n")
				{
					textHeight += lineHeight;
					lineWidth = 0; // Completely ignore the text that was already built up
				}
				// Generic case: add word's width (+ space), check if over
				else
				{
					lineWidth += word.GetSizeF(g, (int)maxW, sf).Width + (space ? spaceWidth : 0);
					if (lineWidth > maxW)
					{
						textHeight += lineHeight;
						lineWidth = word.GetSizeF(g, (int)maxW, sf).Width;
					}
					space = false;
				}
			}
			if (lineWidth > 0) textHeight += lineHeight;
			return textHeight;
		}

		/// <summary>
		/// For use when words of varying styles need to be drawn in a cohesive paragraph.
		/// Meant to be used in a chain with other text that will share the same Graphics object.
		/// </summary>
		/// <param name="words">each word to be drawn</param>
		/// <param name="g">the Graphics object used to draw</param>
		/// <param name="f">the base Font used by the words</param>
		/// <param name="sf">the StringFormat used to draw the words</param>
		/// <param name="maxW">max width the words are allowed to take up</param>
		/// <param name="lineH">height each line takes up</param>
		/// <param name="startY">the y-coordinate to start drawing at</param>
		/// <returns>the y-coordinate drawing ended at</returns>
		private static float DrawWordByWord(List<CardWord> words, Graphics g, Font f, StringFormat sf, float maxW, float lineH, float startY, float xOffset = 0)
		{
			// Set up variables we'll potentially need
			float dlLines = float.Parse(ConfigHelper.GetConfigValue("asset", "dividingLineLines"));
			float asLines = float.Parse(ConfigHelper.GetConfigValue("asset", "actionSymbolLines"));
			float lineSpacing = float.Parse(ConfigHelper.GetConfigValue("text", "lineSpacingFactor"));

			// Draw text word by word
			float currentY = startY;
			int iCheck = 0;
			int iDraw = 0;
			float lineLength;
			bool drawAsset = false;
			bool endOfText = false;
			while (!endOfText)
			{
				// First, find the length of this line
				lineLength = 0;
				try
				{
					bool endOfLine = false;
					float currentWordWidth;
					bool space = false;
					float spaceWidth = 0;
					while (!endOfLine)
					{
						// Measure each word
						currentWordWidth = words[iCheck].GetSizeF(g, maxW, sf).Width;

						if (words[iCheck].GetText() == " ")
						{
							space = true;
							spaceWidth = words[iCheck].GetSizeF(g, maxW, sf).Width;
							iCheck++;
						}
						else if (AssetHelper.AssetExists(words[iCheck].GetText()))
						{
							endOfLine = true;
							drawAsset = true;
						}
						else if (words[iCheck].GetText() == "\n")
						{
							endOfLine = true;
							iCheck++;
						}
						else if (lineLength + currentWordWidth + (space ? spaceWidth : 0) > maxW)
						{
							endOfLine = true;
						}
						else
						{
							lineLength += currentWordWidth + (space ? spaceWidth : 0);
							space = false;
							iCheck++;
						}
					}
				}
				catch (Exception ex)            
				{                
					if (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
					{
						endOfText = true;
					}
					else
						throw;
				}
				// Draw each word in the line
				float currentX = (maxW + xOffset - lineLength) / 2;
				for (int i = iDraw; i < iCheck; i++)
				{
					CardWord word = words[i];
					float wordWidth = words[iDraw].GetSizeF(g, (int)maxW, sf).Width;
					g.DrawString(word.GetText(), word.GetTextFont(), word.GetTextBrush(), new RectangleF(currentX + xOffset, currentY, wordWidth, lineH), sf);
					currentX += wordWidth;
					iDraw++;
				}
				// Move the register down
				currentY += lineH * lineSpacing;
				// Draw the asset that is up to draw
				if (drawAsset)
				{
					string assetName = AssetHelper.GetAssetName(words[iDraw].GetText());
					Image asset = Image.FromFile(PathHelper.GetFullPath(Path.Combine("assets", assetName + AssetHelper.FindExtension(assetName))));
					float yOffset = string.Equals(assetName, "DividingLine") ? dlLines * lineH / 2 : asLines * lineH / 2;
					float resizing = string.Equals(assetName, "DividingLine") ? 1.0F : asLines * lineH / asset.Height;
					DrawSymbol(asset, g, maxW / 2, currentY + yOffset, resizing);
					currentY += lineSpacing * lineH * (string.Equals(assetName, "DividingLine") ? dlLines : asLines);
					iCheck++;
					iDraw++;
					drawAsset = false;
				}
				// Eat through whitespace
				bool ignoreSpaces = true;
				while (ignoreSpaces)
				{
					if (iDraw < words.Count)
					{
						if (words[iDraw].GetText() == " ") iDraw++;
						else ignoreSpaces = false;
					}
					else ignoreSpaces = false;
				}
			}
			return currentY;
		}

		/// <summary>
		/// Draws the given square symbol at the given coordinates.
		/// </summary>
		/// <param name="symbol">the symbol to draw</param>
		/// <param name="g">the Graphics object this will use to draw</param>
		/// <param name="centerX">the x-coordinate of the center of the symbol</param>
		/// <param name="centerY">the y-coordinate of the center of the symbol</param>
		/// <param name="resizing">optional resizing factor</param>
		private static void DrawSymbol(Image symbol, Graphics g, float centerX, float centerY, float resizing = 1.0F)
		{
			Bitmap b = new(symbol, new Size((int)(symbol.Width * resizing), (int)(symbol.Height * resizing)));
			float x = centerX - resizing * symbol.Width / 2;
			float y = centerY - resizing * symbol.Height / 2;
			g.DrawImage(b, new PointF(x, y));
		}

		/// <summary>
		/// Removes formatting and replaces symbols in the given text.
		/// </summary>
		/// <param name="text">string to be cleaned</param>
		/// <returns>the cleaned text</returns>
		private static string GetCleanText(string text)
		{
			List<char> cleanText = [];

			// char italicSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "italicCharacter"));
			char escapeSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "escapeCharacter"));
			char newlineSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "newlineCharacter"));
			char assetSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "assetCharacter"));

			// bool italicsOpen = false;
			bool escapeNext = false;
			string storedWord = "";
			foreach (char letter in text)
			{
				if (escapeNext)
				{
					cleanText.Add(letter);
					escapeNext = false;
				}
				else
				{
					/*if (letter == italicSymbol && italicsOpen)
					{
						italicsOpen = true;
					}
					else */if (letter == escapeSymbol)
					{
						escapeNext = true;
					}
					else if (letter == newlineSymbol)
					{
						cleanText.Add('\n');
					}
					else if (letter == assetSymbol)
					{
						if (AssetHelper.AssetExists(storedWord + assetSymbol))
						{
							for (int _ = 0; _ < storedWord.Length; _++)
							{
								cleanText.RemoveAt(cleanText.Count - 1);
							}
							cleanText.Add('\n'); // carriage return to start new line
							cleanText.Add('\n'); // carriage return to leave one line empty
							if (storedWord != "DividingLine")
							{
								cleanText.Add('\n'); // carriage return to leave two lines empty
								cleanText.Add('\n'); // carriage return to leave three lines empty
							}
						}
					}
					else
					{
						if (letter == ' ') storedWord = "";
						else storedWord += letter;
						cleanText.Add(letter);
					}
				}
			}

			return new string([.. cleanText]);
		}

		public class CardWord
		{
			private string text;
			private Brush textBrush;
			private Font textFont;
			private bool isType = false;

			public CardWord()
			{
				text = "";
				textBrush = new SolidBrush(Color.Black);
				textFont = new Font(FontLoader.GetFont("roboto.ttf", 1), new FontStyle());
			}

			public CardWord(string text)
			{
				this.text = text;
				textBrush = new SolidBrush(Color.Black);
				textFont = new Font(FontLoader.GetFont("roboto.ttf", 1), new FontStyle());
			}

			public CardWord(string text, Brush textBrush, Font textFont)
			{
				this.text = text;
				this.textBrush = textBrush;
				this.textFont = textFont;
			}

			public CardWord(CardWord other)
			{
				text = other.GetText();
				textBrush = other.GetTextBrush();
				textFont = other.GetTextFont();
			}

			public string GetText() { return text; }
			public Brush GetTextBrush() { return textBrush; }
			public Font GetTextFont() { return textFont; }
			public bool IsType() { return isType; }

			public void SetText(string text) { this.text = text; }
			public void SetTextBrush(Brush textBrush) { this.textBrush = textBrush; }
			public void SetTextFont(Font textFont) { this.textFont = textFont; }
			public void SetType(bool isType) { this.isType = isType; }

			public SizeF GetSizeF(Graphics drawing, float maxWidth, StringFormat sf)
			{
				if (text == " ")
				{
					return drawing.MeasureString(text, textFont) * float.Parse(ConfigHelper.GetConfigValue("text", "spaceShrink"));
				}
				else if (isType)
				{
					return drawing.MeasureString(text.ToUpper(), textFont, (int)maxWidth, sf);
				}
				return drawing.MeasureString(text, textFont, (int)maxWidth, sf);
			}
		}
	}
}