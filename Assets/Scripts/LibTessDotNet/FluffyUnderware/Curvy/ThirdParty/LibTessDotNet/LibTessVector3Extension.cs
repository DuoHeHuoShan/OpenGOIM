using UnityEngine;

namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	public static class LibTessVector3Extension
	{
		public static Vec3 Vec3(this Vector3 v)
		{
			Vec3 result = default(Vec3);
			result.X = v.x;
			result.Y = v.y;
			result.Z = v.z;
			return result;
		}

		public static ContourVertex ContourVertex(this Vector3 v)
		{
			ContourVertex result = default(ContourVertex);
			result.Position = v.Vec3();
			return result;
		}
	}
}
