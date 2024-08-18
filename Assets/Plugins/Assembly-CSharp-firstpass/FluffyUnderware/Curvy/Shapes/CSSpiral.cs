using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("3D/Spiral", false)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shape/Spiral")]
	public class CSSpiral : CurvyShape2D
	{
		[Positive(Tooltip = "Number of Control Points per full Circle")]
		[SerializeField]
		private int m_Count = 8;

		[Positive(Tooltip = "Number of Full Circles")]
		[SerializeField]
		private float m_Circles = 3f;

		[Positive(Tooltip = "Base Radius")]
		[SerializeField]
		private float m_Radius = 5f;

		[Label(Tooltip = "Radius Multiplicator")]
		[SerializeField]
		private AnimationCurve m_RadiusFactor = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField]
		private AnimationCurve m_Z = AnimationCurve.Linear(0f, 0f, 1f, 10f);

		public int Count
		{
			get
			{
				return m_Count;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_Count != num)
				{
					m_Count = num;
					Dirty = true;
				}
			}
		}

		public float Circles
		{
			get
			{
				return m_Circles;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_Circles != num)
				{
					m_Circles = num;
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

		public AnimationCurve RadiusFactor
		{
			get
			{
				return m_RadiusFactor;
			}
			set
			{
				if (m_RadiusFactor != value)
				{
					m_RadiusFactor = value;
					Dirty = true;
				}
			}
		}

		public AnimationCurve Z
		{
			get
			{
				return m_Z;
			}
			set
			{
				if (m_Z != value)
				{
					m_Z = value;
					Dirty = true;
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Count = 8;
			Circles = 3f;
			Radius = 5f;
			RadiusFactor = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			Z = AnimationCurve.Linear(0f, 0f, 1f, 10f);
		}

		protected override void ApplyShape()
		{
			PrepareSpline(CurvyInterpolation.CatmullRom, CurvyOrientation.Dynamic, 50, false);
			base.Spline.RestrictTo2D = false;
			int num = Mathf.FloorToInt((float)Count * Circles);
			PrepareControlPoints(num);
			if (num != 0)
			{
				float num2 = (float)Math.PI * 2f / (float)Count;
				for (int i = 0; i < num; i++)
				{
					float time = (float)i / (float)num;
					float num3 = Radius * RadiusFactor.Evaluate(time);
					SetPosition(i, new Vector3(Mathf.Sin(num2 * (float)i) * num3, Mathf.Cos(num2 * (float)i) * num3, m_Z.Evaluate(time)));
				}
			}
		}
	}
}
