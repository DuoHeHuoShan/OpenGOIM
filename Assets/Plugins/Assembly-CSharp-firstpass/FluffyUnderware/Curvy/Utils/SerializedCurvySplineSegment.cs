using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	[Serializable]
	public class SerializedCurvySplineSegment : SerializedCurvyObject<SerializedCurvySplineSegment>
	{
		public Vector3 P;

		public Vector3 R;

		public bool Bake;

		public bool Anchor;

		public CurvyOrientationSwirl Swirl;

		public float SwirlT;

		public bool BzAuto = true;

		public float BzAutoDist = 0.39f;

		public Vector3 BzOut = new Vector3(1f, 0f, 0f);

		public Vector3 BzIn = new Vector3(-1f, 0f, 0f);

		public string Data = string.Empty;

		public SerializedCurvySplineSegment()
		{
		}

		public SerializedCurvySplineSegment(CurvySplineSegment segment, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			if ((bool)segment)
			{
				P = ((space != 0) ? segment.localPosition : segment.position);
				R = ((space != 0) ? segment.localRotation.eulerAngles : segment.rotation.eulerAngles);
				Bake = segment.AutoBakeOrientation;
				Anchor = segment.OrientationAnchor;
				Swirl = segment.Swirl;
				SwirlT = segment.SwirlTurns;
				BzAuto = segment.AutoHandles;
				BzAutoDist = segment.AutoHandleDistance;
				BzOut = segment.HandleOut;
				BzIn = segment.HandleIn;
			}
		}

		public CurvySplineSegment Deserialize(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			if (spline != null)
			{
				if (spline.ControlPointCount > 0)
				{
					return Deserialize(spline.ControlPoints[spline.ControlPointCount - 1], space);
				}
				CurvySplineSegment curvySplineSegment = spline.Add();
				DeserializeInto(curvySplineSegment, space);
				return curvySplineSegment;
			}
			return null;
		}

		public CurvySplineSegment Deserialize(CurvySplineSegment controlPoint, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			if ((bool)controlPoint)
			{
				CurvySplineSegment curvySplineSegment = controlPoint.Spline.InsertAfter(controlPoint);
				DeserializeInto(curvySplineSegment, space);
				return curvySplineSegment;
			}
			return null;
		}

		public void DeserializeInto(CurvySplineSegment controlPoint, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			if ((bool)controlPoint)
			{
				if (space == CurvySerializationSpace.World)
				{
					controlPoint.position = P;
					controlPoint.rotation = Quaternion.Euler(R);
				}
				else
				{
					controlPoint.localPosition = P;
					controlPoint.localRotation = Quaternion.Euler(R);
				}
				controlPoint.AutoBakeOrientation = Bake;
				controlPoint.OrientationAnchor = Anchor;
				controlPoint.Swirl = Swirl;
				controlPoint.SwirlTurns = SwirlT;
				controlPoint.AutoHandles = BzAuto;
				controlPoint.AutoHandleDistance = BzAutoDist;
				controlPoint.SetBezierHandleIn(BzIn);
				controlPoint.SetBezierHandleOut(BzOut);
			}
		}
	}
}
