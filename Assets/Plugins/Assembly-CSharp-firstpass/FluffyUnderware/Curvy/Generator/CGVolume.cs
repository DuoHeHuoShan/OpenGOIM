using System;
using FluffyUnderware.Curvy.Utils;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.08f, 0.4f, 0.75f, 1f)]
	public class CGVolume : CGPath
	{
		public Vector3[] Vertex = new Vector3[0];

		public Vector3[] VertexNormal = new Vector3[0];

		public float[] CrossF = new float[0];

		public float[] CrossMap = new float[0];

		public float[] SegmentLength;

		public bool CrossClosed;

		public bool CrossSeamless;

		public float CrossFShift;

		public SamplePointsMaterialGroupCollection CrossMaterialGroups;

		public int CrossSize
		{
			get
			{
				return CrossF.Length;
			}
		}

		public int VertexCount
		{
			get
			{
				return Vertex.Length;
			}
		}

		public CGVolume()
		{
		}

		public CGVolume(int samplePoints, CGShape crossShape)
		{
			CrossF = (float[])crossShape.F.Clone();
			CrossMap = (float[])crossShape.Map.Clone();
			CrossClosed = crossShape.Closed;
			CrossSeamless = crossShape.Seamless;
			CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
			SegmentLength = new float[Count];
			Vertex = new Vector3[CrossSize * samplePoints];
			VertexNormal = new Vector3[Vertex.Length];
		}

		public CGVolume(CGPath path, CGShape crossShape)
			: base(path)
		{
			CrossF = (float[])crossShape.F.Clone();
			CrossMap = (float[])crossShape.Map.Clone();
			SegmentLength = new float[Count];
			CrossClosed = crossShape.Closed;
			CrossSeamless = crossShape.Seamless;
			CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
			Vertex = new Vector3[CrossSize * Count];
			VertexNormal = new Vector3[Vertex.Length];
		}

		public CGVolume(CGVolume source)
			: base(source)
		{
			Vertex = (Vector3[])source.Vertex.Clone();
			VertexNormal = (Vector3[])source.VertexNormal.Clone();
			CrossF = (float[])source.CrossF.Clone();
			CrossMap = (float[])source.CrossMap.Clone();
			SegmentLength = new float[Count];
			CrossClosed = source.Closed;
			CrossSeamless = source.CrossSeamless;
			CrossFShift = source.CrossFShift;
			CrossMaterialGroups = new SamplePointsMaterialGroupCollection(source.CrossMaterialGroups);
		}

		public static CGVolume Get(CGVolume data, CGPath path, CGShape crossShape)
		{
			if (data == null)
			{
				return new CGVolume(path, crossShape);
			}
			CGPath.Copy(data, path);
			Array.Resize(ref data.SegmentLength, data.CrossF.Length);
			data.SegmentLength = new float[data.Count];
			Array.Resize(ref data.CrossF, crossShape.F.Length);
			crossShape.F.CopyTo(data.CrossF, 0);
			Array.Resize(ref data.CrossMap, crossShape.Map.Length);
			crossShape.Map.CopyTo(data.CrossMap, 0);
			data.CrossClosed = crossShape.Closed;
			data.CrossSeamless = crossShape.Seamless;
			data.CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
			Array.Resize(ref data.Vertex, data.CrossSize * data.Position.Length);
			Array.Resize(ref data.VertexNormal, data.Vertex.Length);
			return data;
		}

		public override T Clone<T>()
		{
			return new CGVolume(this) as T;
		}

		public void InterpolateVolume(float f, float crossF, out Vector3 pos, out Vector3 dir, out Vector3 up)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			Vector3 vector = Vertex[vertexIndex];
			Vector3 vector2 = Vertex[vertexIndex + 1];
			Vector3 vector3 = Vertex[vertexIndex + CrossSize];
			Vector3 vector5;
			Vector3 vector6;
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector4 = Vertex[vertexIndex + CrossSize + 1];
				vector5 = vector4 - vector3;
				vector6 = vector4 - vector2;
				pos = vector3 - vector6 * (1f - pathFrag) + vector5 * crossFrag;
			}
			else
			{
				vector5 = vector2 - vector;
				vector6 = vector3 - vector;
				pos = vector + vector6 * pathFrag + vector5 * crossFrag;
			}
			dir = vector6.normalized;
			up = Vector3.Cross(vector6, vector5);
		}

		public Vector3 InterpolateVolumePosition(float f, float crossF)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			Vector3 vector = Vertex[vertexIndex];
			Vector3 vector2 = Vertex[vertexIndex + 1];
			Vector3 vector3 = Vertex[vertexIndex + CrossSize];
			Vector3 vector5;
			Vector3 vector6;
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector4 = Vertex[vertexIndex + CrossSize + 1];
				vector5 = vector4 - vector3;
				vector6 = vector4 - vector2;
				return vector3 - vector6 * (1f - pathFrag) + vector5 * crossFrag;
			}
			vector5 = vector2 - vector;
			vector6 = vector3 - vector;
			return vector + vector6 * pathFrag + vector5 * crossFrag;
		}

		public Vector3 InterpolateVolumeDirection(float f, float crossF)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector = Vertex[vertexIndex + 1];
				Vector3 vector2 = Vertex[vertexIndex + CrossSize + 1];
				return (vector2 - vector).normalized;
			}
			Vector3 vector3 = Vertex[vertexIndex];
			Vector3 vector4 = Vertex[vertexIndex + CrossSize];
			return (vector4 - vector3).normalized;
		}

		public Vector3 InterpolateVolumeUp(float f, float crossF)
		{
			float pathFrag;
			float crossFrag;
			int vertexIndex = GetVertexIndex(f, crossF, out pathFrag, out crossFrag);
			Vector3 vector = Vertex[vertexIndex + 1];
			Vector3 vector2 = Vertex[vertexIndex + CrossSize];
			Vector3 rhs;
			Vector3 lhs;
			if (pathFrag + crossFrag > 1f)
			{
				Vector3 vector3 = Vertex[vertexIndex + CrossSize + 1];
				rhs = vector3 - vector2;
				lhs = vector3 - vector;
			}
			else
			{
				Vector3 vector4 = Vertex[vertexIndex];
				rhs = vector - vector4;
				lhs = vector2 - vector4;
			}
			return Vector3.Cross(lhs, rhs);
		}

		public float GetCrossLength(float pathF)
		{
			int s0Index;
			int s1Index;
			float frag;
			GetSegmentIndices(pathF, out s0Index, out s1Index, out frag);
			if (SegmentLength[s0Index] == 0f)
			{
				SegmentLength[s0Index] = calcSegmentLength(s0Index);
			}
			if (SegmentLength[s1Index] == 0f)
			{
				SegmentLength[s1Index] = calcSegmentLength(s1Index);
			}
			return Mathf.Lerp(SegmentLength[s0Index], SegmentLength[s1Index], frag);
		}

		public float CrossFToDistance(float f, float crossF, CurvyClamping crossClamping = CurvyClamping.Clamp)
		{
			return GetCrossLength(f) * CurvyUtility.ClampTF(crossF, crossClamping);
		}

		public float CrossDistanceToF(float f, float distance, CurvyClamping crossClamping = CurvyClamping.Clamp)
		{
			float crossLength = GetCrossLength(f);
			return CurvyUtility.ClampDistance(distance, crossClamping, crossLength) / crossLength;
		}

		public void GetSegmentIndices(float pathF, out int s0Index, out int s1Index, out float frag)
		{
			s0Index = GetFIndex(Mathf.Repeat(pathF, 1f), out frag);
			s1Index = s0Index + 1;
		}

		public int GetSegmentIndex(int segment)
		{
			return segment * CrossSize;
		}

		public int GetCrossFIndex(float crossF, out float frag)
		{
			float num = crossF + CrossFShift;
			if (num != 1f)
			{
				return getGenericFIndex(ref CrossF, Mathf.Repeat(num, 1f), out frag);
			}
			return getGenericFIndex(ref CrossF, num, out frag);
		}

		public int GetVertexIndex(float pathF, out float pathFrag)
		{
			int fIndex = GetFIndex(pathF, out pathFrag);
			return fIndex * CrossSize;
		}

		public int GetVertexIndex(float pathF, float crossF, out float pathFrag, out float crossFrag)
		{
			int vertexIndex = GetVertexIndex(pathF, out pathFrag);
			int crossFIndex = GetCrossFIndex(crossF, out crossFrag);
			return vertexIndex + crossFIndex;
		}

		public Vector3[] GetSegmentVertices(params int[] segmentIndices)
		{
			Vector3[] array = new Vector3[CrossSize * segmentIndices.Length];
			for (int i = 0; i < segmentIndices.Length; i++)
			{
				Array.Copy(Vertex, segmentIndices[i] * CrossSize, array, i * CrossSize, CrossSize);
			}
			return array;
		}

		private float calcSegmentLength(int segmentIndex)
		{
			int num = segmentIndex * CrossSize;
			int num2 = num + CrossSize - 1;
			float num3 = 0f;
			for (int i = num; i < num2; i++)
			{
				num3 += (Vertex[i + 1] - Vertex[i]).magnitude;
			}
			return num3;
		}
	}
}
