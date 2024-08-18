using System;
using System.Globalization;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class StringExt
	{
		public static Color ColorFromHtml(this string hexString)
		{
			if (hexString.Length < 9)
			{
				hexString += "FF";
			}
			if (hexString.StartsWith("#") && hexString.Length == 9)
			{
				int[] array = new int[4];
				try
				{
					array[0] = int.Parse(hexString.Substring(1, 2), NumberStyles.HexNumber);
					array[1] = int.Parse(hexString.Substring(3, 2), NumberStyles.HexNumber);
					array[2] = int.Parse(hexString.Substring(5, 2), NumberStyles.HexNumber);
					array[3] = int.Parse(hexString.Substring(7, 2), NumberStyles.HexNumber);
					return new Color((float)array[0] / 255f, (float)array[1] / 255f, (float)array[2] / 255f, (float)array[3] / 255f);
				}
				catch
				{
					return Color.white;
				}
			}
			return Color.white;
		}

		public static string TrimStart(this string s, string trim, StringComparison compare = StringComparison.CurrentCultureIgnoreCase)
		{
			if (!s.StartsWith(trim, compare))
			{
				return s;
			}
			return s.Substring(trim.Length);
		}

		public static string TrimEnd(this string s, string trim, StringComparison compare = StringComparison.CurrentCultureIgnoreCase)
		{
			if (!s.EndsWith(trim, compare))
			{
				return s;
			}
			return s.Substring(0, s.Length - trim.Length);
		}
	}
}
