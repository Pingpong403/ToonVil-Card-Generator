using System;
using System.Net;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class SettingsHelper
	{
		/// <summary>
		/// Checks for a setting in the specified -Settings file.
		/// </summary>
		/// <param name="settingsFile">the settings file to search, not including the trailing "Settings.txt"</param>
		/// <param name="setting">the setting to check</param>
		/// <returns>the setting if it is set, an empty string if not</returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static string GetSettingsValue(string settingsFile, string setting)
		{
			string path = PathHelper.GetFullPath(Path.Combine("Card Data", "-Settings", settingsFile + "Settings.txt"));
			if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Settings file not found: {path}");
            }

			string line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new(path);

				// Read the first line of text
				line = sr.ReadLine();

				//Continue to read until you reach end of file
				while (line != null)
				{
					if (line != "")
					{
					if (line[0] != '#')
						{
							// Split line into key, value pair
							string[] pair = line.Split(":");

							// If right side is set, return true
							if (pair[0] == setting && pair[1] != "")
							{
								return pair[1];
							}
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
			
			return "";
		}
	}
}