using System;
using System.IO;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class ConfigHelper
	{
		// card-config.txt helpers
		public static string GetConfigValue(string configFile, string key)
		{
			string path = PathHelper.GetFullPath($"config\\{configFile}");
			if (!File.Exists(path))
            {
                // Fallback to current working dir (e.g., when running with dotnet run from project root)
                path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "config", configFile));
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Config file not found: {path}");
            }

			string line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new StreamReader(path);

				// Read the first line of text
				line = sr.ReadLine();

				//Continue to read until you reach end of file
				while (line != null)
				{
					// Split line into key, value pair
					string[] pair = line.Split(":");

					// If key matches given key, return the value
					if (string.Equals(pair[0], key))
					{
						return pair[1];
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