using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public struct CGSpot
	{
		[SerializeField]
		[Label("Idx", "")]
		private int m_Index;

		[SerializeField]
		[VectorEx("Pos", "", Options = AttributeOptionsFlags.Compact, Precision = 4)]
		private Vector3 m_Position;

		[SerializeField]
		[VectorEx("Rot", "", Options = AttributeOptionsFlags.Compact, Precision = 4)]
		private Quaternion m_Rotation;

		[SerializeField]
		[VectorEx("Scl", "", Options = AttributeOptionsFlags.Compact, Precision = 4)]
		private Vector3 m_Scale;

		public int Index
		{
			get
			{
				return m_Index;
			}
		}

		public Vector3 Position
		{
			get
			{
				return m_Position;
			}
			set
			{
				if (m_Position != value)
				{
					m_Position = value;
				}
			}
		}

		public Quaternion Rotation
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
				return Matrix4x4.TRS(m_Position, m_Rotation, m_Scale);
			}
		}

		public CGSpot(int index)
			: this(index, Vector3.zero, Quaternion.identity, Vector3.one)
		{
		}

		public CGSpot(int index, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			m_Index = index;
			m_Position = position;
			m_Rotation = rotation;
			m_Scale = scale;
		}

		public void ToTransform(Transform transform)
		{
			transform.localPosition = Position;
			transform.localRotation = Rotation;
			transform.localScale = Scale;
		}
	}
}
