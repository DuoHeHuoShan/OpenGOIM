using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class TransformExt
	{
		public static void CopyFrom(this Transform t, Transform other)
		{
			t.position = other.position;
			t.rotation = other.rotation;
			t.localScale = other.localScale;
		}

		public static void SetChildrenHideFlags(this Transform t, HideFlags flags)
		{
			if (t != null)
			{
				int childCount = t.childCount;
				for (int i = 0; i < childCount; i++)
				{
					t.GetChild(i).hideFlags = flags;
				}
			}
		}
	}
}
