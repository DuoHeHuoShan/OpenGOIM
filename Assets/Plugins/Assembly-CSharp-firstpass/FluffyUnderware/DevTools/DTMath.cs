using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTMath
	{
		public static Vector3 ParallelTransportFrame(ref Vector3 up, ref Vector3 tan0, ref Vector3 tan1)
		{
			Vector3 vector = Vector3.Cross(tan0, tan1);
			float num = Mathf.Atan2(vector.magnitude, Vector3.Dot(tan0, tan1));
			return Quaternion.AngleAxis(57.29578f * num, vector.normalized) * up;
		}

		public static Vector3 LeftTan(ref Vector3 tan, ref Vector3 up)
		{
			return Vector3.Cross(tan, up);
		}

		public static Vector3 RightTan(ref Vector3 tan, ref Vector3 up)
		{
			return Vector3.Cross(up, tan);
		}

		public static float Repeat(float t, float length)
		{
			return (t != length) ? (t - Mathf.Floor(t / length) * length) : t;
		}

		public static double FixNaN(double v)
		{
			if (double.IsNaN(v))
			{
				v = 0.0;
			}
			return v;
		}

		public static float FixNaN(float v)
		{
			if (float.IsNaN(v))
			{
				v = 0f;
			}
			return v;
		}

		public static Vector2 FixNaN(Vector2 v)
		{
			if (float.IsNaN(v.x))
			{
				v.x = 0f;
			}
			if (float.IsNaN(v.y))
			{
				v.y = 0f;
			}
			return v;
		}

		public static Vector3 FixNaN(Vector3 v)
		{
			if (float.IsNaN(v.x))
			{
				v.x = 0f;
			}
			if (float.IsNaN(v.y))
			{
				v.y = 0f;
			}
			if (float.IsNaN(v.z))
			{
				v.z = 0f;
			}
			return v;
		}

		public static float MapValue(float min, float max, float value, float vMin = -1f, float vMax = 1f)
		{
			return min + (max - min) * (value - vMin) / (vMax - vMin);
		}

		public static float SnapPrecision(float value, int decimals)
		{
			return (decimals < 0) ? value : ((float)Math.Round(value, decimals));
		}

		public static Vector2 SnapPrecision(Vector2 value, int decimals)
		{
			if (decimals < 0)
			{
				return value;
			}
			value.Set(SnapPrecision(value.x, decimals), SnapPrecision(value.y, decimals));
			return value;
		}

		public static Vector3 SnapPrecision(Vector3 value, int decimals)
		{
			if (decimals < 0)
			{
				return value;
			}
			value.Set(SnapPrecision(value.x, decimals), SnapPrecision(value.y, decimals), SnapPrecision(value.z, decimals));
			return value;
		}

		public static float LinePointDistanceSqr(Vector3 l1, Vector3 l2, Vector3 p, out float frag)
		{
			Vector3 vector = l2 - l1;
			Vector3 lhs = p - l1;
			float num = Vector3.Dot(lhs, vector);
			if (num <= 0f)
			{
				frag = 0f;
				return (p - l1).sqrMagnitude;
			}
			float num2 = Vector3.Dot(vector, vector);
			if (num2 <= num)
			{
				frag = 1f;
				return (p - l2).sqrMagnitude;
			}
			frag = num / num2;
			Vector3 vector2 = l1 + frag * vector;
			return (p - vector2).sqrMagnitude;
		}

		public static bool RayLineSegmentIntersection(Vector2 r0, Vector2 dir, Vector2 l1, Vector2 l2, out Vector2 hit, out float frag)
		{
			Vector2 vector = l2 - l1;
			frag = ((0f - dir.y) * (r0.x - l1.x) + dir.x * (r0.y - l1.y)) / ((0f - vector.x) * dir.y + dir.x * vector.y);
			float num = (vector.x * (r0.y - l1.y) - vector.y * (r0.x - l1.x)) / ((0f - vector.x) * dir.y + dir.x * vector.y);
			if (frag >= 0f && frag <= 1f && num > 0f)
			{
				hit = new Vector2(r0.x + num * dir.x, r0.y + num * dir.y);
				return true;
			}
			hit = Vector2.zero;
			return false;
		}

		public static bool ShortestIntersectionLine(Vector3 line1A, Vector3 line1B, Vector3 line2A, Vector3 line2B, out Vector3 resultSegmentA, out Vector3 resultSegmentB)
		{
			resultSegmentA = Vector3.zero;
			resultSegmentB = Vector3.zero;
			Vector3 vector = line1A;
			Vector3 vector2 = line2A;
			Vector3 vector3 = vector - vector2;
			Vector3 vector4 = line2B - vector2;
			if (vector4.sqrMagnitude < Mathf.Epsilon)
			{
				return false;
			}
			Vector3 vector5 = line1B - vector;
			if (vector5.sqrMagnitude < Mathf.Epsilon)
			{
				return false;
			}
			double num = (double)vector3.x * (double)vector4.x + (double)vector3.y * (double)vector4.y + (double)vector3.z * (double)vector4.z;
			double num2 = (double)vector4.x * (double)vector5.x + (double)vector4.y * (double)vector5.y + (double)vector4.z * (double)vector5.z;
			double num3 = (double)vector3.x * (double)vector5.x + (double)vector3.y * (double)vector5.y + (double)vector3.z * (double)vector5.z;
			double num4 = (double)vector4.x * (double)vector4.x + (double)vector4.y * (double)vector4.y + (double)vector4.z * (double)vector4.z;
			double num5 = (double)vector5.x * (double)vector5.x + (double)vector5.y * (double)vector5.y + (double)vector5.z * (double)vector5.z;
			double num6 = num5 * num4 - num2 * num2;
			if (Math.Abs(num6) < double.Epsilon)
			{
				return false;
			}
			double num7 = num * num2 - num3 * num4;
			double num8 = num7 / num6;
			double num9 = (num + num2 * num8) / num4;
			resultSegmentA = new Vector3((float)((double)vector.x + num8 * (double)vector5.x), (float)((double)vector.y + num8 * (double)vector5.y), (float)((double)vector.z + num8 * (double)vector5.z));
			resultSegmentB = new Vector3((float)((double)vector2.x + num9 * (double)vector4.x), (float)((double)vector2.y + num9 * (double)vector4.y), (float)((double)vector2.z + num9 * (double)vector4.z));
			return true;
		}

		public static bool LineLineIntersection(Vector3 line1A, Vector3 line1B, Vector3 line2A, Vector3 line2B, out Vector3 hitPoint)
		{
			Vector3 resultSegmentB;
			if (ShortestIntersectionLine(line1A, line1B, line2A, line2B, out hitPoint, out resultSegmentB) && (resultSegmentB - hitPoint).sqrMagnitude <= Mathf.Epsilon * Mathf.Epsilon)
			{
				return true;
			}
			return false;
		}

		public static bool LineLineIntersect(Vector2 line1A, Vector2 line1B, Vector2 line2A, Vector2 line2B, out Vector2 hitPoint, bool segmentOnly = true)
		{
			hitPoint = Vector2.zero;
			double num = (line2B.y - line2A.y) * (line1B.x - line1A.x) - (line2B.x - line2A.x) * (line1B.y - line1A.y);
			double num2 = (line2B.x - line2A.x) * (line1A.y - line2A.y) - (line2B.y - line2A.y) * (line1A.x - line2A.x);
			double num3 = (line1B.x - line1A.x) * (line1A.y - line2A.y) - (line1B.y - line1A.y) * (line1A.x - line2A.x);
			if (num == 0.0)
			{
				return false;
			}
			double num4 = num2 / num;
			double num5 = num3 / num;
			if (!segmentOnly || (num4 >= 0.0 && num4 <= 1.0 && num5 >= 0.0 && num5 <= 1.0))
			{
				hitPoint.Set((float)((double)line1A.x + num4 * (double)(line1B.x - line1A.x)), (float)((double)line1A.y + num4 * (double)(line1B.y - line1A.y)));
				return true;
			}
			return false;
		}

		public static bool PointInsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 p, out float ac, out float ab, bool edgesAllowed)
		{
			Vector3 vector = C - A;
			Vector3 vector2 = B - A;
			Vector3 rhs = p - A;
			float num = Vector3.Dot(vector, vector);
			float num2 = Vector3.Dot(vector, vector2);
			float num3 = Vector3.Dot(vector, rhs);
			float num4 = Vector3.Dot(vector2, vector2);
			float num5 = Vector3.Dot(vector2, rhs);
			float num6 = 1f / (num * num4 - num2 * num2);
			ac = (num4 * num3 - num2 * num5) * num6;
			ab = (num * num5 - num2 * num3) * num6;
			if (edgesAllowed)
			{
				return ac >= 0f && ab >= 0f && ac + ab < 1f;
			}
			return ac > 0f && ab > 0f && ac + ab < 1f;
		}
	}
}
