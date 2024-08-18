using System;
using UnityEngine;

namespace UltimateGameTools.MeshSimplifier
{
	[Serializable]
	public class RelevanceSphere
	{
		public bool m_bExpanded;

		public Vector3 m_v3Position;

		public Vector3 m_v3Rotation;

		public Vector3 m_v3Scale;

		public float m_fRelevance;

		public RelevanceSphere()
		{
			m_v3Scale = Vector3.one;
		}

		public void SetDefault(Transform target, float fRelevance)
		{
			m_bExpanded = true;
			m_v3Position = target.position + Vector3.up;
			m_v3Rotation = target.rotation.eulerAngles;
			m_v3Scale = Vector3.one;
			m_fRelevance = fRelevance;
		}
	}
}
