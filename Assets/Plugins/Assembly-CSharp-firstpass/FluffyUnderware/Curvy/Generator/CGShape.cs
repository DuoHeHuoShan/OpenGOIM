using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.73f, 0.87f, 0.98f, 1f)]
	public class CGShape : CGData
	{
		public float[] SourceF = new float[0];

		public float[] F = new float[0];

		public Vector3[] Position = new Vector3[0];

		public Vector3[] Normal = new Vector3[0];

		public float[] Map = new float[0];

		public Bounds Bounds;

		public List<SamplePointsMaterialGroup> MaterialGroups = new List<SamplePointsMaterialGroup>();

		public bool SourceIsManaged;

		public bool Closed;

		public bool Seamless;

		public float Length;

		private float mCacheLastF = float.MaxValue;

		private int mCacheLastIndex;

		private float mCacheLastFrag;

		public override int Count
		{
			get
			{
				return F.Length;
			}
		}

		public CGShape()
		{
		}

		public CGShape(CGShape source)
		{
			Position = (Vector3[])source.Position.Clone();
			Normal = (Vector3[])source.Normal.Clone();
			Map = (float[])source.Map.Clone();
			F = (float[])source.F.Clone();
			SourceF = (float[])source.SourceF.Clone();
			MaterialGroups = new List<SamplePointsMaterialGroup>(source.MaterialGroups);
			Closed = source.Closed;
			Seamless = source.Seamless;
			Length = source.Length;
			Bounds = source.Bounds;
			SourceIsManaged = source.SourceIsManaged;
		}

		public override T Clone<T>()
		{
			return new CGShape(this) as T;
		}

		public static void Copy(CGShape dest, CGShape source)
		{
			Array.Resize(ref dest.Position, source.Position.Length);
			source.Position.CopyTo(dest.Position, 0);
			Array.Resize(ref dest.Normal, source.Normal.Length);
			source.Normal.CopyTo(dest.Normal, 0);
			Array.Resize(ref dest.Map, source.Map.Length);
			source.Map.CopyTo(dest.Map, 0);
			Array.Resize(ref dest.F, source.F.Length);
			source.F.CopyTo(dest.F, 0);
			Array.Resize(ref dest.SourceF, source.SourceF.Length);
			source.SourceF.CopyTo(dest.SourceF, 0);
			dest.MaterialGroups = new List<SamplePointsMaterialGroup>(source.MaterialGroups);
			dest.Bounds = source.Bounds;
			dest.Closed = source.Closed;
			dest.Seamless = source.Seamless;
			dest.Length = source.Length;
			dest.Touch();
		}

		public float DistanceToF(float distance)
		{
			return Mathf.Clamp(distance, 0f, Length) / Length;
		}

		public float FToDistance(float f)
		{
			return Mathf.Clamp01(f) * Length;
		}

		public int GetFIndex(float f, out float frag)
		{
			if (mCacheLastF != f)
			{
				mCacheLastF = f;
				mCacheLastIndex = getGenericFIndex(ref F, f, out mCacheLastFrag);
			}
			frag = mCacheLastFrag;
			return mCacheLastIndex;
		}

		public Vector3 InterpolatePosition(float f)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			return Vector3.Lerp(Position[fIndex], Position[fIndex + 1], frag);
		}

		public void Move(ref float f, ref int direction, float speed, CurvyClamping clamping)
		{
			f = CurvyUtility.ClampTF(f + speed, ref direction, clamping);
		}

		public void MoveBy(ref float f, ref int direction, float speedDist, CurvyClamping clamping)
		{
			float distance = CurvyUtility.ClampDistance(FToDistance(f) + speedDist * (float)direction, ref direction, clamping, Length);
			f = DistanceToF(distance);
		}

		public virtual void Recalculate()
		{
			Length = 0f;
			float[] array = new float[Count];
			for (int i = 1; i < Count; i++)
			{
				array[i] = array[i - 1] + (Position[i] - Position[i - 1]).magnitude;
			}
			if (Count > 0)
			{
				Length = array[Count - 1];
				if (Length > 0f)
				{
					F[0] = 0f;
					for (int j = 1; j < Count - 1; j++)
					{
						F[j] = array[j] / Length;
					}
					F[Count - 1] = 1f;
				}
				else
				{
					F = new float[Count];
				}
			}
			Touch();
		}

		public void RecalculateNormals(List<int> softEdges)
		{
			if (Normal.Length != Position.Length)
			{
				Array.Resize(ref Normal, Position.Length);
			}
			for (int i = 0; i < MaterialGroups.Count; i++)
			{
				for (int j = 0; j < MaterialGroups[i].Patches.Count; j++)
				{
					SamplePointsPatch samplePointsPatch = MaterialGroups[i].Patches[j];
					Vector3 normalized;
					for (int k = 0; k < samplePointsPatch.Count; k++)
					{
						int num = samplePointsPatch.Start + k;
						normalized = (Position[num + 1] - Position[num]).normalized;
						Normal[num] = new Vector3(0f - normalized.y, normalized.x, 0f);
					}
					normalized = (Position[samplePointsPatch.End] - Position[samplePointsPatch.End - 1]).normalized;
					Normal[samplePointsPatch.End] = new Vector3(0f - normalized.y, normalized.x, 0f);
				}
			}
			for (int l = 0; l < softEdges.Count; l++)
			{
				int num2 = softEdges[l] - 1;
				if (num2 < 0)
				{
					num2 = Position.Length - 1;
				}
				int num3 = num2 - 1;
				int num4 = softEdges[l] + 1;
				if (num4 == Position.Length)
				{
					num4 = 0;
				}
				Normal[softEdges[l]] = Vector3.Slerp(Normal[num3], Normal[num4], 0.5f);
				Normal[num2] = Normal[softEdges[l]];
			}
		}
	}
}
