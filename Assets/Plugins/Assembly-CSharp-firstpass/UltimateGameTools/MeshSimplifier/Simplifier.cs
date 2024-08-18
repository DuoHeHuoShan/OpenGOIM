using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UltimateGameTools.MeshSimplifier
{
	public class Simplifier : MonoBehaviour
	{
		public delegate void ProgressDelegate(string strTitle, string strProgressMessage, float fT);

		private class Triangle
		{
			private Vertex[] m_aVertices;

			private bool m_bUVData;

			private int[] m_aUV;

			private int[] m_aIndices;

			private Vector3 m_v3Normal;

			private int m_nSubMesh;

			public Vertex[] Vertices
			{
				get
				{
					return m_aVertices;
				}
			}

			public bool HasUVData
			{
				get
				{
					return m_bUVData;
				}
			}

			public int[] IndicesUV
			{
				get
				{
					return m_aUV;
				}
			}

			public Vector3 Normal
			{
				get
				{
					return m_v3Normal;
				}
			}

			public int[] Indices
			{
				get
				{
					return m_aIndices;
				}
			}

			public Triangle(Simplifier simplifier, int nSubMesh, Vertex v0, Vertex v1, Vertex v2, bool bUVData, int nIndex1, int nIndex2, int nIndex3)
			{
				m_aVertices = new Vertex[3];
				m_aUV = new int[3];
				m_aIndices = new int[3];
				m_aVertices[0] = v0;
				m_aVertices[1] = v1;
				m_aVertices[2] = v2;
				m_nSubMesh = nSubMesh;
				m_bUVData = bUVData;
				if (m_bUVData)
				{
					m_aUV[0] = nIndex1;
					m_aUV[1] = nIndex2;
					m_aUV[2] = nIndex3;
				}
				m_aIndices[0] = nIndex1;
				m_aIndices[1] = nIndex2;
				m_aIndices[2] = nIndex3;
				ComputeNormal();
				simplifier.m_aListTriangles[nSubMesh].m_listTriangles.Add(this);
				for (int i = 0; i < 3; i++)
				{
					m_aVertices[i].m_listFaces.Add(this);
					for (int j = 0; j < 3; j++)
					{
						if (i != j && !m_aVertices[i].m_listNeighbors.Contains(m_aVertices[j]))
						{
							m_aVertices[i].m_listNeighbors.Add(m_aVertices[j]);
						}
					}
				}
			}

			public void Destructor(Simplifier simplifier)
			{
				simplifier.m_aListTriangles[m_nSubMesh].m_listTriangles.Remove(this);
				for (int i = 0; i < 3; i++)
				{
					if (m_aVertices[i] != null)
					{
						m_aVertices[i].m_listFaces.Remove(this);
					}
				}
				for (int i = 0; i < 3; i++)
				{
					int num = (i + 1) % 3;
					if (m_aVertices[i] != null && m_aVertices[num] != null)
					{
						m_aVertices[i].RemoveIfNonNeighbor(m_aVertices[num]);
						m_aVertices[num].RemoveIfNonNeighbor(m_aVertices[i]);
					}
				}
			}

			public bool HasVertex(Vertex v)
			{
				return v == m_aVertices[0] || v == m_aVertices[1] || v == m_aVertices[2];
			}

			public void ComputeNormal()
			{
				Vector3 v3Position = m_aVertices[0].m_v3Position;
				Vector3 v3Position2 = m_aVertices[1].m_v3Position;
				Vector3 v3Position3 = m_aVertices[2].m_v3Position;
				m_v3Normal = Vector3.Cross(v3Position2 - v3Position, v3Position3 - v3Position2);
				if (m_v3Normal.magnitude != 0f)
				{
					m_v3Normal = m_v3Normal.normalized;
				}
			}

			public int TexAt(Vertex vertex)
			{
				for (int i = 0; i < 3; i++)
				{
					if (m_aVertices[i] == vertex)
					{
						return m_aUV[i];
					}
				}
				UnityEngine.Debug.LogError("TexAt(): Vertex not found");
				return 0;
			}

			public int TexAt(int i)
			{
				return m_aUV[i];
			}

			public void SetTexAt(Vertex vertex, int uv)
			{
				for (int i = 0; i < 3; i++)
				{
					if (m_aVertices[i] == vertex)
					{
						m_aUV[i] = uv;
						return;
					}
				}
				UnityEngine.Debug.LogError("SetTexAt(): Vertex not found");
			}

			public void SetTexAt(int i, int uv)
			{
				m_aUV[i] = uv;
			}

			public void ReplaceVertex(Vertex vold, Vertex vnew)
			{
				if (vold == m_aVertices[0])
				{
					m_aVertices[0] = vnew;
				}
				else if (vold == m_aVertices[1])
				{
					m_aVertices[1] = vnew;
				}
				else
				{
					m_aVertices[2] = vnew;
				}
				vold.m_listFaces.Remove(this);
				vnew.m_listFaces.Add(this);
				for (int i = 0; i < 3; i++)
				{
					vold.RemoveIfNonNeighbor(m_aVertices[i]);
					m_aVertices[i].RemoveIfNonNeighbor(vold);
				}
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						if (i != j && !m_aVertices[i].m_listNeighbors.Contains(m_aVertices[j]))
						{
							m_aVertices[i].m_listNeighbors.Add(m_aVertices[j]);
						}
					}
				}
				ComputeNormal();
			}
		}

		private class TriangleList
		{
			public List<Triangle> m_listTriangles;

			public TriangleList()
			{
				m_listTriangles = new List<Triangle>();
			}
		}

		private class Vertex
		{
			public Vector3 m_v3Position;

			public Vector3 m_v3PositionWorld;

			public bool m_bHasBoneWeight;

			public BoneWeight m_boneWeight;

			public int m_nID;

			public List<Vertex> m_listNeighbors;

			public List<Triangle> m_listFaces;

			public float m_fObjDist;

			public Vertex m_collapse;

			public int m_nHeapSpot;

			public Vertex(Simplifier simplifier, Vector3 v, Vector3 v3World, bool bHasBoneWeight, BoneWeight boneWeight, int nID)
			{
				m_v3Position = v;
				m_v3PositionWorld = v3World;
				m_bHasBoneWeight = bHasBoneWeight;
				m_boneWeight = boneWeight;
				m_nID = nID;
				m_listNeighbors = new List<Vertex>();
				m_listFaces = new List<Triangle>();
				simplifier.m_listVertices.Add(this);
			}

			public void Destructor(Simplifier simplifier)
			{
				while (m_listNeighbors.Count > 0)
				{
					m_listNeighbors[0].m_listNeighbors.Remove(this);
					if (m_listNeighbors.Count > 0)
					{
						m_listNeighbors.RemoveAt(0);
					}
				}
				simplifier.m_listVertices.Remove(this);
			}

			public void RemoveIfNonNeighbor(Vertex n)
			{
				if (!m_listNeighbors.Contains(n))
				{
					return;
				}
				for (int i = 0; i < m_listFaces.Count; i++)
				{
					if (m_listFaces[i].HasVertex(n))
					{
						return;
					}
				}
				m_listNeighbors.Remove(n);
			}

			public bool IsBorder()
			{
				for (int i = 0; i < m_listNeighbors.Count; i++)
				{
					int num = 0;
					for (int j = 0; j < m_listFaces.Count; j++)
					{
						if (m_listFaces[j].HasVertex(m_listNeighbors[i]))
						{
							num++;
						}
					}
					if (num == 1)
					{
						return true;
					}
				}
				return false;
			}
		}

		private class VertexDataHashComparer : IEqualityComparer<VertexDataHash>
		{
			public bool Equals(VertexDataHash a, VertexDataHash b)
			{
				return a.UV1 == b.UV1 && a.UV2 == b.UV2 && a.Vertex == b.Vertex && a.Color.r == b.Color.r && a.Color.g == b.Color.g && a.Color.b == b.Color.b && a.Color.a == b.Color.a;
			}

			public int GetHashCode(VertexDataHash vdata)
			{
				return vdata.GetHashCode();
			}
		}

		private class VertexDataHash
		{
			private Vector3 _v3Vertex;

			private Vector3 _v3Normal;

			private Vector2 _v2Mapping1;

			private Vector2 _v2Mapping2;

			private Color32 _color;

			private MeshUniqueVertices.UniqueVertex _uniqueVertex;

			public Vector3 Vertex
			{
				get
				{
					return _v3Vertex;
				}
			}

			public Vector3 Normal
			{
				get
				{
					return _v3Normal;
				}
			}

			public Vector2 UV1
			{
				get
				{
					return _v2Mapping1;
				}
			}

			public Vector2 UV2
			{
				get
				{
					return _v2Mapping2;
				}
			}

			public Color32 Color
			{
				get
				{
					return _color;
				}
			}

			public VertexDataHash(Vector3 v3Vertex, Vector3 v3Normal, Vector2 v2Mapping1, Vector2 v2Mapping2, Color32 color)
			{
				_v3Vertex = v3Vertex;
				_v3Normal = v3Normal;
				_v2Mapping1 = v2Mapping1;
				_v2Mapping2 = v2Mapping2;
				_color = color;
				_uniqueVertex = new MeshUniqueVertices.UniqueVertex(v3Vertex);
			}

			public override bool Equals(object obj)
			{
				VertexDataHash vertexDataHash = obj as VertexDataHash;
				return vertexDataHash._v2Mapping1 == _v2Mapping1 && vertexDataHash._v2Mapping2 == _v2Mapping2 && vertexDataHash._v3Vertex == _v3Vertex && vertexDataHash._color.r == _color.r && vertexDataHash._color.g == _color.g && vertexDataHash._color.b == _color.b && vertexDataHash._color.a == _color.a;
			}

			public override int GetHashCode()
			{
				return _uniqueVertex.GetHashCode();
			}

			public static bool operator ==(VertexDataHash a, VertexDataHash b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(VertexDataHash a, VertexDataHash b)
			{
				return !a.Equals(b);
			}
		}

		private static int m_nCoroutineFrameMiliseconds;

		private const float MAX_VERTEX_COLLAPSE_COST = 10000000f;

		private List<Vertex> m_listVertices;

		private List<Vertex> m_listHeap;

		private TriangleList[] m_aListTriangles;

		[SerializeField]
		[HideInInspector]
		private int m_nOriginalMeshVertexCount = -1;

		[SerializeField]
		[HideInInspector]
		private float m_fOriginalMeshSize = 1f;

		[SerializeField]
		[HideInInspector]
		private List<int> m_listVertexMap;

		[SerializeField]
		[HideInInspector]
		private List<int> m_listVertexPermutationBack;

		[SerializeField]
		[HideInInspector]
		private MeshUniqueVertices m_meshUniqueVertices;

		[SerializeField]
		[HideInInspector]
		private Mesh m_meshOriginal;

		[SerializeField]
		[HideInInspector]
		private bool m_bUseEdgeLength = true;

		[SerializeField]
		[HideInInspector]
		private bool m_bUseCurvature = true;

		[SerializeField]
		[HideInInspector]
		private bool m_bProtectTexture = true;

		[SerializeField]
		[HideInInspector]
		private bool m_bLockBorder = true;

		public static bool Cancelled { get; set; }

		public static int CoroutineFrameMiliseconds
		{
			get
			{
				return m_nCoroutineFrameMiliseconds;
			}
			set
			{
				m_nCoroutineFrameMiliseconds = value;
			}
		}

		public bool CoroutineEnded { get; set; }

		public bool UseEdgeLength
		{
			get
			{
				return m_bUseEdgeLength;
			}
			set
			{
				m_bUseEdgeLength = value;
			}
		}

		public bool UseCurvature
		{
			get
			{
				return m_bUseCurvature;
			}
			set
			{
				m_bUseCurvature = value;
			}
		}

		public bool ProtectTexture
		{
			get
			{
				return m_bProtectTexture;
			}
			set
			{
				m_bProtectTexture = value;
			}
		}

		public bool LockBorder
		{
			get
			{
				return m_bLockBorder;
			}
			set
			{
				m_bLockBorder = value;
			}
		}

		public IEnumerator ProgressiveMesh(GameObject gameObject, Mesh sourceMesh, RelevanceSphere[] aRelevanceSpheres, string strProgressDisplayObjectName = "", ProgressDelegate progress = null)
		{
			m_meshOriginal = sourceMesh;
			Vector3[] aVerticesWorld = GetWorldVertices(gameObject);
			if (aVerticesWorld == null)
			{
				CoroutineEnded = true;
				yield break;
			}
			m_listVertexMap = new List<int>();
			m_listVertexPermutationBack = new List<int>();
			m_listVertices = new List<Vertex>();
			m_aListTriangles = new TriangleList[m_meshOriginal.subMeshCount];
			if (progress != null)
			{
				progress("Preprocessing mesh: " + strProgressDisplayObjectName, "Building unique vertex data", 1f);
				if (Cancelled)
				{
					CoroutineEnded = true;
					yield break;
				}
			}
			m_meshUniqueVertices = new MeshUniqueVertices();
			m_meshUniqueVertices.BuildData(m_meshOriginal, aVerticesWorld);
			m_nOriginalMeshVertexCount = m_meshUniqueVertices.ListVertices.Count;
			m_fOriginalMeshSize = Mathf.Max(m_meshOriginal.bounds.size.x, m_meshOriginal.bounds.size.y, m_meshOriginal.bounds.size.z);
			m_listHeap = new List<Vertex>(m_meshUniqueVertices.ListVertices.Count);
			for (int i = 0; i < m_meshUniqueVertices.ListVertices.Count; i++)
			{
				m_listVertexMap.Add(-1);
				m_listVertexPermutationBack.Add(-1);
			}
			Vector2[] av2Mapping = m_meshOriginal.uv;
			AddVertices(m_meshUniqueVertices.ListVertices, m_meshUniqueVertices.ListVerticesWorld, m_meshUniqueVertices.ListBoneWeights);
			for (int j = 0; j < m_meshOriginal.subMeshCount; j++)
			{
				int[] triangles = m_meshOriginal.GetTriangles(j);
				m_aListTriangles[j] = new TriangleList();
				AddFaceListSubMesh(j, m_meshUniqueVertices.SubmeshesFaceList[j].m_listIndices, triangles, av2Mapping);
			}
			if (Application.isEditor && !Application.isPlaying)
			{
				IEnumerator enumerator = ComputeAllEdgeCollapseCosts(strProgressDisplayObjectName, gameObject.transform, aRelevanceSpheres, progress);
				while (enumerator.MoveNext())
				{
					if (Cancelled)
					{
						CoroutineEnded = true;
						yield break;
					}
				}
			}
			else
			{
				yield return StartCoroutine(ComputeAllEdgeCollapseCosts(strProgressDisplayObjectName, gameObject.transform, aRelevanceSpheres, progress));
			}
			int nVertices = m_listVertices.Count;
			Stopwatch sw = Stopwatch.StartNew();
			while (m_listVertices.Count > 0)
			{
				if (progress != null && (m_listVertices.Count & 0xFF) == 0)
				{
					progress("Preprocessing mesh: " + strProgressDisplayObjectName, "Collapsing edges", 1f - (float)m_listVertices.Count / (float)nVertices);
					if (Cancelled)
					{
						CoroutineEnded = true;
						yield break;
					}
				}
				if (sw.ElapsedMilliseconds > CoroutineFrameMiliseconds && CoroutineFrameMiliseconds > 0)
				{
					yield return null;
					sw = Stopwatch.StartNew();
				}
				Vertex mn = MinimumCostEdge();
				m_listVertexPermutationBack[m_listVertices.Count - 1] = mn.m_nID;
				m_listVertexMap[mn.m_nID] = ((mn.m_collapse == null) ? (-1) : mn.m_collapse.m_nID);
				Collapse(mn, mn.m_collapse, true, gameObject.transform, aRelevanceSpheres);
			}
			m_listHeap.Clear();
			CoroutineEnded = true;
		}

		public IEnumerator ComputeMeshWithVertexCount(GameObject gameObject, Mesh meshOut, int nVertices, string strProgressDisplayObjectName = "", ProgressDelegate progress = null)
		{
			if (GetOriginalMeshUniqueVertexCount() == -1)
			{
				CoroutineEnded = true;
				yield break;
			}
			if (nVertices < 3)
			{
				CoroutineEnded = true;
				yield break;
			}
			if (nVertices >= GetOriginalMeshUniqueVertexCount())
			{
				meshOut.triangles = new int[0];
				meshOut.subMeshCount = m_meshOriginal.subMeshCount;
				meshOut.vertices = m_meshOriginal.vertices;
				meshOut.normals = m_meshOriginal.normals;
				meshOut.tangents = m_meshOriginal.tangents;
				meshOut.uv = m_meshOriginal.uv;
				meshOut.uv2 = m_meshOriginal.uv2;
				meshOut.colors32 = m_meshOriginal.colors32;
				meshOut.boneWeights = m_meshOriginal.boneWeights;
				meshOut.bindposes = m_meshOriginal.bindposes;
				meshOut.triangles = m_meshOriginal.triangles;
				meshOut.subMeshCount = m_meshOriginal.subMeshCount;
				for (int i = 0; i < m_meshOriginal.subMeshCount; i++)
				{
					meshOut.SetTriangles(m_meshOriginal.GetTriangles(i), i);
				}
				meshOut.name = gameObject.name + " simplified mesh";
				CoroutineEnded = true;
				yield break;
			}
			m_listVertices = new List<Vertex>();
			m_aListTriangles = new TriangleList[m_meshOriginal.subMeshCount];
			List<Vertex> listVertices = new List<Vertex>();
			AddVertices(m_meshUniqueVertices.ListVertices, m_meshUniqueVertices.ListVerticesWorld, m_meshUniqueVertices.ListBoneWeights);
			for (int j = 0; j < m_listVertices.Count; j++)
			{
				m_listVertices[j].m_collapse = ((m_listVertexMap[j] != -1) ? m_listVertices[m_listVertexMap[j]] : null);
				listVertices.Add(m_listVertices[m_listVertexPermutationBack[j]]);
			}
			Vector2[] av2Mapping = m_meshOriginal.uv;
			for (int k = 0; k < m_meshOriginal.subMeshCount; k++)
			{
				int[] triangles = m_meshOriginal.GetTriangles(k);
				m_aListTriangles[k] = new TriangleList();
				AddFaceListSubMesh(k, m_meshUniqueVertices.SubmeshesFaceList[k].m_listIndices, triangles, av2Mapping);
			}
			int nTotalVertices = listVertices.Count;
			Stopwatch sw = Stopwatch.StartNew();
			while (listVertices.Count > nVertices)
			{
				if (progress != null && nTotalVertices != nVertices && (listVertices.Count & 0xFF) == 0)
				{
					float fT = 1f - (float)(listVertices.Count - nVertices) / (float)(nTotalVertices - nVertices);
					progress("Simplifying mesh: " + strProgressDisplayObjectName, "Collapsing edges", fT);
					if (Cancelled)
					{
						CoroutineEnded = true;
						yield break;
					}
				}
				Vertex mn = listVertices[listVertices.Count - 1];
				listVertices.RemoveAt(listVertices.Count - 1);
				Collapse(mn, mn.m_collapse, false, null, null);
				if (sw.ElapsedMilliseconds > CoroutineFrameMiliseconds && CoroutineFrameMiliseconds > 0)
				{
					yield return null;
					sw = Stopwatch.StartNew();
				}
			}
			Vector3[] av3Vertices = new Vector3[m_listVertices.Count];
			for (int l = 0; l < m_listVertices.Count; l++)
			{
				m_listVertices[l].m_nID = l;
				av3Vertices[l] = m_listVertices[l].m_v3Position;
			}
			if (Application.isEditor && !Application.isPlaying)
			{
				IEnumerator enumerator = ConsolidateMesh(gameObject, m_meshOriginal, meshOut, m_aListTriangles, av3Vertices, strProgressDisplayObjectName, progress);
				while (enumerator.MoveNext())
				{
					if (Cancelled)
					{
						CoroutineEnded = true;
						yield break;
					}
				}
			}
			else
			{
				yield return StartCoroutine(ConsolidateMesh(gameObject, m_meshOriginal, meshOut, m_aListTriangles, av3Vertices, strProgressDisplayObjectName, progress));
			}
			CoroutineEnded = true;
		}

		public int GetOriginalMeshUniqueVertexCount()
		{
			return m_nOriginalMeshVertexCount;
		}

		public int GetOriginalMeshTriangleCount()
		{
			return m_meshOriginal.triangles.Length / 3;
		}

		public static Vector3[] GetWorldVertices(GameObject gameObject)
		{
			Vector3[] array = null;
			SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			if (component != null)
			{
				if (component.sharedMesh == null)
				{
					return null;
				}
				array = component.sharedMesh.vertices;
				BoneWeight[] boneWeights = component.sharedMesh.boneWeights;
				Matrix4x4[] bindposes = component.sharedMesh.bindposes;
				Transform[] bones = component.bones;
				if (array == null || boneWeights == null || bindposes == null || bones == null)
				{
					return null;
				}
				if (boneWeights.Length == 0 || bindposes.Length == 0 || bones.Length == 0)
				{
					return null;
				}
				for (int i = 0; i < array.Length; i++)
				{
					BoneWeight boneWeight = boneWeights[i];
					Vector3 zero = Vector3.zero;
					if (Math.Abs(boneWeight.weight0) > 1E-05f)
					{
						Vector3 point = bindposes[boneWeight.boneIndex0].MultiplyPoint3x4(array[i]);
						zero += bones[boneWeight.boneIndex0].transform.localToWorldMatrix.MultiplyPoint3x4(point) * boneWeight.weight0;
					}
					if (Math.Abs(boneWeight.weight1) > 1E-05f)
					{
						Vector3 point = bindposes[boneWeight.boneIndex1].MultiplyPoint3x4(array[i]);
						zero += bones[boneWeight.boneIndex1].transform.localToWorldMatrix.MultiplyPoint3x4(point) * boneWeight.weight1;
					}
					if (Math.Abs(boneWeight.weight2) > 1E-05f)
					{
						Vector3 point = bindposes[boneWeight.boneIndex2].MultiplyPoint3x4(array[i]);
						zero += bones[boneWeight.boneIndex2].transform.localToWorldMatrix.MultiplyPoint3x4(point) * boneWeight.weight2;
					}
					if (Math.Abs(boneWeight.weight3) > 1E-05f)
					{
						Vector3 point = bindposes[boneWeight.boneIndex3].MultiplyPoint3x4(array[i]);
						zero += bones[boneWeight.boneIndex3].transform.localToWorldMatrix.MultiplyPoint3x4(point) * boneWeight.weight3;
					}
					array[i] = zero;
				}
			}
			else if (component2 != null)
			{
				if (component2.sharedMesh == null)
				{
					return null;
				}
				array = component2.sharedMesh.vertices;
				if (array == null)
				{
					return null;
				}
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = gameObject.transform.TransformPoint(array[j]);
				}
			}
			return array;
		}

		private IEnumerator ConsolidateMesh(GameObject gameObject, Mesh meshIn, Mesh meshOut, TriangleList[] aListTriangles, Vector3[] av3Vertices, string strProgressDisplayObjectName = "", ProgressDelegate progress = null)
		{
			Vector3[] av3NormalsIn = meshIn.normals;
			Vector4[] av4TangentsIn = meshIn.tangents;
			Vector2[] av2Mapping1In = meshIn.uv;
			Vector2[] av2Mapping2In = meshIn.uv2;
			Color[] acolColorsIn = meshIn.colors;
			Color32[] aColors32In = meshIn.colors32;
			List<List<int>> listlistIndicesOut = new List<List<int>>();
			List<Vector3> listVerticesOut = new List<Vector3>();
			List<Vector3> listNormalsOut = new List<Vector3>();
			List<Vector4> listTangentsOut = new List<Vector4>();
			List<Vector2> listMapping1Out = new List<Vector2>();
			List<Vector2> listMapping2Out = new List<Vector2>();
			List<Color32> listColors32Out = new List<Color32>();
			List<BoneWeight> listBoneWeightsOut = new List<BoneWeight>();
			Dictionary<VertexDataHash, int> dicVertexDataHash2Index = new Dictionary<VertexDataHash, int>(new VertexDataHashComparer());
			bool bUV1 = av2Mapping1In != null && av2Mapping1In.Length > 0;
			bool bUV2 = av2Mapping2In != null && av2Mapping2In.Length > 0;
			bool bNormal = av3NormalsIn != null && av3NormalsIn.Length > 0;
			bool bTangent = av4TangentsIn != null && av4TangentsIn.Length > 0;
			Stopwatch sw = Stopwatch.StartNew();
			for (int nSubMesh = 0; nSubMesh < aListTriangles.Length; nSubMesh++)
			{
				List<int> listIndicesOut = new List<int>();
				string strMesh = ((aListTriangles.Length <= 1) ? "Consolidating mesh" : ("Consolidating submesh " + (nSubMesh + 1)));
				for (int i = 0; i < aListTriangles[nSubMesh].m_listTriangles.Count; i++)
				{
					if (progress != null && (i & 0xFF) == 0)
					{
						float fT = ((aListTriangles[nSubMesh].m_listTriangles.Count != 1) ? ((float)i / (float)(aListTriangles[nSubMesh].m_listTriangles.Count - 1)) : 1f);
						progress("Simplifying mesh: " + strProgressDisplayObjectName, strMesh, fT);
						if (Cancelled)
						{
							yield break;
						}
					}
					if (sw.ElapsedMilliseconds > CoroutineFrameMiliseconds && CoroutineFrameMiliseconds > 0)
					{
						yield return null;
						sw = Stopwatch.StartNew();
					}
					for (int j = 0; j < 3; j++)
					{
						int num = aListTriangles[nSubMesh].m_listTriangles[i].IndicesUV[j];
						int num2 = aListTriangles[nSubMesh].m_listTriangles[i].Indices[j];
						bool flag = false;
						Vector3 v3Position = aListTriangles[nSubMesh].m_listTriangles[i].Vertices[j].m_v3Position;
						Vector3 vector = ((!bNormal) ? Vector3.zero : av3NormalsIn[num2]);
						Vector4 item = ((!bTangent) ? Vector4.zero : av4TangentsIn[num2]);
						Vector2 vector2 = ((!bUV1) ? Vector2.zero : av2Mapping1In[num]);
						Vector2 vector3 = ((!bUV2) ? Vector2.zero : av2Mapping2In[num2]);
						Color32 color = new Color32(0, 0, 0, 0);
						if (acolColorsIn != null && acolColorsIn.Length > 0)
						{
							color = acolColorsIn[num2];
							flag = true;
						}
						else if (aColors32In != null && aColors32In.Length > 0)
						{
							color = aColors32In[num2];
							flag = true;
						}
						VertexDataHash vertexDataHash = new VertexDataHash(v3Position, vector, vector2, vector3, color);
						if (dicVertexDataHash2Index.ContainsKey(vertexDataHash))
						{
							listIndicesOut.Add(dicVertexDataHash2Index[vertexDataHash]);
							continue;
						}
						dicVertexDataHash2Index.Add(vertexDataHash, listVerticesOut.Count);
						listVerticesOut.Add(vertexDataHash.Vertex);
						if (bNormal)
						{
							listNormalsOut.Add(vector);
						}
						if (bUV1)
						{
							listMapping1Out.Add(vector2);
						}
						if (bUV2)
						{
							listMapping2Out.Add(vector3);
						}
						if (bTangent)
						{
							listTangentsOut.Add(item);
						}
						if (flag)
						{
							listColors32Out.Add(color);
						}
						if (aListTriangles[nSubMesh].m_listTriangles[i].Vertices[j].m_bHasBoneWeight)
						{
							listBoneWeightsOut.Add(aListTriangles[nSubMesh].m_listTriangles[i].Vertices[j].m_boneWeight);
						}
						listIndicesOut.Add(listVerticesOut.Count - 1);
					}
				}
				listlistIndicesOut.Add(listIndicesOut);
			}
			meshOut.triangles = new int[0];
			meshOut.vertices = listVerticesOut.ToArray();
			meshOut.normals = ((listNormalsOut.Count <= 0) ? null : listNormalsOut.ToArray());
			meshOut.tangents = ((listTangentsOut.Count <= 0) ? null : listTangentsOut.ToArray());
			meshOut.uv = ((listMapping1Out.Count <= 0) ? null : listMapping1Out.ToArray());
			meshOut.uv2 = ((listMapping2Out.Count <= 0) ? null : listMapping2Out.ToArray());
			meshOut.colors32 = ((listColors32Out.Count <= 0) ? null : listColors32Out.ToArray());
			meshOut.boneWeights = ((listBoneWeightsOut.Count <= 0) ? null : listBoneWeightsOut.ToArray());
			meshOut.bindposes = meshIn.bindposes;
			meshOut.subMeshCount = listlistIndicesOut.Count;
			for (int k = 0; k < listlistIndicesOut.Count; k++)
			{
				meshOut.SetTriangles(listlistIndicesOut[k].ToArray(), k);
			}
			meshOut.name = gameObject.name + " simplified mesh";
			progress("Simplifying mesh: " + strProgressDisplayObjectName, "Mesh consolidation done", 1f);
		}

		private int MapVertex(int nVertex, int nMax)
		{
			if (nMax <= 0)
			{
				return 0;
			}
			while (nVertex >= nMax)
			{
				nVertex = m_listVertexMap[nVertex];
			}
			return nVertex;
		}

		private float ComputeEdgeCollapseCost(Vertex u, Vertex v, float fRelevanceBias)
		{
			bool bUseEdgeLength = m_bUseEdgeLength;
			bool bUseCurvature = m_bUseCurvature;
			bool bProtectTexture = m_bProtectTexture;
			bool bLockBorder = m_bLockBorder;
			float num = ((!bUseEdgeLength) ? 1f : (Vector3.Magnitude(v.m_v3Position - u.m_v3Position) / m_fOriginalMeshSize));
			float num2 = 0.001f;
			List<Triangle> list = new List<Triangle>();
			for (int i = 0; i < u.m_listFaces.Count; i++)
			{
				if (u.m_listFaces[i].HasVertex(v))
				{
					list.Add(u.m_listFaces[i]);
				}
			}
			if (bUseCurvature)
			{
				for (int i = 0; i < u.m_listFaces.Count; i++)
				{
					float num3 = 1f;
					for (int j = 0; j < list.Count; j++)
					{
						float num4 = Vector3.Dot(u.m_listFaces[i].Normal, list[j].Normal);
						num3 = Mathf.Min(num3, (1f - num4) / 2f);
					}
					num2 = Mathf.Max(num2, num3);
				}
			}
			if (u.IsBorder() && list.Count > 1)
			{
				num2 = 1f;
			}
			if (bProtectTexture)
			{
				bool flag = true;
				for (int i = 0; i < u.m_listFaces.Count; i++)
				{
					for (int k = 0; k < list.Count; k++)
					{
						if (!u.m_listFaces[i].HasUVData)
						{
							flag = false;
							break;
						}
						if (u.m_listFaces[i].TexAt(u) == list[k].TexAt(u))
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					num2 = 1f;
				}
			}
			if (bLockBorder && u.IsBorder())
			{
				num2 = 10000000f;
			}
			num2 += fRelevanceBias;
			return num * num2;
		}

		private void ComputeEdgeCostAtVertex(Vertex v, Transform transform, RelevanceSphere[] aRelevanceSpheres)
		{
			if (v.m_listNeighbors.Count == 0)
			{
				v.m_collapse = null;
				v.m_fObjDist = -0.01f;
				return;
			}
			v.m_fObjDist = 10000000f;
			v.m_collapse = null;
			float fRelevanceBias = 0f;
			if (aRelevanceSpheres != null)
			{
				for (int i = 0; i < aRelevanceSpheres.Length; i++)
				{
					Matrix4x4 matrix4x = Matrix4x4.TRS(aRelevanceSpheres[i].m_v3Position, Quaternion.Euler(aRelevanceSpheres[i].m_v3Rotation), aRelevanceSpheres[i].m_v3Scale);
					Vector3 v3PositionWorld = v.m_v3PositionWorld;
					if (matrix4x.inverse.MultiplyPoint(v3PositionWorld).magnitude <= 0.5f)
					{
						fRelevanceBias = aRelevanceSpheres[i].m_fRelevance;
					}
				}
			}
			for (int j = 0; j < v.m_listNeighbors.Count; j++)
			{
				float num = ComputeEdgeCollapseCost(v, v.m_listNeighbors[j], fRelevanceBias);
				if (v.m_collapse == null || num < v.m_fObjDist)
				{
					v.m_collapse = v.m_listNeighbors[j];
					v.m_fObjDist = num;
				}
			}
		}

		private IEnumerator ComputeAllEdgeCollapseCosts(string strProgressDisplayObjectName, Transform transform, RelevanceSphere[] aRelevanceSpheres, ProgressDelegate progress = null)
		{
			Stopwatch sw = Stopwatch.StartNew();
			for (int i = 0; i < m_listVertices.Count; i++)
			{
				if (progress != null && (i & 0xFF) == 0)
				{
					progress("Preprocessing mesh: " + strProgressDisplayObjectName, "Computing edge collapse cost", (m_listVertices.Count != 1) ? ((float)i / ((float)m_listVertices.Count - 1f)) : 1f);
					if (Cancelled)
					{
						break;
					}
				}
				if (sw.ElapsedMilliseconds > CoroutineFrameMiliseconds && CoroutineFrameMiliseconds > 0)
				{
					yield return null;
					sw = Stopwatch.StartNew();
				}
				ComputeEdgeCostAtVertex(m_listVertices[i], transform, aRelevanceSpheres);
				HeapAdd(m_listVertices[i]);
			}
		}

		private void Collapse(Vertex u, Vertex v, bool bRecompute, Transform transform, RelevanceSphere[] aRelevanceSpheres)
		{
			if (v == null)
			{
				u.Destructor(this);
				return;
			}
			List<Vertex> list = new List<Vertex>();
			for (int i = 0; i < u.m_listNeighbors.Count; i++)
			{
				list.Add(u.m_listNeighbors[i]);
			}
			List<Triangle> list2 = new List<Triangle>();
			for (int i = 0; i < u.m_listFaces.Count; i++)
			{
				if (u.m_listFaces[i].HasVertex(v))
				{
					list2.Add(u.m_listFaces[i]);
				}
			}
			for (int i = 0; i < u.m_listFaces.Count; i++)
			{
				if (u.m_listFaces[i].HasVertex(v) || !u.m_listFaces[i].HasUVData)
				{
					continue;
				}
				for (int j = 0; j < list2.Count; j++)
				{
					if (u.m_listFaces[i].TexAt(u) == list2[j].TexAt(u))
					{
						u.m_listFaces[i].SetTexAt(u, list2[j].TexAt(v));
						break;
					}
				}
			}
			for (int i = u.m_listFaces.Count - 1; i >= 0; i--)
			{
				if (i < u.m_listFaces.Count && i >= 0 && u.m_listFaces[i].HasVertex(v))
				{
					u.m_listFaces[i].Destructor(this);
				}
			}
			for (int i = u.m_listFaces.Count - 1; i >= 0; i--)
			{
				u.m_listFaces[i].ReplaceVertex(u, v);
			}
			u.Destructor(this);
			if (bRecompute)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ComputeEdgeCostAtVertex(list[i], transform, aRelevanceSpheres);
					HeapSortUp(list[i].m_nHeapSpot);
					HeapSortDown(list[i].m_nHeapSpot);
				}
			}
		}

		private void AddVertices(List<Vector3> listVertices, List<Vector3> listVerticesWorld, List<MeshUniqueVertices.SerializableBoneWeight> listBoneWeights)
		{
			bool flag = listBoneWeights != null && listBoneWeights.Count > 0;
			for (int i = 0; i < listVertices.Count; i++)
			{
				new Vertex(this, listVertices[i], listVerticesWorld[i], flag, (!flag) ? default(BoneWeight) : listBoneWeights[i].ToBoneWeight(), i);
			}
		}

		private void AddFaceListSubMesh(int nSubMesh, List<int> listTriangles, int[] anIndices, Vector2[] v2Mapping)
		{
			bool bUVData = false;
			if (v2Mapping != null && v2Mapping.Length > 0)
			{
				bUVData = true;
			}
			for (int i = 0; i < listTriangles.Count / 3; i++)
			{
				Triangle t = new Triangle(this, nSubMesh, m_listVertices[listTriangles[i * 3]], m_listVertices[listTriangles[i * 3 + 1]], m_listVertices[listTriangles[i * 3 + 2]], bUVData, anIndices[i * 3], anIndices[i * 3 + 1], anIndices[i * 3 + 2]);
				ShareUV(v2Mapping, t);
			}
		}

		private void ShareUV(Vector2[] aMapping, Triangle t)
		{
			if (!t.HasUVData || aMapping == null || aMapping.Length == 0)
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				int num = i;
				for (int j = 0; j < t.Vertices[num].m_listFaces.Count; j++)
				{
					Triangle triangle = t.Vertices[num].m_listFaces[j];
					if (t == triangle)
					{
						continue;
					}
					int num2 = t.TexAt(t.Vertices[num]);
					int num3 = triangle.TexAt(t.Vertices[num]);
					if (num2 != num3)
					{
						Vector2 vector = aMapping[num2];
						Vector2 vector2 = aMapping[num3];
						if (vector == vector2)
						{
							t.SetTexAt(t.Vertices[num], num3);
						}
					}
				}
			}
		}

		private Vertex MinimumCostEdge()
		{
			return HeapPop();
		}

		private float HeapValue(int i)
		{
			if (i < 0 || i >= m_listHeap.Count)
			{
				return 1E+13f;
			}
			if (m_listHeap[i] == null)
			{
				return 1E+13f;
			}
			return m_listHeap[i].m_fObjDist;
		}

		private void HeapSortUp(int k)
		{
			int num;
			while (HeapValue(k) < HeapValue(num = (k - 1) / 2))
			{
				Vertex value = m_listHeap[k];
				m_listHeap[k] = m_listHeap[num];
				m_listHeap[k].m_nHeapSpot = k;
				m_listHeap[num] = value;
				m_listHeap[num].m_nHeapSpot = num;
				k = num;
			}
		}

		private void HeapSortDown(int k)
		{
			if (k == -1)
			{
				return;
			}
			int num;
			while (HeapValue(k) > HeapValue(num = (k + 1) * 2) || HeapValue(k) > HeapValue(num - 1))
			{
				num = ((!(HeapValue(num) < HeapValue(num - 1))) ? (num - 1) : num);
				Vertex vertex = m_listHeap[k];
				m_listHeap[k] = m_listHeap[num];
				m_listHeap[k].m_nHeapSpot = k;
				m_listHeap[num] = vertex;
				if (vertex != null)
				{
					m_listHeap[num].m_nHeapSpot = num;
				}
				k = num;
			}
		}

		private void HeapAdd(Vertex v)
		{
			int count = m_listHeap.Count;
			m_listHeap.Add(v);
			v.m_nHeapSpot = count;
			HeapSortUp(count);
		}

		private Vertex HeapPop()
		{
			Vertex vertex = m_listHeap[0];
			vertex.m_nHeapSpot = -1;
			m_listHeap[0] = null;
			HeapSortDown(0);
			return vertex;
		}
	}
}
