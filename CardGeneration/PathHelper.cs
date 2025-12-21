using System;

namespace ToonVil_Card_Generator
{
	public static class PathHelper
	{
		public static string GetFullPath(string relativePath)
		{
			var baseDir = AppContext.BaseDirectory;
            var fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", relativePath));
			return fullPath;
		}
	}
}