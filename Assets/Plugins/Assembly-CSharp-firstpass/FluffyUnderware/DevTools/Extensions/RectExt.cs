using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class RectExt
	{
		public static Rect Set(this Rect rect, Vector2 pos, Vector2 size)
		{
			rect.Set(pos.x, pos.y, size.x, size.y);
			return new Rect(rect);
		}

		public static Rect SetBetween(this Rect rect, Vector2 pos, Vector2 pos2)
		{
			rect.Set(pos.x, pos.y, pos2.x - pos.x, pos2.y - pos.y);
			return new Rect(rect);
		}

		public static Rect SetPosition(this Rect rect, Vector2 pos)
		{
			rect.x = pos.x;
			rect.y = pos.y;
			return new Rect(rect);
		}

		public static Rect SetPosition(this Rect rect, float x, float y)
		{
			rect.x = x;
			rect.y = y;
			return new Rect(rect);
		}

		public static Vector2 GetSize(this Rect rect)
		{
			return new Vector2(rect.width, rect.height);
		}

		public static Rect SetSize(this Rect rect, Vector2 size)
		{
			rect.width = size.x;
			rect.height = size.y;
			return new Rect(rect);
		}

		public static Rect ScaleBy(this Rect rect, int pixel)
		{
			return rect.ScaleBy(pixel, pixel);
		}

		public static Rect ScaleBy(this Rect rect, int x, int y)
		{
			rect.x -= x;
			rect.y -= y;
			rect.width += (float)x * 2f;
			rect.height += (float)y * 2f;
			return new Rect(rect);
		}

		public static Rect ShiftBy(this Rect rect, int x, int y)
		{
			rect.x += x;
			rect.y += y;
			return new Rect(rect);
		}

		public static Rect Include(this Rect rect, Rect other)
		{
			Rect result = default(Rect);
			result.xMin = Mathf.Min(rect.xMin, other.xMin);
			result.xMax = Mathf.Max(rect.xMax, other.xMax);
			result.yMin = Mathf.Min(rect.yMin, other.yMin);
			result.yMax = Mathf.Max(rect.yMax, other.yMax);
			return result;
		}
	}
}
