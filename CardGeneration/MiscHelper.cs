using System;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class MiscHelper
	{
		/// <summary>
		/// Gets all the lines in a given -TextFiles sub-directory.
		/// </summary>
		/// <param name="file">The file to be searched, not including the extension.</param>
		/// <returns>Each non-comment line found in the file.</returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static List<string> GetTextFilesLines(string file)
		{
			List<string> lines = [];
			string path = PathHelper.GetFullPath(Path.Combine("Card Data", "-TextFiles", file + ".txt"));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Text file not found: {path}");
            }

			string line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new(path);

				// Read the first line of text
				line = sr.ReadLine();

				// Continue to read until you reach end of file
				while (line != null)
				{
					// Skip empty lines
					if (line != "")
					{
						// '#' denotes a comment line
						if (line[0] != '#')
						{
							// Add the line to the list
							lines.Add(line);
						}
					}
					// Read the next line
					line = sr.ReadLine();
				}
				//close the file
				sr.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
			}

			return lines;
		}

		/// <summary>
		/// Helper method to capitalize just the first letter in a string.
		/// </summary>
		/// <param name="input">String to be capitalized</param>
		/// <returns>The capitalized string</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <meta>Original code from https://stackoverflow.com/a/4405876</meta>
		public static string Capitalize(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };

		/// <summary>
		/// Punctuation hugs the ends of words.
		/// </summary>
		/// <param name="text">Text to be compared to list of punctuation.</param>
		/// <returns>Whether or not given text is punctuation.</returns>
		public static bool IsPunctuation(string text)
		{
			if (".?!,;:/-".Contains(text))
			{
				return true;
			}
			return false;
		}
	}
}