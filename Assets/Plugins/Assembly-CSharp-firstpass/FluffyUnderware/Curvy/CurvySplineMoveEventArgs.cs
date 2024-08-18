using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvySplineMoveEventArgs : CurvyControlPointEventArgs
	{
		public bool WorldUnits;

		public int Direction;

		public float Delta;

		public float TF;

		public CurvySplineMoveEventArgs(MonoBehaviour sender, CurvySpline spline, CurvySplineSegment cp, float tf, float delta, int dir, bool worldUnits = false)
			: base(sender, spline, cp)
		{
			Sender = sender;
			Direction = dir;
			Delta = delta;
			TF = tf;
			WorldUnits = worldUnits;
		}

		public CurvySplineMoveEventArgs()
		{
		}

		public void SetPosition(float tf)
		{
			TF = tf;
			if ((bool)Spline)
			{
				ControlPoint = Spline.TFToSegment(tf);
			}
		}

		public void SetPosition(CurvySplineSegment segment, float localF = 0f)
		{
			ControlPoint = segment;
			Spline = segment.Spline;
			TF = Spline.SegmentToTF(ControlPoint, localF);
		}

		public void Follow(CurvySplineSegment controlPoint, ConnectionHeadingEnum direction = ConnectionHeadingEnum.Auto)
		{
			if (!WorldUnits)
			{
				Delta *= Spline.Length / controlPoint.Spline.Length;
			}
			float num = controlPoint.LocalFToTF(0f);
			SetPosition(controlPoint);
			if (num == 0f && Direction == -1 && Spline.Closed)
			{
				num = 1f;
			}
			if (num == 0f)
			{
				Direction = 1;
				return;
			}
			if (num == 1f)
			{
				Direction = -1;
				return;
			}
			switch (direction)
			{
			case ConnectionHeadingEnum.Minus:
				Direction = -1;
				break;
			case ConnectionHeadingEnum.Plus:
				Direction = 1;
				break;
			}
		}

		public float AngleTo(CurvySplineSegment controlPoint)
		{
			float result = 0f;
			if ((bool)Spline && (bool)controlPoint)
			{
				Vector3 tangentFast = Spline.GetTangentFast(TF);
				Vector3 tangentFast2 = controlPoint.Spline.GetTangentFast(controlPoint.LocalFToTF(0f));
				if (!controlPoint.Spline.Closed && ((controlPoint.IsFirstVisibleControlPoint && Direction == -1) || (controlPoint.IsLastVisibleControlPoint && Direction == 1)))
				{
					tangentFast2 *= -1f;
				}
				result = Vector3.Angle(tangentFast, tangentFast2);
			}
			return result;
		}
	}
}
