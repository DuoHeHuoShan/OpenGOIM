using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGMeshProperties
	{
		[SerializeField]
		private Mesh m_Mesh;

		[SerializeField]
		private Material[] m_Material = new Material[0];

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Translation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Rotation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Scale = Vector3.one;

		public Mesh Mesh
		{
			get
			{
				return m_Mesh;
			}
			set
			{
				if (m_Mesh != value)
				{
					m_Mesh = value;
				}
				if ((bool)m_Mesh && m_Mesh.subMeshCount != m_Material.Length)
				{
					Array.Resize(ref m_Material, m_Mesh.subMeshCount);
				}
			}
		}

		public Material[] Material
		{
			get
			{
				return m_Material;
			}
			set
			{
				if (m_Material != value)
				{
					m_Material = value;
				}
			}
		}

		public Vector3 Translation
		{
			get
			{
				return m_Translation;
			}
			set
			{
				if (m_Translation != value)
				{
					m_Translation = value;
				}
			}
		}

		public Vector3 Rotation
		{
			get
			{
				return m_Rotation;
			}
			set
			{
				if (m_Rotation != value)
				{
					m_Rotation = value;
				}
			}
		}

		public Vector3 Scale
		{
			get
			{
				return m_Scale;
			}
			set
			{
				if (m_Scale != value)
				{
					m_Scale = value;
				}
			}
		}

		public Matrix4x4 Matrix
		{
			get
			{
				return Matrix4x4.TRS(Translation, Quaternion.Euler(Rotation), Scale);
			}
		}

		public CGMeshProperties()
		{
		}

		public CGMeshProperties(Mesh mesh)
		{
			Mesh = mesh;
			Material = ((!(mesh != null)) ? new Material[0] : new Material[mesh.subMeshCount]);
		}
	}
}
