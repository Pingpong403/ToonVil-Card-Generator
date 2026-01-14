using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Metadata;
using System.Diagnostics.Tracing;

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
		/// <meta>Drawing code from https://gist.github.com/naveedmurtuza/6600103</meta>
		public static void DrawText(String text, Font font, Color textColor, int maxWidth, int maxHeight, String element, Dictionary<string, string> keywordsAndColors)
		{
			// First, if it is a Title, capitalize the text
			if (string.Equals(element.ToLower(), "title"))
			{
				text = text.ToUpper();
			}

			// Set the stringformat flags and trimming
			StringFormat sf = StringFormat.GenericTypographic;
			sf.Trimming = StringTrimming.Word;
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			// Create a new image of the right size with minimal padding
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Adjust for high quality
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// Create a brush for the text
			Brush textBrush = new SolidBrush(textColor);

			// Find the proper font size or squish ratio for given text
			int textHeight;
			if (element != "Ability") textHeight = (int)Math.Ceiling(drawing.MeasureString(text, font, 100000, sf).Height);
			else textHeight = (int)Math.Ceiling(drawing.MeasureString(text, font, maxWidth, sf).Height);
			float horizontalSquish = 1.0F;
			float granularity = 0.5F;
			if (element == "Ability")
			{
				float currentFontSize = font.Size;
				do
				{
					SizeF currentSize = drawing.MeasureString(text, font, maxWidth, sf);
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
					horizontalSquish = (float)maxWidth / textFullWidth;
					drawing.ScaleTransform(horizontalSquish, 1.0F);
					maxWidth = (int)textFullWidth;
				}
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textBrush, font, keywordsAndColors);

			// Set up variables
			int iCheckWordLengths = 0;
			int iDraw = 0;
			int line = 0;
			int lineLength;
			int startY = (maxHeight - textHeight) / 2;

			// Draw text word by word
			bool done = false;
			while (!done)
			{
				lineLength = 0;
				try
				{
					while (lineLength + (int)Math.Ceiling(words[iCheckWordLengths].GetSizeF(drawing, maxWidth, sf).Width) <= maxWidth)
					{
						lineLength += (int)Math.Ceiling(words[iCheckWordLengths].GetSizeF(drawing, maxWidth, sf).Width);
						iCheckWordLengths++;
					}
				}
				catch (Exception ex)            
				{                
					if (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
					{
						done = true;
					}
					else
						throw;
				}
				int currentX = (maxWidth - lineLength) / 2;
				for (int i = iDraw; i < iCheckWordLengths; i++)
				{
					CardWord word = words[i];
					int wordWidth = (int)Math.Ceiling(words[iDraw].GetSizeF(drawing, maxWidth, sf).Width);
					int wordHeight = (int)Math.Ceiling(words[iDraw].GetSizeF(drawing, maxWidth, sf).Height);
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
			Console.WriteLine($"Image saved: {outpath}");
		}

		public static List<CardWord> GetCardWords(string text, Brush defaultBrush, Font defaultFont, Dictionary<string, string> keywordData)
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
						bool isKeyword = keywordData.TryGetValue(builtWord, out string? value);
						CardWord word = new CardWord(
							builtWord,
							isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16))) : defaultBrush,
							isKeyword && !ignoreFormatting ? boldFont : defaultFont
						);
						cardWords.Add(word);
					}
					cardWords.Add(new CardWord(" ", defaultBrush, defaultFont));
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
					else if (letter == newlineSymbol)
					{
						cardWords.Add(new CardWord("\n"));
					}
					else
					{
						if (MiscHelper.IsPunctuation(Convert.ToString(letter)))
						{
							bool isKeyword = keywordData.TryGetValue(builtWord, out string? value);
							CardWord word = new CardWord(
								builtWord,
								isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16))) : defaultBrush,
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
				bool isKeyword = keywordData.TryGetValue(builtWord, out string? value);
				CardWord word = new CardWord(
					builtWord,
					isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16))) : defaultBrush,
					isKeyword && !ignoreFormatting ? boldFont : defaultFont
				);
				cardWords.Add(word);
			}

			return cardWords;
		}

		private static string GetCleanText(string text)
		{
			char[] cleanText = new char[text.Length];

			// char italicSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "italicCharacter"));
			char escapeSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "escapeCharacter"));
			char newlineSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "newlineCharacter"));

			// bool italicsOpen = false;
			bool escapeNext = false;
			foreach (char letter in text)
			{
				if (escapeNext)
				{
					cleanText.Append(letter);
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
						cleanText.Append('\n');
					}
					else
					{
						cleanText.Append(letter);
					}
				}
			}

			return new string(cleanText);
		}

		public class CardWord
		{
			private string text;
			private Brush textBrush;
			private Font textFont;

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

			public void SetText(string text) { this.text = text; }
			public void SetTextBrush(Brush textBrush) { this.textBrush = textBrush; }
			public void SetTextFont(Font textFont) { this.textFont = textFont; }

			public SizeF GetSizeF(Graphics drawing, int maxWidth, StringFormat sf)
			{
				if (text == " ")
				{
					return drawing.MeasureString(text, textFont) * 0.7F;
				}
				return drawing.MeasureString(text, textFont, maxWidth, sf);
			}
		}
	}
}