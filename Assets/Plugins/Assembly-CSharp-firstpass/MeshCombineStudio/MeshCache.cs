using System;
using UnityEngine;

namespace MeshCombineStudio
{
	public class MeshCache
	{
		public class SubMeshCache
		{
			public Vector3[] vertices;

			public Vector3[] normals;

			public Vector4[] tangents;

			public Vector2[] uv;

			public Vector2[] uv2;

			public Vector2[] uv3;

			public Vector2[] uv4;

			public Color32[] colors32;

			public int[] triangles;

			public bool hasNormals;

			public bool hasTangents;

			public bool hasUv;

			public bool hasUv2;

			public bool hasUv3;

			public bool hasUv4;

			public bool hasColors;

			public int vertexCount;

			public int triangleCount;

			public SubMeshCache()
			{
			}

			public SubMeshCache(Mesh mesh, int subMeshIndex)
			{
				triangles = mesh.GetTriangles(subMeshIndex);
				triangleCount = triangles.Length;
			}

			public SubMeshCache(Mesh mesh, bool assignTriangles)
			{
				vertices = mesh.vertices;
				normals = mesh.normals;
				tangents = mesh.tangents;
				uv = mesh.uv;
				uv2 = mesh.uv2;
				uv3 = mesh.uv3;
				uv4 = mesh.uv4;
				colors32 = mesh.colors32;
				if (assignTriangles)
				{
					triangles = mesh.triangles;
					triangleCount = triangles.Length;
				}
				CheckHasArrays();
				vertexCount = vertices.Length;
			}

			public void CopySubMeshCache(SubMeshCache source)
			{
				vertexCount = source.vertexCount;
				Array.Copy(source.vertices, 0, vertices, 0, vertexCount);
				hasNormals = source.hasNormals;
				hasTangents = source.hasTangents;
				hasColors = source.hasColors;
				hasUv = source.hasUv;
				hasUv2 = source.hasUv2;
				hasUv3 = source.hasUv3;
				hasUv4 = source.hasUv4;
				if (source.hasNormals)
				{
					CopyArray(source.normals, ref normals, vertexCount);
				}
				if (source.hasTangents)
				{
					CopyArray(source.tangents, ref tangents, vertexCount);
				}
				if (source.hasUv)
				{
					CopyArray(source.uv, ref uv, vertexCount);
				}
				if (source.hasUv2)
				{
					CopyArray(source.uv2, ref uv2, vertexCount);
				}
				if (source.hasUv3)
				{
					CopyArray(source.uv3, ref uv3, vertexCount);
				}
				if (source.hasUv4)
				{
					CopyArray(source.uv4, ref uv4, vertexCount);
				}
				if (source.hasColors)
				{
					CopyArray(source.colors32, ref colors32, vertexCount);
				}
			}

			public void CopyArray<T>(Array sourceArray, ref T[] destinationArray, int vertexCount)
			{
				if (destinationArray == null)
				{
					destinationArray = new T[65534];
				}
				Array.Copy(sourceArray, 0, destinationArray, 0, vertexCount);
			}

			public void CheckHasArrays()
			{
				if (normals != null && normals.Length > 0)
				{
					hasNormals = true;
				}
				if (tangents != null && tangents.Length > 0)
				{
					hasTangents = true;
				}
				if (uv != null && uv.Length > 0)
				{
					hasUv = true;
				}
				if (uv2 != null && uv2.Length > 0)
				{
					hasUv2 = true;
				}
				if (uv3 != null && uv3.Length > 0)
				{
					hasUv3 = true;
				}
				if (uv4 != null && uv4.Length > 0)
				{
					hasUv4 = true;
				}
				if (colors32 != null && colors32.Length > 0)
				{
					hasColors = true;
				}
			}

			public void ResetHasBooleans()
			{
				hasNormals = (hasTangents = (hasUv = (hasUv2 = (hasUv3 = (hasUv4 = (hasColors = false))))));
			}

			public void Init(bool initTriangles = true)
			{
				vertices = new Vector3[65534];
				if (initTriangles)
				{
					triangles = new int[786408];
				}
			}

			public void RebuildVertexBuffer(SubMeshCache sub, bool resizeArrays)
			{
				int[] array = new int[sub.vertices.Length];
				int[] array2 = new int[array.Length];
				vertexCount = 0;
				for (int i = 0; i < triangleCount; i++)
				{
					int num = triangles[i];
					if (array[num] == 0)
					{
						array[num] = vertexCount + 1;
						array2[vertexCount] = num;
						triangles[i] = vertexCount;
						vertexCount++;
					}
					else
					{
						triangles[i] = array[num] - 1;
					}
				}
				if (resizeArrays)
				{
					vertices = new Vector3[vertexCount];
				}
				hasNormals = sub.hasNormals;
				hasTangents = sub.hasTangents;
				hasColors = sub.hasColors;
				hasUv = sub.hasUv;
				hasUv2 = sub.hasUv2;
				hasUv3 = sub.hasUv3;
				hasUv4 = sub.hasUv4;
				if (resizeArrays)
				{
					if (hasNormals)
					{
						normals = new Vector3[vertexCount];
					}
					if (hasTangents)
					{
						tangents = new Vector4[vertexCount];
					}
					if (hasUv)
					{
						uv = new Vector2[vertexCount];
					}
					if (hasUv2)
					{
						uv2 = new Vector2[vertexCount];
					}
					if (hasUv3)
					{
						uv3 = new Vector2[vertexCount];
					}
					if (hasUv4)
					{
						uv4 = new Vector2[vertexCount];
					}
					if (hasColors)
					{
						colors32 = new Color32[vertexCount];
					}
				}
				for (int j = 0; j < vertexCount; j++)
				{
					int num2 = array2[j];
					vertices[j] = sub.vertices[num2];
					if (hasNormals)
					{
						normals[j] = sub.normals[num2];
					}
					if (hasTangents)
					{
						tangents[j] = sub.tangents[num2];
					}
					if (hasUv)
					{
						uv[j] = sub.uv[num2];
					}
					if (hasUv2)
					{
						uv2[j] = sub.uv2[num2];
					}
					if (hasUv3)
					{
						uv3[j] = sub.uv3[num2];
					}
					if (hasUv4)
					{
						uv4[j] = sub.uv4[num2];
					}
					if (hasColors)
					{
						colors32[j] = sub.colors32[num2];
					}
				}
			}
		}

		public Mesh mesh;

		public SubMeshCache[] subMeshCache;

		public int subMeshCount;

		public MeshCache(Mesh mesh)
		{
			this.mesh = mesh;
			subMeshCount = mesh.subMeshCount;
			subMeshCache = new SubMeshCache[subMeshCount];
			if (subMeshCount == 1)
			{
				subMeshCache[0] = new SubMeshCache(mesh, true);
				return;
			}
			SubMeshCache sub = new SubMeshCache(mesh, false);
			for (int i = 0; i < subMeshCache.Length; i++)
			{
				subMeshCache[i] = new SubMeshCache(mesh, i);
				subMeshCache[i].RebuildVertexBuffer(sub, true);
			}
		}
	}
}
