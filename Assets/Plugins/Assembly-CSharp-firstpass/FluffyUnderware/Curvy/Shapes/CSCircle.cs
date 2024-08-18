using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("2D/Circle", true)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shape/Circle")]
	public class CSCircle : CurvyShape2D
	{
		[Positive(Tooltip = "Number of Control Points")]
		[SerializeField]
		private int m_Count = 4;

		[SerializeField]
		private float m_Radius = 1f;

		public int Count
		{
			get
			{
				return m_Count;
			}
			set
			{
				int num = Mathf.Max(2, value);
				if (m_Count != num)
				{
					m_Count = num;
					Dirty = true;
				}
			}
		}

		public float Radius
		{
			get
			{
				return m_Radius;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_Radius != num)
				{
					m_Radius = num;
					Dirty = true;
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Count = 4;
			Radius = 1f;
		}

		protected override void ApplyShape()
		{
			PrepareSpline(CurvyInterpolation.Bezier);
			PrepareControlPoints(Count);
			float num = (float)Math.PI * 2f / (float)Count;
			for (int i = 0; i < Count; i++)
			{
				base.Spline.ControlPoints[i].localPosition = new Vector3(Mathf.Sin(num * (float)i) * Radius, Mathf.Cos(num * (float)i) * Radius, 0f);
			}
		}
	}
}
