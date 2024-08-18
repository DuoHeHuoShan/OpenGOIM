using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("2D/Rectangle", true)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shape/Rectangle")]
	public class CSRectangle : CurvyShape2D
	{
		[Positive]
		[SerializeField]
		private float m_Width = 1f;

		[Positive]
		[SerializeField]
		private float m_Height = 1f;

		public float Width
		{
			get
			{
				return m_Width;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_Width != num)
				{
					m_Width = num;
					Dirty = true;
				}
			}
		}

		public float Height
		{
			get
			{
				return m_Height;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_Height != num)
				{
					m_Height = num;
					Dirty = true;
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Width = 1f;
			Height = 1f;
		}

		protected override void ApplyShape()
		{
			base.ApplyShape();
			PrepareSpline(CurvyInterpolation.Linear, CurvyOrientation.Static, 1);
			PrepareControlPoints(4);
			float num = Width / 2f;
			float num2 = Height / 2f;
			SetCGHardEdges();
			SetPosition(0, new Vector3(0f - num, 0f - num2));
			SetPosition(1, new Vector3(0f - num, num2));
			SetPosition(2, new Vector3(num, num2));
			SetPosition(3, new Vector3(num, 0f - num2));
		}
	}
}
