using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvySplineEventArgs : CurvyEventArgs
	{
		public CurvySpline Spline;

		public CurvySplineEventArgs(MonoBehaviour sender, CurvySpline spline = null, object data = null)
		{
			Sender = sender;
			Spline = spline;
			Data = data;
		}

		public CurvySplineEventArgs()
		{
		}
	}
}
