using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class Vector3Ext
	{
		public static float AngleSigned(this Vector3 a, Vector3 b, Vector3 normal)
		{
			return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(a, b)), Vector3.Dot(a, b)) * 57.29578f;
		}

		public static Vector3 RotateAround(this Vector3 point, Vector3 origin, Quaternion rotation)
		{
			Vector3 vector = point - origin;
			vector = rotation * vector;
			return origin + vector;
		}

		public static Vector2 ToVector2(this Vector3 v)
		{
			return new Vector2(v.x, v.y);
		}
	}
}
