using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class WeightedRandom<T>
	{
		private List<T> mData;

		private int mCurrentPosition = -1;

		private T mCurrentItem;

		public int Seed { get; set; }

		public bool RandomizeSeed { get; set; }

		private int Capacity
		{
			get
			{
				return mData.Capacity;
			}
		}

		public int Size
		{
			get
			{
				return mData.Count;
			}
		}

		public WeightedRandom(int initCapacity = 0)
		{
			mData = new List<T>(initCapacity);
		}

		public void Add(T item, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				mData.Add(item);
			}
			mCurrentPosition = Size - 1;
		}

		public T Next()
		{
			if (mCurrentPosition < 1)
			{
				mCurrentPosition = Size - 1;
				mCurrentItem = mData[0];
				return mCurrentItem;
			}
			Random.State state = Random.state;
			if (RandomizeSeed)
			{
				Seed = Random.Range(0, int.MaxValue);
			}
			Random.InitState(Seed);
			int index = Random.Range(0, mCurrentPosition);
			Random.state = state;
			mCurrentItem = mData[index];
			mData[index] = mData[mCurrentPosition];
			mData[mCurrentPosition] = mCurrentItem;
			mCurrentPosition--;
			return mCurrentItem;
		}

		public void Reset()
		{
			mCurrentPosition = Size - 1;
		}

		public void Clear()
		{
			mData.Clear();
			mCurrentPosition = -1;
		}
	}
}
