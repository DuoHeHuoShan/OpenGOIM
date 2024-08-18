using UnityEngine;

namespace MeshCombineStudio
{
	public static class Mathw
	{
		public static bool IntersectAABB3Sphere3(AABB3 box, Sphere3 sphere)
		{
			Vector3 center = sphere.center;
			Vector3 min = box.min;
			Vector3 max = box.max;
			float num = 0f;
			if (center.x < min.x)
			{
				float num2 = center.x - min.x;
				num += num2 * num2;
			}
			else if (center.x > max.x)
			{
				float num2 = center.x - max.x;
				num += num2 * num2;
			}
			if (center.y < min.y)
			{
				float num2 = center.y - min.y;
				num += num2 * num2;
			}
			else if (center.y > max.y)
			{
				float num2 = center.y - max.y;
				num += num2 * num2;
			}
			if (center.z < min.z)
			{
				float num2 = center.z - min.z;
				num += num2 * num2;
			}
			else if (center.z > max.z)
			{
				float num2 = center.z - max.z;
				num += num2 * num2;
			}
			return num <= sphere.radius * sphere.radius;
		}
	}
}
