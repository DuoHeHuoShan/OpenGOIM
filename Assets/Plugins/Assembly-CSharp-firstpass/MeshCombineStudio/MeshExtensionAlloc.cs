using System;
using UnityEngine;

namespace MeshCombineStudio
{
	public static class MeshExtensionAlloc
	{
		public static void ApplyVertices(Mesh mesh, Vector3[] vertices, int length)
		{
			Vector3[] array = new Vector3[length];
			Array.Copy(vertices, array, length);
			mesh.vertices = array;
		}

		public static void ApplyNormals(Mesh mesh, Vector3[] normals, int length)
		{
			Vector3[] array = new Vector3[length];
			Array.Copy(normals, array, length);
			mesh.normals = array;
		}

		public static void ApplyTangents(Mesh mesh, Vector4[] tangents, int length)
		{
			Vector4[] array = new Vector4[length];
			Array.Copy(tangents, array, length);
			mesh.tangents = array;
		}

		public static void ApplyUvs(Mesh mesh, int channel, Vector2[] uvs, int length)
		{
			Vector2[] array = new Vector2[length];
			Array.Copy(uvs, array, length);
			switch (channel)
			{
			case 0:
				mesh.uv = array;
				break;
			case 1:
				mesh.uv2 = array;
				break;
			case 2:
				mesh.uv3 = array;
				break;
			default:
				mesh.uv4 = array;
				break;
			}
		}

		public static void ApplyColors32(Mesh mesh, Color32[] colors, int length)
		{
			Color32[] array = new Color32[length];
			Array.Copy(colors, array, length);
			mesh.colors32 = array;
		}

		public static void ApplyTriangles(Mesh mesh, int[] triangles, int length)
		{
			int[] array = new int[length];
			Array.Copy(triangles, array, length);
			mesh.triangles = array;
		}
	}
}
