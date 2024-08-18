using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ArrayExt
	{
		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			length = Mathf.Clamp(length, 0, data.Length - index);
			T[] array = new T[length];
			if (length > 0)
			{
				Array.Copy(data, index, array, 0, length);
			}
			return array;
		}

		public static T[] RemoveAt<T>(this T[] source, int index)
		{
			T[] array = new T[source.Length - 1];
			if (index > 0)
			{
				Array.Copy(source, 0, array, 0, index);
			}
			if (index < source.Length - 1)
			{
				Array.Copy(source, index + 1, array, index, source.Length - index - 1);
			}
			return array;
		}

		public static T[] InsertAt<T>(this T[] source, int index)
		{
			T[] array = new T[source.Length + 1];
			index = Mathf.Clamp(index, 0, source.Length - 1);
			if (index > 0)
			{
				Array.Copy(source, 0, array, 0, index);
			}
			Array.Copy(source, index, array, index + 1, source.Length - index);
			return array;
		}

		public static T[] Swap<T>(this T[] source, int index, int with)
		{
			index = Mathf.Clamp(index, 0, source.Length - 1);
			with = Mathf.Clamp(index, 0, source.Length - 1);
			T val = source[index];
			source[index] = source[with];
			source[with] = val;
			return source;
		}

		public static T[] Add<T>(this T[] source, T item)
		{
			Array.Resize(ref source, source.Length + 1);
			source[source.Length - 1] = item;
			return source;
		}

		public static T[] AddRange<T>(this T[] source, T[] items)
		{
			Array.Resize(ref source, source.Length + items.Length);
			Array.Copy(items, 0, source, source.Length - items.Length, items.Length);
			return source;
		}

		public static T[] RemoveDuplicates<T>(this T[] source)
		{
			List<T> list = new List<T>();
			HashSet<T> hashSet = new HashSet<T>();
			foreach (T item in source)
			{
				if (hashSet.Add(item))
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public static int IndexOf<T>(this T[] source, T item)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (source[i].Equals(item))
				{
					return i;
				}
			}
			return -1;
		}

		public static T[] Remove<T>(this T[] source, T item)
		{
			int num = source.IndexOf(item);
			if (num > -1)
			{
				return source.RemoveAt(num);
			}
			return source;
		}
	}
}
