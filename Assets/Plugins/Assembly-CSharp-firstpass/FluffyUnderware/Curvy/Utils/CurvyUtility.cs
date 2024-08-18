using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class CurvyUtility
	{
		public static float ClampTF(float tf, CurvyClamping clamping)
		{
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return Mathf.Repeat(tf, 1f);
			case CurvyClamping.PingPong:
				return Mathf.PingPong(tf, 1f);
			default:
				return Mathf.Clamp01(tf);
			}
		}

		public static float ClampValue(float tf, CurvyClamping clamping, float minTF, float maxTF)
		{
			switch (clamping)
			{
			case CurvyClamping.Loop:
			{
				float t2 = DTMath.MapValue(0f, 1f, tf, minTF, maxTF);
				return DTMath.MapValue(minTF, maxTF, Mathf.Repeat(t2, 1f), 0f);
			}
			case CurvyClamping.PingPong:
			{
				float t = DTMath.MapValue(0f, 1f, tf, minTF, maxTF);
				return DTMath.MapValue(minTF, maxTF, Mathf.PingPong(t, 1f), 0f);
			}
			default:
				return Mathf.Clamp(tf, minTF, maxTF);
			}
		}

		public static float ClampTF(float tf, ref int dir, CurvyClamping clamping)
		{
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return Mathf.Repeat(tf, 1f);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(tf) % 2 != 0)
				{
					dir *= -1;
				}
				return Mathf.PingPong(tf, 1f);
			default:
				return Mathf.Clamp01(tf);
			}
		}

		public static float ClampTF(float tf, ref int dir, CurvyClamping clamping, float minTF, float maxTF)
		{
			minTF = Mathf.Clamp01(minTF);
			maxTF = Mathf.Clamp(maxTF, minTF, 1f);
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return minTF + Mathf.Repeat(tf, maxTF - minTF);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(tf / (maxTF - minTF)) % 2 != 0)
				{
					dir *= -1;
				}
				return minTF + Mathf.PingPong(tf, maxTF - minTF);
			default:
				return Mathf.Clamp(tf, minTF, maxTF);
			}
		}

		public static float ClampDistance(float distance, CurvyClamping clamping, float length)
		{
			if (length == 0f)
			{
				return 0f;
			}
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return Mathf.Repeat(distance, length);
			case CurvyClamping.PingPong:
				return Mathf.PingPong(distance, length);
			default:
				return Mathf.Clamp(distance, 0f, length);
			}
		}

		public static float ClampDistance(float distance, CurvyClamping clamping, float length, float min, float max)
		{
			if (length == 0f)
			{
				return 0f;
			}
			min = Mathf.Clamp(min, 0f, length);
			max = Mathf.Clamp(max, min, length);
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return min + Mathf.Repeat(distance, max - min);
			case CurvyClamping.PingPong:
				return min + Mathf.PingPong(distance, max - min);
			default:
				return Mathf.Clamp(distance, min, max);
			}
		}

		public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length)
		{
			if (length == 0f)
			{
				return 0f;
			}
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return Mathf.Repeat(distance, length);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(distance / length) % 2 != 0)
				{
					dir *= -1;
				}
				return Mathf.PingPong(distance, length);
			default:
				return Mathf.Clamp(distance, 0f, length);
			}
		}

		public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length, float min, float max)
		{
			if (length == 0f)
			{
				return 0f;
			}
			min = Mathf.Clamp(min, 0f, length);
			max = Mathf.Clamp(max, min, length);
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return min + Mathf.Repeat(distance, max - min);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(distance / (max - min)) % 2 != 0)
				{
					dir *= -1;
				}
				return min + Mathf.PingPong(distance, max - min);
			default:
				return Mathf.Clamp(distance, min, max);
			}
		}

		public static Material GetDefaultMaterial()
		{
			Material material = Resources.Load("CurvyDefaultMaterial") as Material;
			if (material == null)
			{
				material = new Material(Shader.Find("Diffuse"));
			}
			return material;
		}
	}
}
