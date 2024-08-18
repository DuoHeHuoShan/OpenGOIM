using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGData
	{
		public string Name;

		private DateTime mTimestamp;

		public DateTime Timestamp
		{
			get
			{
				return mTimestamp;
			}
		}

		public virtual int Count
		{
			get
			{
				return 0;
			}
		}

		public CGData()
		{
			mTimestamp = DateTime.Now;
		}

		public static implicit operator bool(CGData a)
		{
			return !object.ReferenceEquals(a, null);
		}

		public virtual T Clone<T>() where T : CGData
		{
			return new CGData() as T;
		}

		protected int getGenericFIndex(ref float[] FMapArray, float fValue, out float frag)
		{
			if (fValue == 1f)
			{
				frag = 1f;
				return FMapArray.Length - 2;
			}
			fValue = Mathf.Repeat(fValue, 1f);
			for (int i = 1; i < FMapArray.Length; i++)
			{
				if (FMapArray[i] > fValue)
				{
					frag = (fValue - FMapArray[i - 1]) / (FMapArray[i] - FMapArray[i - 1]);
					return i - 1;
				}
			}
			frag = 0f;
			return 0;
		}

		public void Touch()
		{
			mTimestamp = DateTime.Now;
		}
	}
}
