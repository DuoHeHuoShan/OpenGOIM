using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[ResourceLoader("Spline")]
	public class CGSplineResourceLoader : ICGResourceLoader
	{
		public Component Create(CGModule module, string context)
		{
			CurvySpline curvySpline = CurvySpline.Create();
			curvySpline.transform.position = Vector3.zero;
			curvySpline.Closed = true;
			curvySpline.Add(new Vector3(0f, 0f, 0f), new Vector3(5f, 0f, 10f), new Vector3(-5f, 0f, 10f));
			return curvySpline;
		}

		public void Destroy(CGModule module, Component obj, string context, bool kill)
		{
			if (obj != null)
			{
				Object.Destroy(obj);
			}
		}
	}
}
