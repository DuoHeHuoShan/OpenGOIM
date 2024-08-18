using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class TRSModuleBase : CGModule
	{
		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Transpose;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Rotation;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Scale = Vector3.one;

		public Vector3 Transpose
		{
			get
			{
				return m_Transpose;
			}
			set
			{
				if (m_Transpose != value)
				{
					m_Transpose = value;
				}
				base.Dirty = true;
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
				base.Dirty = true;
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
				base.Dirty = true;
			}
		}

		public Matrix4x4 Matrix
		{
			get
			{
				return Matrix4x4.TRS(Transpose, Quaternion.Euler(Rotation), Scale);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 200f;
			Properties.LabelWidth = 50f;
		}
	}
}
