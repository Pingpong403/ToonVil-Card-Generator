using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Metadata;

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
		public static void DrawText(String text, Font font, Color textColor, int maxWidth, String element)
		{
			// First, if it is a Title, capitalize the text
			if (string.Equals(element.ToLower(), "title"))
			{
				text = text.ToUpper();
			}

			// Set the stringformat flags and trimming
			StringFormat sf = new StringFormat();
			sf.Trimming = StringTrimming.Word;

			// Create a dummy bitmap just to get a graphics object
			Image img = new Bitmap(1, 1);
			Graphics drawing = Graphics.FromImage(img);

			// Measure the string to see how big the image needs to be
			SizeF textSize = drawing.MeasureString(text, font, maxWidth, sf);
			img.Dispose();
			drawing.Dispose();

			// Create a new image of the right size with minimal padding
			int areaWidth = (int)System.Math.Ceiling(textSize.Width);
			int areaHheight = (int)System.Math.Ceiling(textSize.Height);
			img = new Bitmap(areaWidth, areaHheight);
			drawing = Graphics.FromImage(img);

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

			// Clean up words and create a list for them


			// Set up variables
			string[] words = text.Split(" ");
			// string[] cleanWords = GetCleanWords(text);
			// Dictionary<string, int> wordsAndLengths = GetWordsAndLengths(text, font, maxWidth);
			int iCheckWordLengths = 0;
			int iDraw = 0;
			int line = 0;
			int lineLength;

			// Draw text word by word
			bool done = false;
			while (!done)
			{
				lineLength = 0;
				try
				{
					// while (lineLength + wordsAndLengths[cleanWords[iCheckWordLengths]] - 7 <= maxWidth)
					// {
					// 	lineLength += wordsAndLengths[cleanWords[iCheckWordLengths]] - 7;
					// 	iCheckWordLengths++;
					// }
				}
				catch (IndexOutOfRangeException)
				{
					done = true;
				}
				int currentX = areaWidth / 2 - lineLength / 2;
				for (int i = iDraw; i <= iCheckWordLengths - 1; i++)
				{
					// string word = cleanWords[i];
					// int wordWidth = wordsAndLengths[cleanWords[iDraw]];
					// drawing.DrawString(word, font, textBrush, new RectangleF(currentX, 200 * line + 7, wordWidth + 6, 200), sf);
					// currentX += wordWidth - 7;
					// iDraw++;
				}
				line++;
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
							isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32(keywordData[builtWord], 16))) : defaultBrush,
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
								isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32(keywordData[builtWord], 16))) : defaultBrush,
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
					isKeyword && !ignoreFormatting ? new SolidBrush(Color.FromArgb(Convert.ToInt32(keywordData[builtWord], 16))) : defaultBrush,
					isKeyword && !ignoreFormatting ? boldFont : defaultFont
				);
				cardWords.Add(word);
			}

			return cardWords;
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
				textFont = new Font(FontLoader.GetFont("roboto", 1), new FontStyle());
			}

			public CardWord(string text)
			{
				this.text = text;
				textBrush = new SolidBrush(Color.Black);
				textFont = new Font(FontLoader.GetFont("roboto", 1), new FontStyle());
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

			public SizeF GetSizeF(Graphics drawing)
			{
				return drawing.MeasureString(text, textFont);
			}
		}
	}
}