using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	[Serializable]
	public class SerializedCurvySpline : SerializedCurvyObject<SerializedCurvySpline>
	{
		public string Name = string.Empty;

		public Vector3 P;

		public Vector3 R;

		public CurvyInterpolation Interpolation = CurvyGlobalManager.DefaultInterpolation;

		public bool Keep2D;

		public bool Closed;

		public bool AutoEndTangents = true;

		public CurvyOrientation Orientation = CurvyOrientation.Dynamic;

		public float BzAutoDist = 0.39f;

		public int CacheDensity = 50;

		public bool Pooling = true;

		public bool Threading;

		public bool CheckTForm;

		public CurvyUpdateMethod UpdateIn;

		public SerializedCurvySplineSegment[] ControlPoints = new SerializedCurvySplineSegment[0];

		public string Data = string.Empty;

		public SerializedCurvySpline()
		{
		}

		public SerializedCurvySpline(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			if ((bool)spline)
			{
				Name = spline.name;
				P = ((space != CurvySerializationSpace.Self) ? spline.transform.position : spline.transform.localPosition);
				R = ((space != CurvySerializationSpace.Self) ? spline.transform.rotation.eulerAngles : spline.transform.localRotation.eulerAngles);
				Interpolation = spline.Interpolation;
				Keep2D = spline.RestrictTo2D;
				Closed = spline.Closed;
				AutoEndTangents = spline.AutoEndTangents;
				Orientation = spline.Orientation;
				BzAutoDist = spline.AutoHandleDistance;
				CacheDensity = spline.CacheDensity;
				Pooling = spline.UsePooling;
				Threading = spline.UseThreading;
				CheckTForm = spline.CheckTransform;
				UpdateIn = spline.UpdateIn;
				ControlPoints = new SerializedCurvySplineSegment[spline.ControlPointCount];
				for (int i = 0; i < spline.ControlPointCount; i++)
				{
					ControlPoints[i] = new SerializedCurvySplineSegment(spline.ControlPoints[i]);
				}
			}
		}

		public CurvySpline Deserialize(Transform parent = null, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			CurvySpline curvySpline = CurvySpline.Create();
			if (!string.IsNullOrEmpty(Name))
			{
				curvySpline.name = Name;
			}
			curvySpline.transform.SetParent(parent);
			DeserializeInto(curvySpline, space, onDeserializedCP);
			return curvySpline;
		}

		public CurvySpline Deserialize(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			if (!string.IsNullOrEmpty(Name))
			{
				spline.name = Name;
			}
			DeserializeInto(spline, space, onDeserializedCP);
			return spline;
		}

		public void DeserializeInto(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			if (!spline)
			{
				return;
			}
			if (!string.IsNullOrEmpty(Name))
			{
				spline.name = Name;
			}
			spline.Clear();
			if (space == CurvySerializationSpace.Self)
			{
				spline.transform.localPosition = P;
				spline.transform.localRotation = Quaternion.Euler(R);
			}
			else
			{
				spline.transform.position = P;
				spline.transform.rotation = Quaternion.Euler(R);
			}
			spline.Interpolation = Interpolation;
			spline.RestrictTo2D = Keep2D;
			spline.Closed = Closed;
			spline.AutoEndTangents = AutoEndTangents;
			spline.Orientation = Orientation;
			spline.AutoHandleDistance = BzAutoDist;
			spline.CacheDensity = CacheDensity;
			spline.UsePooling = Pooling;
			spline.UseThreading = Threading;
			spline.CheckTransform = CheckTForm;
			spline.UpdateIn = UpdateIn;
			SerializedCurvySplineSegment[] controlPoints = ControlPoints;
			foreach (SerializedCurvySplineSegment serializedCurvySplineSegment in controlPoints)
			{
				CurvySplineSegment arg = serializedCurvySplineSegment.Deserialize(spline, space);
				if (onDeserializedCP != null)
				{
					onDeserializedCP(arg, serializedCurvySplineSegment.Data);
				}
			}
		}
	}
}
