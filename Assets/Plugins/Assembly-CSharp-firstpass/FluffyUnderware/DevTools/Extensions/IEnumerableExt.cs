using System;
using System.Collections.Generic;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class IEnumerableExt
	{
		public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
		{
			foreach (T item in ie)
			{
				action(item);
			}
		}
	}
}
