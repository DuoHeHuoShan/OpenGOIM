using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public static class SerializedCurvyObjectHelper
	{
		public static Type GetJsonSerializedType(string json)
		{
			json = json.Substring(0, Mathf.Min(json.Length, 70));
			json = Regex.Replace(json, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
			json = Regex.Replace(json, "[\f\n\r\t\v]", string.Empty);
			if (json.StartsWith("{\"ControlPoints\":["))
			{
				return typeof(SerializedCurvySplineSegmentCollection);
			}
			if (json.StartsWith("{\"Splines\":["))
			{
				return typeof(SerializedCurvySplineCollection);
			}
			return null;
		}
	}
}
