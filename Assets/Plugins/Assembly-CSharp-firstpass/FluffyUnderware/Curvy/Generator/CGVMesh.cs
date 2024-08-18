using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.98f, 0.5f, 0f, 1f)]
	public class CGVMesh : CGBounds
	{
		public Vector3[] Vertex = new Vector3[0];

		public Vector2[] UV = new Vector2[0];

		public Vector2[] UV2 = new Vector2[0];

		public Vector3[] Normal = new Vector3[0];

		public Vector4[] Tangents = new Vector4[0];

		public CGVSubMesh[] SubMeshes = new CGVSubMesh[0];

		public override int Count
		{
			get
			{
				return Vertex.Length;
			}
		}

		public bool HasUV
		{
			get
			{
				return UV.Length > 0;
			}
		}

		public bool HasUV2
		{
			get
			{
				return UV2.Length > 0;
			}
		}

		public bool HasNormals
		{
			get
			{
				return Normal.Length > 0;
			}
		}

		public bool HasTangents
		{
			get
			{
				return Tangents.Length > 0;
			}
		}

		public int TriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < SubMeshes.Length; i++)
				{
					num += SubMeshes[i].Triangles.Length;
				}
				return num / 3;
			}
		}

		public CGVMesh()
			: this(0)
		{
		}

		public CGVMesh(int vertexCount, bool addUV = false, bool addUV2 = false, bool addNormals = false, bool addTangents = false)
		{
			Vertex = new Vector3[vertexCount];
			if (addUV)
			{
				UV = new Vector2[vertexCount];
			}
			if (addUV2)
			{
				UV2 = new Vector2[vertexCount];
			}
			if (addNormals)
			{
				Normal = new Vector3[vertexCount];
			}
			if (addTangents)
			{
				Tangents = new Vector4[vertexCount];
			}
		}

		public CGVMesh(CGVolume volume)
			: this(0)
		{
			Vertex = (Vector3[])volume.Vertex.Clone();
		}

		public CGVMesh(CGVolume volume, IntRegion subset)
			: this(0)
		{
			int sourceIndex = subset.Low * volume.CrossSize;
			int num = (subset.LengthPositive + 1) * volume.CrossSize;
			Vertex = new Vector3[num];
			Array.Copy(volume.Vertex, sourceIndex, Vertex, 0, num);
			Normal = new Vector3[num];
			Array.Copy(volume.VertexNormal, sourceIndex, Normal, 0, num);
		}

		public CGVMesh(CGVMesh source)
			: base(source)
		{
			base.Bounds = source.Bounds;
			Vertex = (Vector3[])source.Vertex.Clone();
			UV = (Vector2[])source.UV.Clone();
			UV2 = (Vector2[])source.UV2.Clone();
			Normal = (Vector3[])source.Normal.Clone();
			Tangents = (Vector4[])source.Tangents.Clone();
			SetSubMeshCount(source.SubMeshes.Length);
			for (int i = 0; i < source.SubMeshes.Length; i++)
			{
				SubMeshes[i] = new CGVSubMesh(source.SubMeshes[i]);
			}
		}

		public CGVMesh(CGMeshProperties meshProperties)
			: this(meshProperties.Mesh, meshProperties.Material, meshProperties.Matrix)
		{
		}

		public CGVMesh(Mesh source, Material[] materials, Matrix4x4 trsMatrix)
		{
			Name = source.name;
			Vertex = (Vector3[])source.vertices.Clone();
			Normal = (Vector3[])source.normals.Clone();
			Tangents = (Vector4[])source.tangents.Clone();
			UV = (Vector2[])source.uv.Clone();
			UV2 = (Vector2[])source.uv2.Clone();
			SetSubMeshCount(source.subMeshCount);
			for (int i = 0; i < source.subMeshCount; i++)
			{
				SubMeshes[i] = new CGVSubMesh(source.GetTriangles(i), (materials.Length <= i) ? null : materials[i]);
			}
			base.Bounds = source.bounds;
			if (!trsMatrix.isIdentity)
			{
				TRS(trsMatrix);
			}
		}

		public override T Clone<T>()
		{
			return new CGVMesh(this) as T;
		}

		public static CGVMesh Get(CGVMesh data, CGVolume source, bool addUV, bool reverseNormals)
		{
			return Get(data, source, new IntRegion(0, source.Count - 1), addUV, reverseNormals);
		}

		public static CGVMesh Get(CGVMesh data, CGVolume source, IntRegion subset, bool addUV, bool reverseNormals)
		{
			if (data == null)
			{
				return new CGVMesh(source, subset);
			}
			int sourceIndex = subset.Low * source.CrossSize;
			int num = (subset.LengthPositive + 1) * source.CrossSize;
			Array.Resize(ref data.Vertex, num);
			Array.Copy(source.Vertex, sourceIndex, data.Vertex, 0, num);
			Array.Resize(ref data.Normal, num);
			if (reverseNormals)
			{
				for (int i = 0; i < data.Normal.Length; i++)
				{
					data.Normal[i] = data.Normal[i] * -1f;
				}
			}
			else
			{
				Array.Copy(source.VertexNormal, data.Normal, num);
			}
			Array.Resize(ref data.UV, addUV ? source.Vertex.Length : 0);
			data.UV2 = new Vector2[0];
			data.Normal = new Vector3[0];
			data.Tangents = new Vector4[0];
			data.Touch();
			return data;
		}

		public void SetSubMeshCount(int count)
		{
			Array.Resize(ref SubMeshes, count);
			Touch();
		}

		public void AddSubMesh(CGVSubMesh submesh = null)
		{
			SubMeshes = SubMeshes.Add(submesh);
			Touch();
		}

		public void MergeVMesh(CGVMesh source)
		{
			int count = Count;
			copyData(ref source.Vertex, ref Vertex, count, source.Count);
			if (HasUV && source.HasUV)
			{
				copyData(ref source.UV, ref UV, count, source.Count);
			}
			if (HasUV2 && source.HasUV2)
			{
				copyData(ref source.UV2, ref UV2, count, source.Count);
			}
			if (HasNormals && source.HasNormals)
			{
				copyData(ref source.Normal, ref Normal, count, source.Count);
			}
			if (HasTangents && source.HasTangents)
			{
				copyData(ref source.Tangents, ref Tangents, count, source.Count);
			}
			for (int i = 0; i < source.SubMeshes.Length; i++)
			{
				GetMaterialSubMesh(source.SubMeshes[i].Material).Add(source.SubMeshes[i], count);
			}
			mBounds = null;
			Touch();
		}

		public void MergeVMesh(CGVMesh source, Matrix4x4 matrix)
		{
			int count = Count;
			Array.Resize(ref Vertex, Count + source.Count);
			int count2 = Count;
			for (int i = count; i < count2; i++)
			{
				Vertex[i] = matrix.MultiplyPoint3x4(source.Vertex[i - count]);
			}
			if (HasUV && source.HasUV)
			{
				copyData(ref source.UV, ref UV, count, source.Count);
			}
			if (HasUV2 && source.HasUV2)
			{
				copyData(ref source.UV2, ref UV2, count, source.Count);
			}
			if (HasNormals && source.HasNormals)
			{
				copyData(ref source.Normal, ref Normal, count, source.Count);
			}
			if (HasTangents && source.HasTangents)
			{
				copyData(ref source.Tangents, ref Tangents, count, source.Count);
			}
			for (int j = 0; j < source.SubMeshes.Length; j++)
			{
				GetMaterialSubMesh(source.SubMeshes[j].Material).Add(source.SubMeshes[j], count);
			}
			mBounds = null;
			Touch();
		}

		public CGVSubMesh GetMaterialSubMesh(Material mat, bool createIfMissing = true)
		{
			for (int i = 0; i < SubMeshes.Length; i++)
			{
				if (SubMeshes[i].Material == mat)
				{
					return SubMeshes[i];
				}
			}
			if (createIfMissing)
			{
				CGVSubMesh cGVSubMesh = new CGVSubMesh(mat);
				AddSubMesh(cGVSubMesh);
				return cGVSubMesh;
			}
			return null;
		}

		public Mesh AsMesh()
		{
			Mesh msh = new Mesh();
			ToMesh(ref msh);
			return msh;
		}

		public void ToMesh(ref Mesh msh)
		{
			msh.vertices = Vertex;
			if (HasUV)
			{
				msh.uv = UV;
			}
			if (HasUV2)
			{
				msh.uv2 = UV2;
			}
			if (HasNormals)
			{
				msh.normals = Normal;
			}
			if (HasTangents)
			{
				msh.tangents = Tangents;
			}
			msh.subMeshCount = SubMeshes.Length;
			for (int i = 0; i < SubMeshes.Length; i++)
			{
				msh.SetTriangles(SubMeshes[i].Triangles, i);
			}
		}

		public Material[] GetMaterials()
		{
			List<Material> list = new List<Material>();
			for (int i = 0; i < SubMeshes.Length; i++)
			{
				list.Add(SubMeshes[i].Material);
			}
			return list.ToArray();
		}

		public override void RecalculateBounds()
		{
			if (Count == 0)
			{
				mBounds = new Bounds(Vector3.zero, Vector3.zero);
				return;
			}
			Bounds value = new Bounds(Vertex[0], Vector3.zero);
			int num = Vertex.Length;
			for (int i = 1; i < num; i++)
			{
				value.Encapsulate(Vertex[i]);
			}
			mBounds = value;
			Touch();
		}

		public void RecalculateUV2()
		{
			UV2 = CGUtility.CalculateUV2(UV);
		}

		public void TRS(Matrix4x4 matrix)
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				Vertex[i] = matrix.MultiplyPoint3x4(Vertex[i]);
			}
			mBounds = null;
			Touch();
		}

		private void copyData<T>(ref T[] src, ref T[] dst, int currentSize, int extraSize)
		{
			Array.Resize(ref dst, currentSize + extraSize);
			Array.Copy(src, 0, dst, currentSize, src.Length);
			Touch();
		}
	}
}
