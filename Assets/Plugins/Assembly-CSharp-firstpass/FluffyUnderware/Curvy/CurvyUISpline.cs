using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Curvy/Curvy UI Spline", 2)]
	public class CurvyUISpline : CurvySpline
	{
		public static CurvyUISpline CreateUISpline()
		{
			CurvyUISpline component = new GameObject("Curvy UI Spline", typeof(CurvyUISpline)).GetComponent<CurvyUISpline>();
			component.RestrictTo2D = true;
			component.Orientation = CurvyOrientation.None;
			return component;
		}
	}
}
