using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio
{
	public class RemoveGeometryBelowTerrain : MonoBehaviour
	{
		private int totalTriangles;

		private int removeTriangles;

		private int skippedObjects;

		public List<Transform> terrains = new List<Transform>();

		public List<Transform> meshTerrains = new List<Transform>();

		public Bounds[] terrainBounds;

		public Bounds[] meshBounds;

		private Terrain[] terrainComponents;

		private Terrain[] terrainArray;

		private Bounds[] terrainBoundsArray;

		private MeshRenderer[] mrs;

		private Mesh[] meshTerrainComponents;

		private Mesh[] meshArray;

		public bool runOnStart;

		private void Start()
		{
			if (runOnStart)
			{
				Remove(base.gameObject);
			}
		}

		public void Remove(GameObject go)
		{
			MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter>(true);
			totalTriangles = 0;
			removeTriangles = 0;
			skippedObjects = 0;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				RemoveMesh(componentsInChildren[i].transform, componentsInChildren[i].mesh);
			}
			Debug.Log("Removeable " + removeTriangles + " total " + totalTriangles + " improvement " + ((float)removeTriangles / (float)totalTriangles * 100f).ToString("F2"));
			Debug.Log("Skipped Objects " + skippedObjects);
		}

		public void RemoveMesh(Transform t, Mesh mesh)
		{
			if (mesh == null)
			{
				return;
			}
			if (!IsMeshUnderTerrain(t, mesh))
			{
				skippedObjects++;
				return;
			}
			Vector3[] vertices = mesh.vertices;
			List<int> list = new List<int>();
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				list.AddRange(mesh.GetTriangles(i));
				int count = list.Count;
				RemoveTriangles(t, list, vertices);
				if (list.Count < count)
				{
					mesh.SetTriangles(list.ToArray(), i);
				}
			}
		}

		public bool IsMeshUnderTerrain(Transform t, Mesh mesh)
		{
			Bounds bounds = mesh.bounds;
			bounds.center += t.position;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			Vector2 vector = new Vector2(max.x - min.x, max.z - min.z);
			for (float num = 0f; num < 1f; num += 0.125f)
			{
				for (float num2 = 0f; num2 < 1f; num2 += 0.125f)
				{
					Vector3 vector2 = new Vector3(min.x + num2 * vector.x, min.y, min.z + num * vector.y);
					float num3 = 0f;
					if (vector2.y < num3)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void GetTerrainComponents()
		{
			terrainComponents = new Terrain[terrains.Count];
			for (int i = 0; i < terrains.Count; i++)
			{
				Terrain component = terrains[i].GetComponent<Terrain>();
				terrainComponents[i] = component;
			}
		}

		public void GetMeshRenderersAndComponents()
		{
			mrs = new MeshRenderer[meshTerrains.Count];
			meshTerrainComponents = new Mesh[meshTerrains.Count];
			for (int i = 0; i < meshTerrains.Count; i++)
			{
				mrs[i] = meshTerrains[i].GetComponent<MeshRenderer>();
				MeshFilter component = meshTerrains[i].GetComponent<MeshFilter>();
				meshTerrainComponents[i] = component.sharedMesh;
			}
		}

		public void CreateTerrainBounds()
		{
			terrainBounds = new Bounds[terrainComponents.Length];
			for (int i = 0; i < terrainBounds.Length; i++)
			{
				terrainBounds[i] = default(Bounds);
				terrainBounds[i].min = terrains[i].position;
				terrainBounds[i].max = terrainBounds[i].min + terrainComponents[i].terrainData.size;
			}
			meshBounds = new Bounds[meshTerrains.Count];
			for (int j = 0; j < meshTerrains.Count; j++)
			{
				meshBounds[j] = mrs[j].bounds;
			}
		}

		public void MakeIntersectLists(Bounds bounds)
		{
			List<Terrain> list = new List<Terrain>();
			List<Mesh> list2 = new List<Mesh>();
			List<Bounds> list3 = new List<Bounds>();
			List<Bounds> list4 = new List<Bounds>();
			Vector3[] array = new Vector3[8];
			Vector3 size = bounds.size;
			array[0] = bounds.min;
			array[1] = array[0] + new Vector3(size.x, 0f, 0f);
			array[2] = array[0] + new Vector3(0f, 0f, size.z);
			array[3] = array[0] + new Vector3(size.x, 0f, size.z);
			array[4] = array[0] + new Vector3(0f, size.y, 0f);
			array[5] = array[0] + new Vector3(size.x, size.y, 0f);
			array[6] = array[0] + new Vector3(0f, size.y, size.z);
			array[7] = array[0] + size;
			for (int i = 0; i < 8; i++)
			{
				int num = InterectTerrain(array[i]);
				if (num != -1)
				{
					list.Add(terrainArray[num]);
					list3.Add(terrainBounds[num]);
				}
				num = InterectMesh(array[i]);
				if (num != -1)
				{
					list2.Add(meshArray[num]);
					list4.Add(meshBounds[num]);
				}
			}
			terrainArray = list.ToArray();
			meshArray = list2.ToArray();
			terrainBoundsArray = list3.ToArray();
		}

		public int InterectTerrain(Vector3 pos)
		{
			for (int i = 0; i < terrainBounds.Length; i++)
			{
				if (terrainBounds[i].Contains(pos))
				{
					return i;
				}
			}
			return -1;
		}

		public int InterectMesh(Vector3 pos)
		{
			for (int i = 0; i < meshBounds.Length; i++)
			{
				if (meshBounds[i].Contains(pos))
				{
					return i;
				}
			}
			return -1;
		}

		public float GetTerrainHeight(Vector3 pos)
		{
			int num = -1;
			for (int i = 0; i < terrainArray.Length; i++)
			{
				if (terrainBoundsArray[i].Contains(pos))
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				return terrainArray[num].SampleHeight(pos);
			}
			return float.PositiveInfinity;
		}

		public void RemoveTriangles(Transform t, List<int> newTriangles, Vector3[] vertices)
		{
			bool[] array = new bool[vertices.Length];
			Vector3 zero = Vector3.zero;
			float num = 0f;
			for (int i = 0; i < newTriangles.Count; i += 3)
			{
				totalTriangles++;
				int num2 = newTriangles[i];
				bool flag = array[num2];
				if (!flag)
				{
					zero = t.TransformPoint(vertices[num2]);
					num = GetTerrainHeight(zero);
					flag = zero.y < num;
				}
				if (!flag)
				{
					continue;
				}
				array[num2] = true;
				num2 = newTriangles[i + 1];
				flag = array[num2];
				if (!flag)
				{
					zero = t.TransformPoint(vertices[num2]);
					num = GetTerrainHeight(zero);
					flag = zero.y < num;
				}
				if (!flag)
				{
					continue;
				}
				array[num2] = true;
				num2 = newTriangles[i + 2];
				flag = array[num2];
				if (!flag)
				{
					zero = t.TransformPoint(vertices[num2]);
					num = GetTerrainHeight(zero);
					flag = zero.y < num;
				}
				if (flag)
				{
					array[num2] = true;
					removeTriangles++;
					newTriangles.RemoveAt(i + 2);
					newTriangles.RemoveAt(i + 1);
					newTriangles.RemoveAt(i);
					if (i + 3 < newTriangles.Count)
					{
						i -= 3;
					}
				}
			}
		}
	}
}
