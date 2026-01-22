using System;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class AssetHelper
	{
		/// <summary>
		/// Checks whether the given asset name exists.
		/// </summary>
		/// <param name="assetName">the name of the asset, not including the asset symbol</param>
		/// <returns>whether or not the given asset was found in the assets folder</returns>
		public static bool AssetExists(string assetName)
		{
			string relativePathNoExt = Path.Combine("assets", assetName);
			string relativePath = relativePathNoExt + ".png";
			if (!File.Exists(PathHelper.GetFullPath(relativePath)))
			{
				relativePath = relativePathNoExt + ".jpg";
				if (!File.Exists(PathHelper.GetFullPath(relativePath)))
				{
					relativePath = relativePathNoExt + ".jpeg";
					return File.Exists(PathHelper.GetFullPath(relativePath));
				}
				return true;
			}
			return true;
		}

		/// <summary>
		/// Strips the trailing asset symbol from the given asset code.
		/// </summary>
		/// <param name="assetCode">the code for the asset, including the asset symbol</param>
		/// <returns></returns>
		public static string GetAssetName(string assetCode)
		{
			char[] letters = new char[assetCode.Length - 1];
			for (int i = 0; i < assetCode.Length - 1; i++)
			{
				letters[i] = assetCode[i];
			}

			return new string(letters);
		}

		/// <summary>
		/// Checks whether a gains action code is a Gain x Power symbol.
		/// </summary>
		/// <param name="gainsActionCode">the string to be checked</param>
		/// <returns>the amount of power if this is a gain power code, otherwise an empty string</returns>
		public static string GainPowerAmount(string gainsActionCode)
		{
			char[] letters = gainsActionCode.ToCharArray();
			if (letters.Length >= 10)
			{
				// GainXPower
				string gain = new(letters[0..4]);
				string power = new(letters[(letters.Length - 5)..letters.Length]);
				if (Equals(gain, "Gain") && Equals(power, "Power"))
				{
					return new string(letters[4..(letters.Length - 5)]);
				}
			}
			return "";
		}
	}
}