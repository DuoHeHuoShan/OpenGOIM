using System;
using System.Collections.Generic;
using System.IO;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyImportExport : MonoBehaviour
	{
		public enum ExportOptions
		{
			Splines = 0,
			ControlPoints = 1
		}

		public enum ImportOptions
		{
			Create = 0,
			Apply = 1,
			Insert = 2
		}

		[Section("General", true, false, 100)]
		[FieldCondition("FilePath", "", false, ActionAttribute.ActionEnum.ShowWarning, "Missing File Path", ActionAttribute.ActionPositionEnum.Below)]
		[PathSelector(PathSelectorAttribute.DialogMode.CreateFile, Title = "Select File")]
		public string FilePath;

		public CurvySerializationSpace Space = CurvySerializationSpace.WorldSpline;

		[Section("Import", true, false, 100)]
		public ImportOptions Mode;

		[FieldCondition("Mode", ImportOptions.Create, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		public Transform Target;

		[FieldCondition("Mode", ImportOptions.Apply, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[ArrayEx]
		public CurvySpline[] ApplyTo;

		[FieldCondition("Mode", ImportOptions.Insert, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		public CurvySplineSegment InsertAfter;

		[FieldAction("ShowImportButton", ActionAttribute.ActionEnum.Callback)]
		public CurvySplineEvent OnDeserializedSpline;

		public CurvyControlPointEvent OnDeserializedCP;

		[Section("Export", true, false, 100)]
		public ExportOptions ExportOption;

		[FieldCondition("ExportOption", ExportOptions.Splines, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[FieldAction("ShowExportButton", ActionAttribute.ActionEnum.Callback, ShowBelowProperty = true)]
		[ArrayEx]
		public List<CurvySpline> SourceSplines;

		[FieldCondition("ExportOption", ExportOptions.ControlPoints, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[FieldAction("ShowExportButton", ActionAttribute.ActionEnum.Callback, ShowBelowProperty = true)]
		[ArrayEx]
		public List<CurvySplineSegment> SourceControlPoints;

		private static Action<CurvySpline, string> mOnDeserializedSpline;

		private static Action<CurvySplineSegment, string> mOnDeserializedCP;

		public void Import()
		{
			if (OnDeserializedSpline.HasListeners())
			{
				mOnDeserializedSpline = delegate(CurvySpline x, string y)
				{
					OnDeserializedSpline.Invoke(new CurvySplineEventArgs(this, x, y));
				};
			}
			else
			{
				mOnDeserializedSpline = null;
			}
			if (OnDeserializedCP.HasListeners())
			{
				mOnDeserializedCP = delegate(CurvySplineSegment x, string y)
				{
					OnDeserializedCP.Invoke(new CurvyControlPointEventArgs(this, x.Spline, x, CurvyControlPointEventArgs.ModeEnum.Added, y));
				};
			}
			else
			{
				mOnDeserializedCP = null;
			}
			ImportFromFile(FilePath, Target, Space);
		}

		public void Export()
		{
		}

		public static void Deserialize(string json, Transform target, CurvySerializationSpace space)
		{
			if (SerializedCurvyObjectHelper.GetJsonSerializedType(json) == typeof(SerializedCurvySplineCollection))
			{
				SerializedCurvySplineCollection serializedCurvySplineCollection = SerializedCurvyObject<SerializedCurvySplineCollection>.FromJson(json);
				serializedCurvySplineCollection.Deserialize(target, space, mOnDeserializedSpline, mOnDeserializedCP);
			}
			else
			{
				DTLog.LogWarning("[Curvy] CurvyImportExport.Deserialize: Data isn't of type 'SerializedCurvySplineCollection'!");
			}
		}

		public static void Deserialize(string json, CurvySpline[] applyTo, CurvySerializationSpace space)
		{
			if (SerializedCurvyObjectHelper.GetJsonSerializedType(json) == typeof(SerializedCurvySplineCollection))
			{
				SerializedCurvySplineCollection serializedCurvySplineCollection = SerializedCurvyObject<SerializedCurvySplineCollection>.FromJson(json);
				serializedCurvySplineCollection.Deserialize(applyTo, space, mOnDeserializedSpline, mOnDeserializedCP);
			}
			else
			{
				DTLog.LogWarning("[Curvy] CurvyImportExport.Deserialize: Data isn't of type 'SerializedCurvySplineCollection'!");
			}
		}

		public static void Deserialize(string json, CurvySplineSegment insertAfter, CurvySerializationSpace space)
		{
			if (SerializedCurvyObjectHelper.GetJsonSerializedType(json) == typeof(SerializedCurvySplineSegmentCollection))
			{
				SerializedCurvySplineSegmentCollection serializedCurvySplineSegmentCollection = SerializedCurvyObject<SerializedCurvySplineSegmentCollection>.FromJson(json);
				serializedCurvySplineSegmentCollection.Deserialize(insertAfter, space, mOnDeserializedCP);
			}
			else
			{
				DTLog.LogWarning("[Curvy] CurvyImportExport.Deserialize: Data isn't of type 'SerializedCurvySplineSegmentCollection'!");
			}
		}

		public static string Serialize(CurvySerializationSpace space, params CurvySpline[] splines)
		{
			return new SerializedCurvySplineCollection(new List<CurvySpline>(splines), space).ToJson();
		}

		public static string Serialize(CurvySerializationSpace space, params CurvySplineSegment[] controlPoints)
		{
			return new SerializedCurvySplineSegmentCollection(new List<CurvySplineSegment>(controlPoints), space).ToJson();
		}

		public static void ImportFromFile(string filePath, Transform target, CurvySerializationSpace space)
		{
			Deserialize(loadFile(filePath), target, space);
		}

		public static void ImportFromFile(string filePath, CurvySpline[] applyTo, CurvySerializationSpace space)
		{
			Deserialize(loadFile(filePath), applyTo, space);
		}

		public static void ImportFromFile(string filePath, CurvySplineSegment insertAfter, CurvySerializationSpace space)
		{
			Deserialize(loadFile(filePath), insertAfter, space);
		}

		public static void ExportToFile(string filePath, CurvySerializationSpace space, params CurvySpline[] splines)
		{
			saveFile(Serialize(space, splines), filePath);
		}

		public static void ExportToFile(string filePath, CurvySerializationSpace space, params CurvySplineSegment[] controlPoints)
		{
			saveFile(Serialize(space, controlPoints), filePath);
		}

		private static void saveFile(string data, string filePath)
		{
			File.WriteAllText(filePath, data);
		}

		private static string loadFile(string filePath)
		{
			return File.ReadAllText(filePath).Replace("\n", string.Empty);
		}
	}
}
