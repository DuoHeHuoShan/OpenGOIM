using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	public static class UnityLibTessUtility
	{
		public static ContourVertex[] ToContourVertex(Vector3[] v, bool zeroZ = false)
		{
			ContourVertex[] array = new ContourVertex[v.Length];
			for (int i = 0; i < v.Length; i++)
			{
				array[i].Position.X = v[i].x;
				array[i].Position.Y = v[i].y;
				array[i].Position.Z = ((!zeroZ) ? v[i].z : 0f);
			}
			return array;
		}

		public static Vector3[] FromContourVertex(ContourVertex[] v)
		{
			Vector3[] v3Array = new Vector3[v.Length];
			SetFromContourVertex(ref v3Array, ref v);
			return v3Array;
		}

		public static void SetFromContourVertex(ref Vector3[] v3Array, ref ContourVertex[] cvArray)
		{
			Array.Resize(ref v3Array, cvArray.Length);
			for (int i = 0; i < v3Array.Length; i++)
			{
				v3Array[i].x = cvArray[i].Position.X;
				v3Array[i].y = cvArray[i].Position.Y;
				v3Array[i].z = cvArray[i].Position.Z;
			}
		}

		public static void SetToContourVertex(ref ContourVertex[] cvArray, ref Vector3[] v3Array)
		{
			Array.Resize(ref cvArray, v3Array.Length);
			for (int i = 0; i < cvArray.Length; i++)
			{
				cvArray[i].Position.X = v3Array[i].x;
				cvArray[i].Position.Y = v3Array[i].y;
				cvArray[i].Position.Z = v3Array[i].z;
			}
		}
	}
}
