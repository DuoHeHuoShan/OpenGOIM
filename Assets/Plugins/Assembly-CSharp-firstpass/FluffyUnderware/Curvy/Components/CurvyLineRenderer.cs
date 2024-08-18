using System.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[AddComponentMenu("Curvy/Misc/Curvy Line Renderer")]
	[RequireComponent(typeof(LineRenderer))]
	[ExecuteInEditMode]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/curvylinerenderer")]
	public class CurvyLineRenderer : MonoBehaviour
	{
		public CurvySplineBase m_Spline;

		private LineRenderer mRenderer;

		public CurvySplineBase Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					unbindEvents();
					m_Spline = value;
					bindEvents();
					Refresh();
				}
			}
		}

		private void Awake()
		{
			mRenderer = GetComponent<LineRenderer>();
			m_Spline = GetComponent<CurvySpline>();
			if (!m_Spline)
			{
				m_Spline = GetComponent<CurvySplineGroup>();
			}
		}

		private void OnEnable()
		{
			mRenderer = GetComponent<LineRenderer>();
			bindEvents();
		}

		private void OnDisable()
		{
			unbindEvents();
		}

		private IEnumerator Start()
		{
			if (Spline != null)
			{
				while (!Spline.IsInitialized)
				{
					yield return 0;
				}
			}
			Refresh();
		}

		public void Refresh()
		{
			if ((bool)Spline && Spline.IsInitialized)
			{
				Vector3[] approximation = Spline.GetApproximation();
				mRenderer.positionCount = approximation.Length;
				mRenderer.SetPositions(approximation);
			}
			else if (mRenderer != null)
			{
				mRenderer.positionCount = 0;
			}
		}

		private void OnSplineRefresh(CurvySplineEventArgs e)
		{
			Refresh();
		}

		private void bindEvents()
		{
			if ((bool)Spline)
			{
				Spline.OnRefresh.AddListenerOnce(OnSplineRefresh);
			}
		}

		private void unbindEvents()
		{
			if ((bool)Spline)
			{
				Spline.OnRefresh.RemoveListener(OnSplineRefresh);
			}
		}
	}
}
