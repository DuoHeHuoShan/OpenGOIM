using System;

namespace ArabicSupport
{
	public class ArabicFixer
	{
		public static string Fix(string str)
		{
			return Fix(str, false, true);
		}

		public static string Fix(string str, bool rtl)
		{
			if (rtl)
			{
				return Fix(str);
			}
			string[] array = str.Split(' ');
			string text = "";
			string text2 = "";
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				if (char.IsLower(text3.ToLower()[text3.Length / 2]))
				{
					text = text + Fix(text2) + text3 + " ";
					text2 = "";
				}
				else
				{
					text2 = text2 + text3 + " ";
				}
			}
			if (text2 != "")
			{
				text += Fix(text2);
			}
			return text;
		}

		public static string Fix(string str, bool showTashkeel, bool useHinduNumbers)
		{
			ArabicFixerTool.showTashkeel = showTashkeel;
			ArabicFixerTool.useHinduNumbers = useHinduNumbers;
			if (str.Contains("\n"))
			{
				str = str.Replace("\n", Environment.NewLine);
			}
			if (str.Contains(Environment.NewLine))
			{
				string[] separator = new string[1] { Environment.NewLine };
				string[] array = str.Split(separator, StringSplitOptions.None);
				if (array.Length == 0)
				{
					return ArabicFixerTool.FixLine(str);
				}
				if (array.Length == 1)
				{
					return ArabicFixerTool.FixLine(str);
				}
				string text = ArabicFixerTool.FixLine(array[0]);
				int i = 1;
				if (array.Length > 1)
				{
					for (; i < array.Length; i++)
					{
						text = text + Environment.NewLine + ArabicFixerTool.FixLine(array[i]);
					}
				}
				return text;
			}
			return ArabicFixerTool.FixLine(str);
		}
	}
}
