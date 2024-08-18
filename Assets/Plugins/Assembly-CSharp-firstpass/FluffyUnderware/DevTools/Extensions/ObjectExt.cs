using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ObjectExt
	{
		public static void Destroy(this Object c)
		{
			Object.Destroy(c);
		}

		public static string ToDumpString(this object o)
		{
			return new DTObjectDump(o).ToString();
		}
	}
}
