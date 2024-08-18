using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("2D/Star", true)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shape/Star")]
	public class CSStar : CurvyShape2D
	{
		[SerializeField]
		[Positive(Tooltip = "Number of Sides", MinValue = 2f)]
		private int m_Sides = 5;

		[SerializeField]
		[Positive]
		private float m_OuterRadius = 2f;

		[SerializeField]
		[RangeEx(0f, 1f, "", "")]
		private float m_OuterRoundness;

		[SerializeField]
		[Positive]
		private float m_InnerRadius = 1f;

		[SerializeField]
		[RangeEx(0f, 1f, "", "")]
		private float m_InnerRoundness;

		public int Sides
		{
			get
			{
				return m_Sides;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_Sides != num)
				{
					m_Sides = num;
					Dirty = true;
				}
			}
		}

		public float OuterRadius
		{
			get
			{
				return m_OuterRadius;
			}
			set
			{
				float num = Mathf.Max(InnerRadius, value);
				if (m_OuterRadius != num)
				{
					m_OuterRadius = num;
					Dirty = true;
				}
			}
		}

		public float OuterRoundness
		{
			get
			{
				return m_OuterRoundness;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_OuterRoundness != num)
				{
					m_OuterRoundness = num;
					Dirty = true;
				}
			}
		}

		public float InnerRadius
		{
			get
			{
				return m_InnerRadius;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_InnerRadius != num)
				{
					m_InnerRadius = num;
					Dirty = true;
				}
			}
		}

		public float InnerRoundness
		{
			get
			{
				return m_InnerRoundness;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_InnerRoundness != num)
				{
					m_InnerRoundness = num;
					Dirty = true;
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Sides = 5;
			OuterRadius = 2f;
			OuterRoundness = 0f;
			InnerRadius = 1f;
			InnerRoundness = 0f;
		}

		protected override void ApplyShape()
		{
			PrepareSpline(CurvyInterpolation.Bezier);
			PrepareControlPoints(Sides * 2);
			float num = (float)Math.PI * 2f / (float)base.Spline.ControlPointCount;
			for (int i = 0; i < base.Spline.ControlPointCount; i += 2)
			{
				Vector3 vector = new Vector3(Mathf.Sin(num * (float)i), Mathf.Cos(num * (float)i), 0f);
				SetPosition(i, vector * OuterRadius);
				base.Spline.ControlPoints[i].AutoHandleDistance = OuterRoundness;
				vector = new Vector3(Mathf.Sin(num * (float)(i + 1)), Mathf.Cos(num * (float)(i + 1)), 0f);
				SetPosition(i + 1, vector * InnerRadius);
				base.Spline.ControlPoints[i + 1].AutoHandleDistance = InnerRoundness;
			}
		}
	}
}
