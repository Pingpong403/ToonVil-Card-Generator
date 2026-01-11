using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class KeywordHelper
	{
		/// <summary>
		/// Gets all the keywords that exist in Keywords.txt
		/// </summary>
		/// <returns>A hashset of all keywords found.</returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static HashSet<string> GetKeywords()
		{
			HashSet<string> keywords = new HashSet<string>();

			// Keywords.txt should hold all of the keywords
			string path = PathHelper.GetFullPath("-TextFiles\\Keywords.txt");
			if (!File.Exists(path))
            {
                // Fallback to current working dir (e.g., when running with dotnet run from project root)
                path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "-TextFiles", "Keywords.txt"));
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Keyword file not found: {path}");
            }

			string line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new StreamReader(path);

				// Read the first line of text
				line = sr.ReadLine();

				// Continue to read until you reach end of file
				while (line != null)
				{
					// '#' denotes a comment line
					if (line[0] != '#')
					{
						// Split line individual keywords
						string[] variations = line.Split("|");

						// Add each to hashset
						foreach (string variation in variations) keywords.Add(variation);
					}
					// Read the next line
					line = sr.ReadLine();
				}
				// Close the file
				sr.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
			}

			return keywords;
		}

		public static Dictionary<string, string> GetColorMapping()
		{
			Dictionary<string, string> baseKeywordColors = new Dictionary<string, string>();
			Dictionary<string, string> keywordsAndColors = new Dictionary<string, string>();

			// Populate dictionary to hold each singular keyword and its corresponding color
			foreach (string line in MiscHelper.GetLines("Colors"))
			{
				string[] lineSplit = line.Split("|");
				if (lineSplit.Length == 1)
				{
					// Search through color-config.txt for correct color
					string searchParam = lineSplit[0].ToLower() + "Color";
					baseKeywordColors[lineSplit[0]] = ConfigHelper.GetConfigValue("color", searchParam);
				}
				else
				{
					// First part is the base keyword, second part is its color
					baseKeywordColors[lineSplit[0]] = lineSplit[1];
				}
			}

			// Link each keyword variant to its singular form and therefore its correct color
			foreach (string line in MiscHelper.GetLines("Keywords"))
			{
				string[] lineSplit = line.Split("|");
				foreach (string variant in lineSplit)
				{
					if (variant != "")
					{
						if (!baseKeywordColors.TryGetValue(lineSplit[0], out string? value))
						{
							keywordsAndColors[variant] = ConfigHelper.GetConfigValue("color", "fontColor");
						}
						else
						{
							keywordsAndColors[variant] = value == "" ? ConfigHelper.GetConfigValue("color", "fontColor") : value;
						}
					}
				}
			}

			return keywordsAndColors;
		}
	}
}