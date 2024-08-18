using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class SerializedCurvySplineCollection : SerializedCurvyObject<SerializedCurvySplineCollection>
	{
		public SerializedCurvySpline[] Splines = new SerializedCurvySpline[0];

		public string Data = string.Empty;

		public SerializedCurvySplineCollection(List<CurvySpline> splines, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
		{
			Splines = new SerializedCurvySpline[splines.Count];
			for (int i = 0; i < splines.Count; i++)
			{
				Splines[i] = new SerializedCurvySpline(splines[i], space);
			}
		}

		public CurvySpline[] Deserialize(Transform parent = null, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySpline, string> onDeserializedSpline = null, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			CurvySpline[] array = new CurvySpline[Splines.Length];
			for (int i = 0; i < Splines.Length; i++)
			{
				array[i] = Splines[i].Deserialize(parent, space, onDeserializedCP);
				if (onDeserializedSpline != null)
				{
					onDeserializedSpline(array[i], Splines[i].Data);
				}
			}
			return array;
		}

		public CurvySpline[] Deserialize(CurvySpline[] splines, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySpline, string> onDeserializedSpline = null, Action<CurvySplineSegment, string> onDeserializedCP = null)
		{
			CurvySpline[] array = new CurvySpline[Splines.Length];
			for (int i = 0; i < Splines.Length; i++)
			{
				array[i] = Splines[i].Deserialize(splines[i], space, onDeserializedCP);
				if (onDeserializedSpline != null)
				{
					onDeserializedSpline(array[i], Splines[i].Data);
				}
			}
			return array;
		}
	}
}
