using System;
using System.Collections;
using System.Collections.Generic;

namespace FluffyUnderware.DevTools
{
	public class Ring<T> : IList<T>, IEnumerable, ICollection<T>, IEnumerable<T>
	{
		private List<T> mList;

		private int mIndex;

		public int Size { get; private set; }

		public T this[int index]
		{
			get
			{
				return mList[index];
			}
			set
			{
				mList[index] = value;
			}
		}

		public int Count
		{
			get
			{
				return mList.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public Ring(int size)
		{
			mList = new List<T>(size);
			Size = size;
		}

		public void Add(T item)
		{
			if (mList.Count == Size)
			{
				mList[mIndex++] = item;
				if (mIndex == mList.Count)
				{
					mIndex = 0;
				}
			}
			else
			{
				mList.Add(item);
			}
		}

		public void Clear()
		{
			mList.Clear();
			mIndex = 0;
		}

		public int IndexOf(T item)
		{
			return mList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public IEnumerator GetEnumerator()
		{
			return mList.GetEnumerator();
		}

		public bool Contains(T item)
		{
			return mList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			mList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return mList.Remove(item);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
