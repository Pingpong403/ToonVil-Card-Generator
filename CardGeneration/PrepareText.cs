using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Metadata;
using System.Diagnostics.Tracing;
using System.Data;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class PrepareText
	{
		// summary
		// <meta>Drawing code originally from https://gist.github.com/naveedmurtuza/6600103</meta>
		public static void DrawText(String text, Font font, Color textColor, int maxWidth, int maxHeight, string element, Dictionary<string, string> keywordsAndColors)
		{
			// First, if it is a Title, capitalize the text
			if (string.Equals(element.ToLower(), "title") || string.Equals(element.ToLower(), "type"))
			{
				text = text.ToUpper();
			}

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

			// Everything but abilities have one line maximum
			int textHeight;
			if (element != "Ability") textHeight = (int)Math.Ceiling(drawing.MeasureString(text, font, 100000, sf).Height);
			else textHeight = (int)Math.Ceiling(drawing.MeasureString(GetCleanText(text), font, maxWidth, sf).Height);

			// Find the proper font size (abilities) or squish ratio (everything else)
			// for given text
			float granularity = 0.5F;
			if (element == "Ability")
			{
				float currentFontSize = font.Size;
				// Naively find maximum font size to fit text
				do
				{
					SizeF currentSize = drawing.MeasureString(GetCleanText(text), font, maxWidth, sf);
					textHeight = (int)Math.Ceiling(currentSize.Height);
					if (textHeight > maxHeight)
					{
						currentFontSize = font.Size - granularity;
						font = new Font(font.Name, currentFontSize, font.Style);
					}
				} while (textHeight > maxHeight);
			}
			else
			{
				int textFullWidth = (int)drawing.MeasureString(text, font, 100000, sf).Width;
				if (textFullWidth > maxWidth)
				{
					float horizontalSquish = (float)maxWidth / textFullWidth;
					drawing.ScaleTransform(horizontalSquish, 1.0F);
					maxWidth = (int)textFullWidth;
				}
			}
			
			// To keep line spacing consistent - adjust for padding
			float wordHeight = drawing.MeasureString("Tq", font, maxWidth, sf).Height;
			wordHeight *= 1.0F + (5.55F / wordHeight);

			// Get all the words
			List<CardWord> words = GetCardWords(text, textBrush, font, keywordsAndColors, String.Equals(element.ToLower(), "type"));

			// Set up variables
			int iCheckWordLengths = 0;
			int iDraw = 0;
			int line = 0;
			int lineLength;
			int startY = (maxHeight - textHeight) / 2;

			// Draw text word by word
			bool endOfText = false;
			while (!endOfText)
			{
				// First, find the length of this line
				lineLength = 0;
				try
				{
					bool endOfLine = false;
					bool skippedSpace = false;
					int currentWordWidth;
					int previousSpaceWidth = 0;
					while (!endOfLine)
					{
						if (skippedSpace) previousSpaceWidth = (int)Math.Ceiling(words[iCheckWordLengths - 1].GetSizeF(drawing, maxWidth, sf).Width);

						// Measure each word without regards to style.
						Font currentFont = words[iCheckWordLengths].GetTextFont();
						words[iCheckWordLengths].SetTextFont(font);
						currentWordWidth = (int)Math.Ceiling(words[iCheckWordLengths].GetSizeF(drawing, maxWidth, sf).Width);
						words[iCheckWordLengths].SetTextFont(currentFont);

						// Trying to access past end of available words will throw an
						// error that we will catch
						if (words[iCheckWordLengths].GetText() == " ")
						{
							skippedSpace = true;
							iCheckWordLengths++;
						}
						else if (lineLength + currentWordWidth + (skippedSpace ? previousSpaceWidth : 0) > maxWidth)
						{
							endOfLine = true;
						}
						else if (words[iCheckWordLengths].GetText() == "\n")
						{
							endOfLine = true;
							iCheckWordLengths++;
						}
						else
						{
							lineLength += currentWordWidth + (skippedSpace ? previousSpaceWidth : 0);
							skippedSpace = false;
							iCheckWordLengths++;
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

				// Then, draw each word in the line
				int currentX = (maxWidth - lineLength) / 2;
				for (int i = iDraw; i < iCheckWordLengths; i++)
				{
					CardWord word = words[i];
					int wordWidth = (int)Math.Ceiling(words[iDraw].GetSizeF(drawing, maxWidth, sf).Width);
					drawing.DrawString(word.GetText(), word.GetTextFont(), word.GetTextBrush(), new RectangleF(currentX, startY + wordHeight * line, wordWidth, wordHeight), sf);
					currentX += wordWidth;
					iDraw++;
				}
				line++;
			}

			drawing.Save();

			textBrush.Dispose();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, $"{element}.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		public static List<CardWord> GetCardWords(string text, Brush defaultBrush, Font defaultFont, Dictionary<string, string> keywordData, bool isType = false)
		{
			// char italicSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "italicCharacter"));
			char escapeSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "escapeCharacter"));
			char newlineSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "newlineCharacter"));

			// Font italicFont = new Font(defaultFont, FontStyle.Italic);
			Font boldFont = new Font(defaultFont, FontStyle.Bold);

			List<CardWord> cardWords = [];

			// bool italicsOpen = false;
			bool escapeNext = false;
			bool ignoreFormatting = false;
			string builtWord = "";
			foreach (char letter in text)
			{
				if (letter == ' ')
				{
					// End of word
					if (builtWord != "")
					{
						bool isKeyword = keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
						CardWord word = new CardWord(
							builtWord,
							isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord], 16))) : defaultBrush,
							isKeyword && !ignoreFormatting ? boldFont : defaultFont
						);
						word.SetType(isType);
						cardWords.Add(word);
					}
					if (!isType) cardWords.Add(new CardWord(" ", defaultBrush, defaultFont));
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
							bool isKeyword = keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
							CardWord word = new CardWord(
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
						if (MiscHelper.IsPunctuation(Convert.ToString(letter)))
						{
							bool isKeyword = keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
							CardWord word = new CardWord(
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
				bool isKeyword = keywordData.TryGetValue(isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord, out string? value);
				CardWord word = new CardWord(
					builtWord,
					isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[isType ? MiscHelper.Capitalize(builtWord.ToLower()) : builtWord], 16))) : defaultBrush,
					isKeyword && !ignoreFormatting ? boldFont : defaultFont
				);
				word.SetType(isType);
				cardWords.Add(word);
			}

			return cardWords;
		}

		private static string GetCleanText(string text)
		{
			List<char> cleanText = [];

			// char italicSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "italicCharacter"));
			char escapeSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "escapeCharacter"));
			char newlineSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "newlineCharacter"));

			// bool italicsOpen = false;
			bool escapeNext = false;
			foreach (char letter in text)
			{
				if (escapeNext)
				{
					cleanText.Add(letter);
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
					else
					{
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

			public SizeF GetSizeF(Graphics drawing, int maxWidth, StringFormat sf)
			{
				if (text == " ")
				{
					return drawing.MeasureString(text, textFont) * 0.675F;
				}
				else if (isType)
				{
					return drawing.MeasureString(text.ToUpper(), textFont, maxWidth, sf);
				}
				return drawing.MeasureString(text, textFont, maxWidth, sf);
			}
		}
	}
}