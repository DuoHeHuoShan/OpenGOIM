using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateGameTools.MeshSimplifier
{
	[Serializable]
	public class MeshUniqueVertices
	{
		[Serializable]
		public class ListIndices
		{
			public List<int> m_listIndices;

			public ListIndices()
			{
				m_listIndices = new List<int>();
			}
		}

		[Serializable]
		public class SerializableBoneWeight
		{
			public int _boneIndex0;

			public int _boneIndex1;

			public int _boneIndex2;

			public int _boneIndex3;

			public float _boneWeight0;

			public float _boneWeight1;

			public float _boneWeight2;

			public float _boneWeight3;

			public SerializableBoneWeight(BoneWeight boneWeight)
			{
				_boneIndex0 = boneWeight.boneIndex0;
				_boneIndex1 = boneWeight.boneIndex1;
				_boneIndex2 = boneWeight.boneIndex2;
				_boneIndex3 = boneWeight.boneIndex3;
				_boneWeight0 = boneWeight.weight0;
				_boneWeight1 = boneWeight.weight1;
				_boneWeight2 = boneWeight.weight2;
				_boneWeight3 = boneWeight.weight3;
			}

			public BoneWeight ToBoneWeight()
			{
				BoneWeight result = default(BoneWeight);
				result.boneIndex0 = _boneIndex0;
				result.boneIndex1 = _boneIndex1;
				result.boneIndex2 = _boneIndex2;
				result.boneIndex3 = _boneIndex3;
				result.weight0 = _boneWeight0;
				result.weight1 = _boneWeight1;
				result.weight2 = _boneWeight2;
				result.weight3 = _boneWeight3;
				return result;
			}
		}

		public class UniqueVertex
		{
			private int m_nFixedX;

			private int m_nFixedY;

			private int m_nFixedZ;

			private const float fDecimalMultiplier = 100000f;

			public UniqueVertex(Vector3 v3Vertex)
			{
				FromVertex(v3Vertex);
			}

			public override bool Equals(object obj)
			{
				UniqueVertex uniqueVertex = obj as UniqueVertex;
				return uniqueVertex.m_nFixedX == m_nFixedX && uniqueVertex.m_nFixedY == m_nFixedY && uniqueVertex.m_nFixedZ == m_nFixedZ;
			}

			public override int GetHashCode()
			{
				return m_nFixedX + (m_nFixedY << 2) + (m_nFixedZ << 4);
			}

			public Vector3 ToVertex()
			{
				return new Vector3(FixedToCoord(m_nFixedX), FixedToCoord(m_nFixedY), FixedToCoord(m_nFixedZ));
			}

			public static bool operator ==(UniqueVertex a, UniqueVertex b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(UniqueVertex a, UniqueVertex b)
			{
				return !a.Equals(b);
			}

			private void FromVertex(Vector3 vertex)
			{
				m_nFixedX = CoordToFixed(vertex.x);
				m_nFixedY = CoordToFixed(vertex.y);
				m_nFixedZ = CoordToFixed(vertex.z);
			}

			private int CoordToFixed(float fCoord)
			{
				int num = Mathf.FloorToInt(fCoord);
				int num2 = Mathf.FloorToInt((fCoord - (float)num) * 100000f);
				return (num << 16) | num2;
			}

			private float FixedToCoord(int nFixed)
			{
				float num = (float)(nFixed & 0xFFFF) / 100000f;
				float num2 = nFixed >> 16;
				return num2 + num;
			}
		}

		private class RepeatedVertex
		{
			private int _nFaceIndex;

			private int _nOriginalVertexIndex;

			public int FaceIndex
			{
				get
				{
					return _nFaceIndex;
				}
			}

			public int OriginalVertexIndex
			{
				get
				{
					return _nOriginalVertexIndex;
				}
			}

			public RepeatedVertex(int nFaceIndex, int nOriginalVertexIndex)
			{
				_nFaceIndex = nFaceIndex;
				_nOriginalVertexIndex = nOriginalVertexIndex;
			}
		}

		private class RepeatedVertexList
		{
			private int m_nUniqueIndex;

			private List<RepeatedVertex> m_listRepeatedVertices;

			public int UniqueIndex
			{
				get
				{
					return m_nUniqueIndex;
				}
			}

			public RepeatedVertexList(int nUniqueIndex, RepeatedVertex repeatedVertex)
			{
				m_nUniqueIndex = nUniqueIndex;
				m_listRepeatedVertices = new List<RepeatedVertex>();
				m_listRepeatedVertices.Add(repeatedVertex);
			}

			public void Add(RepeatedVertex repeatedVertex)
			{
				m_listRepeatedVertices.Add(repeatedVertex);
			}
		}

		[SerializeField]
		private List<Vector3> m_listVertices;

		[SerializeField]
		private List<Vector3> m_listVerticesWorld;

		[SerializeField]
		private List<SerializableBoneWeight> m_listBoneWeights;

		[SerializeField]
		private ListIndices[] m_aFaceList;

		public ListIndices[] SubmeshesFaceList
		{
			get
			{
				return m_aFaceList;
			}
		}

		public List<Vector3> ListVertices
		{
			get
			{
				return m_listVertices;
			}
		}

		public List<Vector3> ListVerticesWorld
		{
			get
			{
				return m_listVerticesWorld;
			}
		}

		public List<SerializableBoneWeight> ListBoneWeights
		{
			get
			{
				return m_listBoneWeights;
			}
		}

		public void BuildData(Mesh sourceMesh, Vector3[] av3VerticesWorld)
		{
			Vector3[] vertices = sourceMesh.vertices;
			BoneWeight[] boneWeights = sourceMesh.boneWeights;
			Dictionary<UniqueVertex, RepeatedVertexList> dictionary = new Dictionary<UniqueVertex, RepeatedVertexList>();
			m_listVertices = new List<Vector3>();
			m_listVerticesWorld = new List<Vector3>();
			m_listBoneWeights = new List<SerializableBoneWeight>();
			m_aFaceList = new ListIndices[sourceMesh.subMeshCount];
			for (int i = 0; i < sourceMesh.subMeshCount; i++)
			{
				m_aFaceList[i] = new ListIndices();
				int[] triangles = sourceMesh.GetTriangles(i);
				for (int j = 0; j < triangles.Length; j++)
				{
					UniqueVertex key = new UniqueVertex(vertices[triangles[j]]);
					if (dictionary.ContainsKey(key))
					{
						dictionary[key].Add(new RepeatedVertex(j / 3, triangles[j]));
						m_aFaceList[i].m_listIndices.Add(dictionary[key].UniqueIndex);
						continue;
					}
					int count = m_listVertices.Count;
					dictionary.Add(key, new RepeatedVertexList(count, new RepeatedVertex(j / 3, triangles[j])));
					m_listVertices.Add(vertices[triangles[j]]);
					m_listVerticesWorld.Add(av3VerticesWorld[triangles[j]]);
					m_aFaceList[i].m_listIndices.Add(count);
					if (boneWeights != null && boneWeights.Length > 0)
					{
						m_listBoneWeights.Add(new SerializableBoneWeight(boneWeights[triangles[j]]));
					}
				}
			}
		}
	}
}
