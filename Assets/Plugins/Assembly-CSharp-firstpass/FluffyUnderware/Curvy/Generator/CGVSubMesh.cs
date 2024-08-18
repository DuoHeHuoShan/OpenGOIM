using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGVSubMesh : CGData
	{
		public int[] Triangles = new int[0];

		public Material Material;

		public override int Count
		{
			get
			{
				return Triangles.Length;
			}
		}

		public CGVSubMesh(Material material = null)
		{
			Material = material;
		}

		public CGVSubMesh(int[] triangles, Material material = null)
			: this(material)
		{
			Triangles = triangles;
		}

		public CGVSubMesh(int triangleCount, Material material = null)
			: this(material)
		{
			Triangles = new int[triangleCount];
		}

		public CGVSubMesh(CGVSubMesh source)
		{
			Triangles = (int[])source.Triangles.Clone();
			Material = source.Material;
		}

		public override T Clone<T>()
		{
			return new CGVSubMesh(this) as T;
		}

		public static CGVSubMesh Get(CGVSubMesh data, int triangleCount, Material material = null)
		{
			if (data == null)
			{
				return new CGVSubMesh(triangleCount, material);
			}
			Array.Resize(ref data.Triangles, triangleCount);
			data.Material = material;
			data.Touch();
			return data;
		}

		public void ShiftIndices(int offset, int startIndex = 0)
		{
			for (int i = startIndex; i < Triangles.Length; i++)
			{
				Triangles[i] += offset;
			}
			Touch();
		}

		public void Add(CGVSubMesh other, int shiftIndexOffset = 0)
		{
			int num = Triangles.Length;
			Array.Resize(ref Triangles, Triangles.Length + other.Triangles.Length);
			Array.Copy(other.Triangles, 0, Triangles, num, other.Triangles.Length);
			if (shiftIndexOffset != 0)
			{
				ShiftIndices(shiftIndexOffset, num);
			}
			Touch();
		}
	}
}
