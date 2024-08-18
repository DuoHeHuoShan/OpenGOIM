using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class SplineInputModuleBase : CGModule
	{
		[Tab("General")]
		[SerializeField]
		private bool m_UseCache;

		[Tab("Range")]
		[SerializeField]
		private CurvySplineSegment m_StartCP;

		[FieldCondition("m_StartCP", null, true, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		private CurvySplineSegment m_EndCP;

		public bool UseCache
		{
			get
			{
				return m_UseCache;
			}
			set
			{
				if (m_UseCache != value)
				{
					m_UseCache = value;
				}
				base.Dirty = true;
			}
		}

		public CurvySplineSegment StartCP
		{
			get
			{
				return m_StartCP;
			}
			set
			{
				if (m_StartCP != value)
				{
					m_StartCP = value;
				}
				base.Dirty = true;
			}
		}

		public CurvySplineSegment EndCP
		{
			get
			{
				return m_EndCP;
			}
			set
			{
				CurvySplineSegment curvySplineSegment = value;
				if ((bool)curvySplineSegment && (bool)StartCP && curvySplineSegment.ControlPointIndex <= StartCP.ControlPointIndex)
				{
					curvySplineSegment = null;
				}
				if (m_EndCP != curvySplineSegment)
				{
					m_EndCP = curvySplineSegment;
				}
				base.Dirty = true;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
		}

		private void getRange(CurvySpline spline, CGDataRequestRasterization raster, out float startDist, out float endDist)
		{
			if ((bool)StartCP)
			{
				float pathLength = getPathLength(spline);
				startDist = StartCP.Distance + pathLength * raster.Start;
				endDist = StartCP.Distance + pathLength * (raster.Start + raster.Length);
			}
			else
			{
				startDist = spline.Length * raster.Start;
				endDist = spline.Length * (raster.Start + raster.Length);
			}
		}

		protected float getPathLength(CurvySpline spline)
		{
			if (!spline)
			{
				return 0f;
			}
			if ((bool)StartCP && (bool)EndCP)
			{
				return EndCP.Distance - StartCP.Distance;
			}
			return spline.Length;
		}

		protected bool getPathClosed(CurvySpline spline)
		{
			if (!spline || !spline.Closed)
			{
				return false;
			}
			return EndCP == null;
		}

		protected CGData GetSplineData(CurvySpline spline, bool fullPath, CGDataRequestRasterization raster, CGDataRequestMetaCGOptions options)
		{
			if (spline == null || spline.Count == 0)
			{
				return null;
			}
			List<ControlPointOption> list = new List<ControlPointOption>();
			int initialMaterialID = 0;
			float initialMaxStep = float.MaxValue;
			CGShape data = ((!fullPath) ? new CGShape() : new CGPath());
			float startDist;
			float endDist;
			getRange(spline, raster, out startDist, out endDist);
			raster.Resolution = Mathf.Max(raster.Resolution, 2);
			float movedDistance = (endDist - startDist) / (float)(raster.Resolution - 1);
			data.Length = endDist - startDist;
			float tf = spline.DistanceToTF(startDist);
			float startTF = tf;
			float num = ((!(endDist > spline.Length) || !spline.Closed) ? spline.DistanceToTF(endDist) : (spline.DistanceToTF(endDist - spline.Length) + 1f));
			data.SourceIsManaged = IsManagedResource(spline);
			data.Closed = spline.Closed;
			data.Seamless = spline.Closed && raster.Length == 1f;
			if (data.Length == 0f)
			{
				return data;
			}
			if ((bool)options)
			{
				list = CGUtility.GetControlPointsWithOptions(options, spline, startDist, endDist, raster.Mode == CGDataRequestRasterization.ModeEnum.Optimized, out initialMaterialID, out initialMaxStep);
			}
			List<SamplePointUData> list2 = new List<SamplePointUData>();
			List<Vector3> pos = new List<Vector3>();
			List<float> relF = new List<float>();
			List<float> sourceF = new List<float>();
			List<Vector3> tan = new List<Vector3>();
			List<Vector3> up = new List<Vector3>();
			float curDist = startDist;
			Vector3 curTan = Vector3.zero;
			Vector3 item = Vector3.zero;
			List<int> list3 = new List<int>();
			int num2 = 100000;
			Vector3 curPos;
			switch (raster.Mode)
			{
			case CGDataRequestRasterization.ModeEnum.Even:
			{
				bool flag = false;
				SamplePointsMaterialGroup samplePointsMaterialGroup = new SamplePointsMaterialGroup(initialMaterialID);
				SamplePointsPatch item2 = new SamplePointsPatch(0);
				CurvyClamping clamping = (data.Closed ? CurvyClamping.Loop : CurvyClamping.Clamp);
				while (curDist <= endDist && --num2 > 0)
				{
					tf = spline.DistanceToTF(spline.ClampDistance(curDist, clamping));
					curPos = ((!UseCache) ? spline.Interpolate(tf) : spline.InterpolateFast(tf));
					float num3 = (curDist - startDist) / data.Length;
					if (Mathf.Approximately(1f, num3))
					{
						num3 = 1f;
					}
					pos.Add(curPos);
					relF.Add(num3);
					sourceF.Add(curDist / spline.Length);
					if (fullPath)
					{
						curTan = ((!UseCache) ? spline.GetTangent(tf, curPos) : spline.GetTangentFast(tf));
						item = spline.GetOrientationUpFast(tf);
						tan.Add(curTan);
						up.Add(item);
					}
					if (flag)
					{
						pos.Add(curPos);
						relF.Add(num3);
						sourceF.Add(curDist / spline.Length);
						if (fullPath)
						{
							tan.Add(curTan);
							up.Add(item);
						}
						flag = false;
					}
					curDist += movedDistance;
					if (list.Count > 0 && curDist >= list[0].Distance)
					{
						if (list[0].UVEdge || list[0].UVShift)
						{
							list2.Add(new SamplePointUData(pos.Count, list[0].UVEdge, list[0].FirstU, list[0].SecondU));
						}
						curDist = list[0].Distance;
						flag = list[0].HardEdge || list[0].MaterialID != samplePointsMaterialGroup.MaterialID || (options.CheckExtendedUV && list[0].UVEdge);
						if (flag)
						{
							item2.End = pos.Count;
							samplePointsMaterialGroup.Patches.Add(item2);
							if (samplePointsMaterialGroup.MaterialID != list[0].MaterialID)
							{
								data.MaterialGroups.Add(samplePointsMaterialGroup);
								samplePointsMaterialGroup = new SamplePointsMaterialGroup(list[0].MaterialID);
							}
							item2 = new SamplePointsPatch(pos.Count + 1);
							if (!list[0].HardEdge)
							{
								list3.Add(pos.Count + 1);
							}
							if (list[0].UVEdge || list[0].UVShift)
							{
								list2.Add(new SamplePointUData(pos.Count + 1, list[0].UVEdge, list[0].FirstU, list[0].SecondU));
							}
						}
						list.RemoveAt(0);
					}
					if (curDist > endDist && num3 < 1f)
					{
						curDist = endDist;
					}
				}
				if (num2 <= 0)
				{
					Debug.LogError("[Curvy] He's dead, Jim! Deadloop in SplineInputModuleBase.GetSplineData (Even)! Please send a bug report!");
				}
				item2.End = pos.Count - 1;
				samplePointsMaterialGroup.Patches.Add(item2);
				if (data.Closed && !spline[0].GetMetadata<MetaCGOptions>(true).HardEdge)
				{
					list3.Add(0);
				}
				data.MaterialGroups.Add(samplePointsMaterialGroup);
				data.SourceF = sourceF.ToArray();
				data.F = relF.ToArray();
				data.Position = pos.ToArray();
				if (fullPath)
				{
					((CGPath)data).Direction = tan.ToArray();
					data.Normal = up.ToArray();
				}
				break;
			}
			case CGDataRequestRasterization.ModeEnum.Optimized:
			{
				bool flag = false;
				SamplePointsMaterialGroup samplePointsMaterialGroup = new SamplePointsMaterialGroup(initialMaterialID);
				SamplePointsPatch item2 = new SamplePointsPatch(0);
				float stepDist = movedDistance / spline.Length;
				float angleThreshold = raster.AngleThreshold;
				curPos = spline.Interpolate(tf);
				curTan = spline.GetTangent(tf, curPos);
				Action<float> action = delegate(float f)
				{
					sourceF.Add(curDist / spline.Length);
					pos.Add(curPos);
					relF.Add((curDist - startDist) / data.Length);
					if (fullPath)
					{
						tan.Add(curTan);
						up.Add(spline.GetOrientationUpFast(f));
					}
				};
				while (tf < num && num2-- > 0)
				{
					action(tf % 1f);
					float stopTF = ((list.Count <= 0) ? num : list[0].TF);
					bool flag2 = spline.MoveByAngleExtINTERNAL(ref tf, base.Generator.MinDistance, initialMaxStep, angleThreshold, out curPos, out curTan, out movedDistance, stopTF, data.Closed, stepDist);
					curDist += movedDistance;
					if (Mathf.Approximately(tf, num) || tf > num)
					{
						curDist = endDist;
						num = ((!data.Closed) ? Mathf.Clamp01(num) : DTMath.Repeat(num, 1f));
						curPos = spline.Interpolate(num);
						if (fullPath)
						{
							curTan = spline.GetTangent(num, curPos);
						}
						action(num);
						break;
					}
					if (!flag2)
					{
						continue;
					}
					if (list.Count > 0)
					{
						if (list[0].UVEdge || list[0].UVShift)
						{
							list2.Add(new SamplePointUData(pos.Count, list[0].UVEdge, list[0].FirstU, list[0].SecondU));
						}
						curDist = list[0].Distance;
						initialMaxStep = list[0].MaxStepDistance;
						if (list[0].HardEdge || list[0].MaterialID != samplePointsMaterialGroup.MaterialID || (options.CheckExtendedUV && list[0].UVEdge))
						{
							item2.End = pos.Count;
							samplePointsMaterialGroup.Patches.Add(item2);
							if (samplePointsMaterialGroup.MaterialID != list[0].MaterialID)
							{
								data.MaterialGroups.Add(samplePointsMaterialGroup);
								samplePointsMaterialGroup = new SamplePointsMaterialGroup(list[0].MaterialID);
							}
							item2 = new SamplePointsPatch(pos.Count + 1);
							if (!list[0].HardEdge)
							{
								list3.Add(pos.Count + 1);
							}
							if (list[0].UVEdge || list[0].UVShift)
							{
								list2.Add(new SamplePointUData(pos.Count + 1, list[0].UVEdge, list[0].FirstU, list[0].SecondU));
							}
							action(tf);
						}
						list.RemoveAt(0);
						continue;
					}
					action(tf);
					break;
				}
				if (num2 <= 0)
				{
					Debug.LogError("[Curvy] He's dead, Jim! Deadloop in SplineInputModuleBase.GetSplineData (Optimized)! Please send a bug report!");
				}
				item2.End = pos.Count - 1;
				samplePointsMaterialGroup.Patches.Add(item2);
				if (list.Count > 0 && list[0].UVShift)
				{
					list2.Add(new SamplePointUData(pos.Count - 1, list[0].UVEdge, list[0].FirstU, list[0].SecondU));
				}
				if (data.Closed && !spline[0].GetMetadata<MetaCGOptions>(true).HardEdge)
				{
					list3.Add(0);
				}
				data.MaterialGroups.Add(samplePointsMaterialGroup);
				data.SourceF = sourceF.ToArray();
				data.F = relF.ToArray();
				data.Position = pos.ToArray();
				data.Bounds = spline.Bounds;
				if (fullPath)
				{
					((CGPath)data).Direction = tan.ToArray();
					data.Normal = up.ToArray();
				}
				break;
			}
			}
			data.Map = (float[])data.F.Clone();
			if (!fullPath)
			{
				data.RecalculateNormals(list3);
				if ((bool)options && options.CheckExtendedUV)
				{
					CalculateExtendedUV(spline, startTF, num, list2, data);
				}
			}
			return data;
		}

		private void CalculateExtendedUV(CurvySpline spline, float startTF, float endTF, List<SamplePointUData> ext, CGShape data)
		{
			CurvySplineSegment cp;
			MetaCGOptions metaCGOptions = findPreviousReferenceCPOptions(spline, startTF, out cp);
			CurvySplineSegment cp2;
			MetaCGOptions metaCGOptions2 = findNextReferenceCPOptions(spline, startTF, out cp2);
			ext.Insert(0, new SamplePointUData(uv0: ((!cp2.IsFirstVisibleControlPoint) ? ((data.SourceF[0] * spline.Length - cp.Distance) / (cp2.Distance - cp.Distance)) : ((data.SourceF[0] * spline.Length - cp.Distance) / (spline.Length - cp.Distance))) * (metaCGOptions2.FirstU - metaCGOptions.GetDefinedFirstU(0f)) + metaCGOptions.GetDefinedFirstU(0f), vt: 0, uvEdge: startTF == 0f && metaCGOptions.UVEdge, uv1: (startTF != 0f || !metaCGOptions.UVEdge) ? 0f : metaCGOptions.SecondU));
			if (ext[ext.Count - 1].Vertex < data.Count - 1)
			{
				metaCGOptions = findPreviousReferenceCPOptions(spline, endTF, out cp);
				metaCGOptions2 = findNextReferenceCPOptions(spline, endTF, out cp2);
				float num = metaCGOptions2.FirstU;
				float definedSecondU = metaCGOptions.GetDefinedSecondU(0f);
				float num2;
				if (cp2.IsFirstVisibleControlPoint)
				{
					num2 = (data.SourceF[data.Count - 1] * spline.Length - cp.Distance) / (spline.Length - cp.Distance);
					num = (metaCGOptions2.UVEdge ? metaCGOptions2.FirstU : ((ext.Count <= 1) ? 1f : ((float)(Mathf.FloorToInt((!ext[ext.Count - 1].UVEdge) ? ext[ext.Count - 1].FirstU : ext[ext.Count - 1].SecondU) + 1))));
				}
				else
				{
					num2 = (data.SourceF[data.Count - 1] * spline.Length - cp.Distance) / (cp2.Distance - cp.Distance);
				}
				ext.Add(new SamplePointUData(data.Count - 1, false, num2 * (num - definedSecondU) + definedSecondU, 0f));
			}
			float num3 = 0f;
			float num4 = ((!ext[0].UVEdge) ? ext[0].FirstU : ext[0].SecondU);
			float firstU = ext[1].FirstU;
			float num5 = data.F[ext[1].Vertex] - data.F[ext[0].Vertex];
			int num6 = 1;
			for (int i = 0; i < data.Count - 1; i++)
			{
				float num7 = (data.F[i] - num3) / num5;
				data.Map[i] = (firstU - num4) * num7 + num4;
				if (ext[num6].Vertex == i)
				{
					if (ext[num6].FirstU == ext[num6 + 1].FirstU)
					{
						num4 = ((!ext[num6].UVEdge) ? ext[num6].FirstU : ext[num6].SecondU);
						num6++;
					}
					else
					{
						num4 = ext[num6].FirstU;
					}
					firstU = ext[num6 + 1].FirstU;
					num5 = data.F[ext[num6 + 1].Vertex] - data.F[ext[num6].Vertex];
					num3 = data.F[i];
					num6++;
				}
			}
			data.Map[data.Count - 1] = ext[ext.Count - 1].FirstU;
		}

		private MetaCGOptions findPreviousReferenceCPOptions(CurvySpline spline, float tf, out CurvySplineSegment cp)
		{
			cp = spline.TFToSegment(tf);
			MetaCGOptions metadata;
			do
			{
				metadata = cp.GetMetadata<MetaCGOptions>(true);
				if (cp.IsFirstVisibleControlPoint)
				{
					return metadata;
				}
				cp = cp.PreviousSegment;
			}
			while ((bool)cp && !metadata.UVEdge && !metadata.ExplicitU && !metadata.HasDifferentMaterial);
			return metadata;
		}

		private MetaCGOptions findNextReferenceCPOptions(CurvySpline spline, float tf, out CurvySplineSegment cp)
		{
			float localF;
			cp = spline.TFToSegment(tf, out localF);
			MetaCGOptions metadata;
			do
			{
				cp = cp.NextControlPoint;
				metadata = cp.GetMetadata<MetaCGOptions>(true);
				if (!spline.Closed && cp.IsLastVisibleControlPoint)
				{
					return metadata;
				}
			}
			while (!metadata.UVEdge && !metadata.ExplicitU && !metadata.HasDifferentMaterial && !cp.IsFirstSegment);
			return metadata;
		}
	}
}
