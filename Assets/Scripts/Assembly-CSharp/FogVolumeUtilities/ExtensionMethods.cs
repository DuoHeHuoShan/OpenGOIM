using UnityEngine;

namespace FogVolumeUtilities
{
	public static class ExtensionMethods
	{
		public static float Remap(this float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		public static bool TimeSnap(int Frames)
		{
			bool flag = true;
			if (Application.isPlaying)
			{
				return Time.frameCount <= 3 || Time.frameCount % (1 + Frames) == 0;
			}
			return true;
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T val = lhs;
			lhs = rhs;
			rhs = val;
		}
	}
}
