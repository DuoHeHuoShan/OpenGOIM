using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(1f, 0.8f, 0.5f, 1f)]
	public class CGBounds : CGData
	{
		protected Bounds? mBounds;

		public Bounds Bounds
		{
			get
			{
				if (!mBounds.HasValue)
				{
					RecalculateBounds();
				}
				return mBounds.Value;
			}
			set
			{
				if (mBounds != value)
				{
					mBounds = value;
				}
			}
		}

		public float Depth
		{
			get
			{
				return Bounds.size.z;
			}
		}

		public CGBounds()
		{
		}

		public CGBounds(Bounds bounds)
		{
			Bounds = bounds;
		}

		public CGBounds(CGBounds source)
		{
			Bounds = source.Bounds;
		}

		public virtual void RecalculateBounds()
		{
			Bounds = default(Bounds);
			Touch();
		}

		public override T Clone<T>()
		{
			return new CGBounds(this) as T;
		}

		public static void Copy(CGBounds dest, CGBounds source)
		{
			dest.Bounds = source.Bounds;
		}
	}
}
