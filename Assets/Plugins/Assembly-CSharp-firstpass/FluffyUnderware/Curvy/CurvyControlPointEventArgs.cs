using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyControlPointEventArgs : CurvySplineEventArgs
	{
		public enum ModeEnum
		{
			AddBefore = 0,
			AddAfter = 1,
			Delete = 2,
			None = 3,
			Added = 4
		}

		public ModeEnum Mode;

		public CurvySplineSegment ControlPoint;

		public CurvyControlPointEventArgs(MonoBehaviour sender, CurvySpline spline, CurvySplineSegment cp, ModeEnum mode = ModeEnum.None, object data = null)
			: base(sender, spline, data)
		{
			ControlPoint = cp;
			Mode = mode;
		}

		public CurvyControlPointEventArgs(CurvySpline spline)
			: base(spline)
		{
			Mode = ModeEnum.AddAfter;
		}

		public CurvyControlPointEventArgs()
		{
		}
	}
}
