using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGBoundsGroupItem
	{
		public int Index;

		[RangeEx(0f, 1f, "", "", Slider = true, Precision = 1)]
		[SerializeField]
		private float m_Weight = 0.5f;

		public float Weight
		{
			get
			{
				return m_Weight;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				if (m_Weight != num)
				{
					m_Weight = num;
				}
			}
		}
	}
}
