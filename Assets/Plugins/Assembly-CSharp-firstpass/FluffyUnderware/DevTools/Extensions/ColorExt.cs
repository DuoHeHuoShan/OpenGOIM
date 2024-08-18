using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ColorExt
	{
		public static string ToHtml(this Color c)
		{
			Color32 color = c;
			return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.r, color.g, color.b, color.a);
		}
	}
}
