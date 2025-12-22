using System;

namespace ToonVil_Card_Generator.CardGeneration
{
	public static class MiscHelper
	{
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
	}
}