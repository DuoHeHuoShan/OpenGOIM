using UnityEngine;

namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	public static class LibTessV3Extension
	{
		public static Vector3 Vector3(this Vec3 v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}
	}
}
