using System;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class Vector2Ext
	{
		public static Vector2 Snap(this Vector2 v, float snapX, float snapY = -1f)
		{
			if (snapY == -1f)
			{
				snapY = snapX;
			}
			return new Vector2(v.x - v.x % snapX, v.y - v.y % snapY);
		}

		public static float AngleSigned(this Vector2 a, Vector2 b)
		{
			float num = Mathf.Sign(a.x * b.y - a.y * b.x);
			return Vector2.Angle(a, b) * num;
		}

		public static Vector2 LeftNormal(this Vector2 v)
		{
			return new Vector2(0f - v.y, v.x);
		}

		public static Vector2 RightNormal(this Vector2 v)
		{
			return new Vector2(v.y, 0f - v.x);
		}

		public static Vector2 Rotate(this Vector2 v, float degree)
		{
			float f = degree * ((float)Math.PI / 180f);
			float num = Mathf.Cos(f);
			float num2 = Mathf.Sin(f);
			return new Vector2(num * v.x - num2 * v.y, num2 * v.x + num * v.y);
		}

		public static Vector2 ToVector3(this Vector2 v)
		{
			return new Vector3(v.x, v.y, 0f);
		}
	}
}
