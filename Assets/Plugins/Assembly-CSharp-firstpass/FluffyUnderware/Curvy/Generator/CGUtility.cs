using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public static class CGUtility
	{
		public static int CalculateSamplePointsCacheSize(int density, params float[] pathlengths)
		{
			density = Mathf.Clamp(density, 0, 100);
			int num = 0;
			for (int i = 0; i < pathlengths.Length; i++)
			{
				num = Mathf.Max(num, CurvySpline.CalculateCacheSize(density, pathlengths[i], CurvyGlobalManager.MaxCachePPU));
			}
			return num;
		}

		public static Vector2[] CalculateUV2(Vector2[] uv)
		{
			Vector2[] array = new Vector2[uv.Length];
			float num = 1f;
			float num2 = 1f;
			for (int i = 0; i < uv.Length; i++)
			{
				num = Mathf.Max(num, uv[i].x);
				num = Mathf.Max(num2, uv[i].y);
			}
			for (int j = 0; j < uv.Length; j++)
			{
				array[j] = new Vector2(uv[j].x / num, uv[j].y / num2);
			}
			return array;
		}

		public static List<ControlPointOption> GetControlPointsWithOptions(CGDataRequestMetaCGOptions options, CurvySpline shape, float startDist, float endDist, bool optimize, out int initialMaterialID, out float initialMaxStep)
		{
			List<ControlPointOption> list = new List<ControlPointOption>();
			initialMaterialID = 0;
			initialMaxStep = float.MaxValue;
			CurvySplineSegment curvySplineSegment = shape.DistanceToSegment(startDist);
			float num = shape.ClampDistance(endDist, shape.IsClosed ? CurvyClamping.Loop : CurvyClamping.Clamp);
			if (num == 0f)
			{
				num = endDist;
			}
			CurvySplineSegment curvySplineSegment2 = ((num != shape.Length) ? shape.DistanceToSegment(num) : shape.LastVisibleControlPoint);
			if (endDist != shape.Length && endDist > curvySplineSegment2.Distance)
			{
				curvySplineSegment2 = curvySplineSegment2.NextControlPoint;
			}
			float num2 = 0f;
			if ((bool)curvySplineSegment)
			{
				MetaCGOptions metadata = curvySplineSegment.GetMetadata<MetaCGOptions>(true);
				initialMaxStep = ((metadata.MaxStepDistance != 0f) ? metadata.MaxStepDistance : float.MaxValue);
				if (options.CheckMaterialID)
				{
					initialMaterialID = metadata.MaterialID;
				}
				int num3 = initialMaterialID;
				float num4 = metadata.MaxStepDistance;
				CurvySplineSegment curvySplineSegment3 = curvySplineSegment.NextSegment ?? curvySplineSegment.NextControlPoint;
				do
				{
					metadata = curvySplineSegment3.GetMetadata<MetaCGOptions>(true);
					if (curvySplineSegment3.ControlPointIndex < curvySplineSegment.ControlPointIndex)
					{
						num2 = shape.Length;
					}
					if (options.IncludeControlPoints || (options.CheckHardEdges && metadata.HardEdge) || (options.CheckMaterialID && metadata.MaterialID != num3) || (optimize && metadata.MaxStepDistance != num4) || (options.CheckExtendedUV && (metadata.UVEdge || metadata.ExplicitU)))
					{
						bool flag = metadata.MaterialID != num3;
						num4 = ((metadata.MaxStepDistance != 0f) ? metadata.MaxStepDistance : float.MaxValue);
						num3 = ((!options.CheckMaterialID) ? initialMaterialID : metadata.MaterialID);
						list.Add(new ControlPointOption(curvySplineSegment3.LocalFToTF(0f) + (float)Mathf.FloorToInt(num2 / shape.Length), curvySplineSegment3.Distance + num2, options.IncludeControlPoints, num3, options.CheckHardEdges && metadata.HardEdge, metadata.MaxStepDistance, (options.CheckExtendedUV && metadata.UVEdge) || flag, options.CheckExtendedUV && metadata.ExplicitU, metadata.FirstU, metadata.SecondU));
					}
					curvySplineSegment3 = curvySplineSegment3.NextSegment;
				}
				while ((bool)curvySplineSegment3 && curvySplineSegment3 != curvySplineSegment2);
				if (options.CheckExtendedUV && !curvySplineSegment3 && curvySplineSegment2.IsLastVisibleControlPoint)
				{
					metadata = curvySplineSegment2.GetMetadata<MetaCGOptions>(true);
					if (metadata.ExplicitU)
					{
						list.Add(new ControlPointOption(1f, curvySplineSegment2.Distance + num2, options.IncludeControlPoints, num3, options.CheckHardEdges && metadata.HardEdge, metadata.MaxStepDistance, (options.CheckExtendedUV && metadata.UVEdge) || (options.CheckMaterialID && metadata.MaterialID != num3), options.CheckExtendedUV && metadata.ExplicitU, metadata.FirstU, metadata.SecondU));
					}
				}
			}
			return list;
		}
	}
}
