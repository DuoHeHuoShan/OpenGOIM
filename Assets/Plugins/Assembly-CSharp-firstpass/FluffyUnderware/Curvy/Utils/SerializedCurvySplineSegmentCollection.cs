using System;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Utils
{
	public class SerializedCurvySplineSegmentCollection : SerializedCurvyObject<SerializedCurvySplineSegmentCollection>
	{
		public SerializedCurvySplineSegment[] ControlPoints = new SerializedCurvySplineSegment[0];

		public string Data = string.Empty;

		public SerializedCurvySplineSegmentCollection(List<CurvySplineSegment> cps, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			ControlPoints = new SerializedCurvySplineSegment[cps.Count];
			for (int i = 0; i < cps.Count; i++)
			{
				ControlPoints[i] = new SerializedCurvySplineSegment(cps[i], space);
			}
		}

		public CurvySplineSegment[] Deserialize(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			CurvySplineSegment[] array = new CurvySplineSegment[ControlPoints.Length];
			if (spline != null)
			{
				for (int i = 0; i < ControlPoints.Length; i++)
				{
					array[i] = ControlPoints[i].Deserialize(spline, space);
					if (onDeserializedCP != null)
					{
						onDeserializedCP(array[i], ControlPoints[i].Data);
					}
				}
			}
			return array;
		}

		public CurvySplineSegment[] Deserialize(CurvySplineSegment controlPoint, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			CurvySplineSegment[] array = new CurvySplineSegment[ControlPoints.Length];
			if ((bool)controlPoint)
			{
				CurvySplineSegment curvySplineSegment = controlPoint;
				for (int i = 0; i < ControlPoints.Length; i++)
				{
					curvySplineSegment = (array[i] = ControlPoints[i].Deserialize(curvySplineSegment, space));
					if (onDeserializedCP != null)
					{
						onDeserializedCP(curvySplineSegment, ControlPoints[i].Data);
					}
				}
			}
			return array;
		}
	}
}
