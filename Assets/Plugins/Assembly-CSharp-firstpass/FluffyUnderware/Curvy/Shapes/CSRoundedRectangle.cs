using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
	[CurvyShapeInfo("2D/Rounded Rectangle", true)]
	[RequireComponent(typeof(CurvySpline))]
	[AddComponentMenu("Curvy/Shape/Rounded Rectangle")]
	public class CSRoundedRectangle : CurvyShape2D
	{
		[Positive]
		[SerializeField]
		private float m_Width = 1f;

		[Positive]
		[SerializeField]
		private float m_Height = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		private float m_Roundness = 0.5f;

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

		public float Roundness
		{
			get
			{
				return m_Roundness;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				if (m_Roundness != num)
				{
					m_Roundness = num;
					Dirty = true;
				}
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Width = 1f;
			Height = 1f;
			Roundness = 0.5f;
		}

		protected override void ApplyShape()
		{
			PrepareSpline(CurvyInterpolation.Bezier);
			PrepareControlPoints(8);
			float num = Width / 2f;
			float num2 = Height / 2f;
			float num3 = Mathf.Min(num, num2) * Roundness;
			SetPosition(0, new Vector3(0f - num, 0f - num2 + num3));
			SetPosition(1, new Vector3(0f - num, num2 - num3));
			SetPosition(2, new Vector3(0f - num + num3, num2));
			SetPosition(3, new Vector3(num - num3, num2));
			SetPosition(4, new Vector3(num, num2 - num3));
			SetPosition(5, new Vector3(num, 0f - num2 + num3));
			SetPosition(6, new Vector3(num - num3, 0f - num2));
			SetPosition(7, new Vector3(0f - num + num3, 0f - num2));
			SetBezierHandles(0, Vector3.down * num3, Vector3.zero, Space.Self);
			SetBezierHandles(1, Vector3.zero, Vector3.up * num3, Space.Self);
			SetBezierHandles(2, Vector3.left * num3, Vector3.right * num3, Space.Self);
			SetBezierHandles(3, Vector3.zero, Vector3.right * num3, Space.Self);
			SetBezierHandles(4, Vector3.up * num3, Vector3.zero, Space.Self);
			SetBezierHandles(5, Vector3.zero, Vector3.down * num3, Space.Self);
			SetBezierHandles(6, Vector3.right * num3, Vector3.zero, Space.Self);
			SetBezierHandles(7, Vector3.zero, Vector3.left * num3, Space.Self);
		}
	}
}
