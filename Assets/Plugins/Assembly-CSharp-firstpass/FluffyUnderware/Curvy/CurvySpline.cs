using System;
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy
{
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/curvyspline")]
	[AddComponentMenu("Curvy/Curvy Spline", 1)]
	[ExecuteInEditMode]
	public class CurvySpline : CurvySplineBase
	{
		public const string VERSION = "2.1.2";

		public const string VERSIONSHORT = "210";

		public const string WEBROOT = "http://www.fluffyunderware.com/curvy/";

		public const string DOCROOT = "http://www.fluffyunderware.com/curvy/documentation/";

		public const string DOCLINK = "http://www.fluffyunderware.com/curvy/doclink/210/";

		[Section("General", true, false, 100, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvyspline_general")]
		[Tooltip("Interpolation Method")]
		[SerializeField]
		[FormerlySerializedAs("Interpolation")]
		private CurvyInterpolation m_Interpolation = CurvyGlobalManager.DefaultInterpolation;

		[Tooltip("Restrict Control Points to local X/Y axis")]
		[FieldAction("CBCheck2DPlanar", ActionAttribute.ActionEnum.Callback)]
		[SerializeField]
		private bool m_RestrictTo2D;

		[SerializeField]
		[FormerlySerializedAs("Closed")]
		private bool m_Closed;

		[FieldCondition("canHaveManualEndCP", Action = ActionAttribute.ActionEnum.Enable)]
		[Tooltip("Handle End Control Points automatically?")]
		[SerializeField]
		[FormerlySerializedAs("AutoEndTangents")]
		private bool m_AutoEndTangents = true;

		[Tooltip("Orientation Flow")]
		[SerializeField]
		[FormerlySerializedAs("Orientation")]
		private CurvyOrientation m_Orientation = CurvyOrientation.Dynamic;

		[Section("Global Bezier Options", true, false, 100, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvyspline_bezier")]
		[GroupCondition("m_Interpolation", CurvyInterpolation.Bezier, false)]
		[RangeEx(0f, 1f, "Default Distance %", "Handle length by distance to neighbours")]
		[SerializeField]
		private float m_AutoHandleDistance = 0.39f;

		[Section("Global TCB Options", true, false, 100, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvyspline_tcb")]
		[GroupCondition("m_Interpolation", CurvyInterpolation.TCB, false)]
		[GroupAction("TCBOptionsGUI", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("Tension")]
		private float m_Tension;

		[SerializeField]
		[FormerlySerializedAs("Continuity")]
		private float m_Continuity;

		[SerializeField]
		[FormerlySerializedAs("Bias")]
		private float m_Bias;

		[Section("Advanced Settings", true, false, 100, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvyspline_advanced")]
		[FieldAction("ShowGizmoGUI", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Above)]
		[Label("Color", "Gizmo color")]
		[SerializeField]
		private Color m_GizmoColor = CurvyGlobalManager.DefaultGizmoColor;

		[Label("Active Color", "Selected Gizmo color")]
		[SerializeField]
		private Color m_GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;

		[RangeEx(1f, 100f, "", "")]
		[SerializeField]
		[FormerlySerializedAs("Granularity")]
		private int m_CacheDensity = 50;

		[SerializeField]
		[Tooltip("Use a GameObject pool at runtime")]
		private bool m_UsePooling = true;

		[SerializeField]
		[Tooltip("Use threading where applicable")]
		private bool m_UseThreading;

		[Tooltip("Refresh when Control Point position change?")]
		[SerializeField]
		[FormerlySerializedAs("AutoRefresh")]
		private bool m_CheckTransform;

		[SerializeField]
		private CurvyUpdateMethod m_UpdateIn;

		[HideInInspector]
		public List<CurvySplineSegment> ControlPoints = new List<CurvySplineSegment>();

		private List<CurvySplineSegment> mSegments = new List<CurvySplineSegment>();

		private int mCacheSize;

		private int mLastCPCount;

		private Bounds? mBounds;

		private bool mDirtyCurve;

		private bool mDirtyOrientation;

		private List<CurvySplineSegment> mDirtyControlPoints = new List<CurvySplineSegment>();

		private bool mForceRefresh;

		private ThreadPoolWorker mThreadWorker = new ThreadPoolWorker();

		private static CurvySplineMoveEventArgs _moveEventArgs = new CurvySplineMoveEventArgs();

		private CurvySplineSegment _lastDistToSeg;

		public CurvyInterpolation Interpolation
		{
			get
			{
				return m_Interpolation;
			}
			set
			{
				if (m_Interpolation != value)
				{
					m_Interpolation = value;
					SetDirtyAll();
				}
			}
		}

		public bool RestrictTo2D
		{
			get
			{
				return m_RestrictTo2D;
			}
			set
			{
				if (m_RestrictTo2D != value)
				{
					m_RestrictTo2D = value;
				}
			}
		}

		public float AutoHandleDistance
		{
			get
			{
				return m_AutoHandleDistance;
			}
			set
			{
				if (m_AutoHandleDistance != value)
				{
					m_AutoHandleDistance = Mathf.Clamp01(value);
					SetDirtyAll();
				}
			}
		}

		public bool Closed
		{
			get
			{
				return m_Closed;
			}
			set
			{
				if (m_Closed != value)
				{
					m_Closed = value;
					SetDirtyAll();
				}
				if (m_Closed)
				{
					AutoEndTangents = true;
				}
			}
		}

		public bool AutoEndTangents
		{
			get
			{
				return m_AutoEndTangents;
			}
			set
			{
				bool flag = !canHaveManualEndCP() || value;
				if (m_AutoEndTangents != flag)
				{
					m_AutoEndTangents = flag;
					SetDirtyAll();
				}
			}
		}

		public CurvyOrientation Orientation
		{
			get
			{
				return m_Orientation;
			}
			set
			{
				if (m_Orientation != value)
				{
					m_Orientation = value;
					SetDirtyAll();
				}
			}
		}

		public CurvyUpdateMethod UpdateIn
		{
			get
			{
				return m_UpdateIn;
			}
			set
			{
				if (m_UpdateIn != value)
				{
					m_UpdateIn = value;
				}
			}
		}

		public Color GizmoColor
		{
			get
			{
				return m_GizmoColor;
			}
			set
			{
				if (m_GizmoColor != value)
				{
					m_GizmoColor = value;
				}
			}
		}

		public Color GizmoSelectionColor
		{
			get
			{
				return m_GizmoSelectionColor;
			}
			set
			{
				if (m_GizmoSelectionColor != value)
				{
					m_GizmoSelectionColor = value;
				}
			}
		}

		public int CacheDensity
		{
			get
			{
				return m_CacheDensity;
			}
			set
			{
				if (m_CacheDensity != value)
				{
					m_CacheDensity = Mathf.Clamp(value, 1, 100);
					mCacheSize = 0;
					SetDirtyAll();
				}
			}
		}

		public bool UsePooling
		{
			get
			{
				return m_UsePooling;
			}
			set
			{
				if (m_UsePooling != value)
				{
					m_UsePooling = value;
				}
			}
		}

		public bool UseThreading
		{
			get
			{
				return m_UseThreading;
			}
			set
			{
				if (m_UseThreading != value)
				{
					m_UseThreading = value;
				}
			}
		}

		public bool CheckTransform
		{
			get
			{
				return m_CheckTransform;
			}
			set
			{
				if (m_CheckTransform != value)
				{
					m_CheckTransform = value;
				}
			}
		}

		public float Tension
		{
			get
			{
				return m_Tension;
			}
			set
			{
				if (m_Tension != value)
				{
					m_Tension = value;
					SetDirtyAll();
				}
			}
		}

		public float Continuity
		{
			get
			{
				return m_Continuity;
			}
			set
			{
				if (m_Continuity != value)
				{
					m_Continuity = value;
					SetDirtyAll();
				}
			}
		}

		public float Bias
		{
			get
			{
				return m_Bias;
			}
			set
			{
				if (m_Bias != value)
				{
					m_Bias = value;
					SetDirtyAll();
				}
			}
		}

		public override Bounds Bounds
		{
			get
			{
				if (!mBounds.HasValue)
				{
					mBounds = getBounds();
				}
				return mBounds.Value;
			}
		}

		public override int Count
		{
			get
			{
				return mSegments.Count;
			}
		}

		public int ControlPointCount
		{
			get
			{
				return ControlPoints.Count;
			}
		}

		public int CacheSize
		{
			get
			{
				if (mCacheSize == 0)
				{
					for (int i = 0; i < mSegments.Count; i++)
					{
						mCacheSize += mSegments[i].CacheSize;
					}
				}
				return mCacheSize;
			}
		}

		public override bool Dirty
		{
			get
			{
				return mDirtyControlPoints.Count > 0 || mForceRefresh;
			}
		}

		public CurvySplineSegment this[int idx]
		{
			get
			{
				return (idx <= -1 || idx >= mSegments.Count) ? null : mSegments[idx];
			}
		}

		public List<CurvySplineSegment> Segments
		{
			get
			{
				return mSegments;
			}
		}

		public CurvySplineSegment FirstVisibleControlPoint
		{
			get
			{
				if (!base.IsInitialized && ControlPointCount > 0)
				{
					return (!AutoEndTangents) ? ControlPoints[Mathf.Min(ControlPointCount - 1, 1)] : ControlPoints[0];
				}
				return (Count <= 0 || ControlPoints.Count <= 0) ? null : this[0];
			}
		}

		public CurvySplineSegment LastVisibleControlPoint
		{
			get
			{
				if (!base.IsInitialized && ControlPointCount > 0)
				{
					if (Closed)
					{
						return ControlPoints[0];
					}
					return (!AutoEndTangents) ? ControlPoints[Mathf.Max(0, ControlPointCount - 2)] : ControlPoints[ControlPointCount - 1];
				}
				return (Count > 0 && ControlPoints.Count > this[Count - 1].ControlPointIndex + 1) ? ControlPoints[this[Count - 1].ControlPointIndex + 1] : ((!Closed) ? null : ControlPoints[0]);
			}
		}

		public override bool IsContinuous
		{
			get
			{
				return true;
			}
		}

		public override bool IsClosed
		{
			get
			{
				return Closed;
			}
		}

		public CurvySpline NextSpline
		{
			get
			{
				CurvySplineSegment lastVisibleControlPoint = LastVisibleControlPoint;
				return (!lastVisibleControlPoint || !lastVisibleControlPoint.FollowUp) ? null : lastVisibleControlPoint.FollowUp.Spline;
			}
		}

		public CurvySpline PreviousSpline
		{
			get
			{
				CurvySplineSegment firstVisibleControlPoint = FirstVisibleControlPoint;
				return (!firstVisibleControlPoint || !firstVisibleControlPoint.FollowUp) ? null : firstVisibleControlPoint.FollowUp.Spline;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (UsePooling && !DTSingleton<CurvyGlobalManager>.Instance)
			{
				Debug.Log("[Curvy] Unable to create _CurvyGlobalManager!");
			}
		}

		protected override void OnEnable()
		{
			if (ControlPoints.Count == 0)
			{
				SyncSplineFromHierarchy();
			}
			mSegments.Clear();
			if (!Application.isPlaying)
			{
				SetDirtyAll();
			}
		}

		private void OnDisable()
		{
			mIsInitialized = false;
		}

		private void OnDestroy()
		{
			if (1 == 0)
			{
				return;
			}
			for (int i = 0; i < ControlPointCount; i++)
			{
				if (UsePooling && Application.isPlaying)
				{
					ControlPoints[i].StripComponents();
					if (DTSingleton<CurvyGlobalManager>.Instance != null)
					{
						DTSingleton<CurvyGlobalManager>.Instance.ControlPointPool.Push(ControlPoints[i]);
					}
				}
				else
				{
					ControlPoints[i].Disconnect();
				}
			}
		}

		private void Reset()
		{
			Interpolation = CurvyGlobalManager.DefaultInterpolation;
			RestrictTo2D = false;
			AutoHandleDistance = 0.39f;
			Closed = false;
			AutoEndTangents = true;
			Orientation = CurvyOrientation.Dynamic;
			GizmoColor = CurvyGlobalManager.DefaultGizmoColor;
			GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;
			CacheDensity = 50;
			CheckTransform = true;
			Tension = 0f;
			Continuity = 0f;
			Bias = 0f;
			SyncSplineFromHierarchy();
		}

		private void Update()
		{
			if (UpdateIn == CurvyUpdateMethod.Update || !Application.isPlaying)
			{
				doUpdate();
			}
		}

		private void LateUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.LateUpdate)
			{
				doUpdate();
			}
		}

		private void FixedUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
			{
				doUpdate();
			}
		}

		public static CurvySpline Create()
		{
			CurvySpline component = new GameObject("Curvy Spline", typeof(CurvySpline)).GetComponent<CurvySpline>();
			component.gameObject.layer = CurvyGlobalManager.SplineLayer;
			return component;
		}

		public static CurvySpline Create(CurvySpline takeOptionsFrom)
		{
			CurvySpline curvySpline = Create();
			if ((bool)takeOptionsFrom)
			{
				curvySpline.RestrictTo2D = takeOptionsFrom.RestrictTo2D;
				curvySpline.GizmoColor = takeOptionsFrom.GizmoColor;
				curvySpline.GizmoSelectionColor = takeOptionsFrom.GizmoSelectionColor;
				curvySpline.Interpolation = takeOptionsFrom.Interpolation;
				curvySpline.Closed = takeOptionsFrom.Closed;
				curvySpline.AutoEndTangents = takeOptionsFrom.AutoEndTangents;
				curvySpline.CacheDensity = takeOptionsFrom.CacheDensity;
				curvySpline.Orientation = takeOptionsFrom.Orientation;
				curvySpline.CheckTransform = takeOptionsFrom.CheckTransform;
			}
			return curvySpline;
		}

		public static int CalculateCacheSize(int density, float length, float maxPointsPerUnit)
		{
			if (maxPointsPerUnit == 0f)
			{
				maxPointsPerUnit = CurvyGlobalManager.MaxCachePPU;
			}
			float num = DTTween.QuadIn(density - 1, 0f, maxPointsPerUnit, 99f);
			return Mathf.FloorToInt(length * num) + 1;
		}

		public static Vector3 Bezier(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
		{
			double num = 3.0;
			double num2 = -3.0;
			double num3 = 3.0;
			double num4 = -6.0;
			double num5 = 3.0;
			double num6 = -3.0;
			double num7 = 3.0;
			double num8 = (double)(0f - P0.x) + num * (double)T0.x + num2 * (double)T1.x + (double)P1.x;
			double num9 = num3 * (double)P0.x + num4 * (double)T0.x + num5 * (double)T1.x;
			double num10 = num6 * (double)P0.x + num7 * (double)T0.x;
			double num11 = P0.x;
			double num12 = (double)(0f - P0.y) + num * (double)T0.y + num2 * (double)T1.y + (double)P1.y;
			double num13 = num3 * (double)P0.y + num4 * (double)T0.y + num5 * (double)T1.y;
			double num14 = num6 * (double)P0.y + num7 * (double)T0.y;
			double num15 = P0.y;
			double num16 = (double)(0f - P0.z) + num * (double)T0.z + num2 * (double)T1.z + (double)P1.z;
			double num17 = num3 * (double)P0.z + num4 * (double)T0.z + num5 * (double)T1.z;
			double num18 = num6 * (double)P0.z + num7 * (double)T0.z;
			double num19 = P0.z;
			float x = (float)(((num8 * (double)f + num9) * (double)f + num10) * (double)f + num11);
			float y = (float)(((num12 * (double)f + num13) * (double)f + num14) * (double)f + num15);
			float z = (float)(((num16 * (double)f + num17) * (double)f + num18) * (double)f + num19);
			return new Vector3(x, y, z);
		}

		public static float BezierTangent(float T0, float P0, float P1, float T1, float t)
		{
			float num = P1 - 3f * T1 + 3f * T0 - P0;
			float num2 = 3f * T1 - 6f * T0 + 3f * P0;
			float num3 = 3f * T0 - 3f * P0;
			return 3f * num * t * t + 2f * num2 * t + num3;
		}

		public static Vector3 BezierTangent(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
		{
			Vector3 vector = P1 - 3f * T1 + 3f * T0 - P0;
			Vector3 vector2 = 3f * T1 - 6f * T0 + 3f * P0;
			Vector3 vector3 = 3f * T0 - 3f * P0;
			return 3f * vector * f * f + 2f * vector2 * f + vector3;
		}

		public static Vector3 CatmullRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
		{
			double num = -0.5;
			double num2 = 1.5;
			double num3 = -1.5;
			double num4 = 0.5;
			double num5 = -2.5;
			double num6 = 2.0;
			double num7 = -0.5;
			double num8 = -0.5;
			double num9 = 0.5;
			double num10 = num * (double)T0.x + num2 * (double)P0.x + num3 * (double)P1.x + num4 * (double)T1.x;
			double num11 = (double)T0.x + num5 * (double)P0.x + num6 * (double)P1.x + num7 * (double)T1.x;
			double num12 = num8 * (double)T0.x + num9 * (double)P1.x;
			double num13 = P0.x;
			double num14 = num * (double)T0.y + num2 * (double)P0.y + num3 * (double)P1.y + num4 * (double)T1.y;
			double num15 = (double)T0.y + num5 * (double)P0.y + num6 * (double)P1.y + num7 * (double)T1.y;
			double num16 = num8 * (double)T0.y + num9 * (double)P1.y;
			double num17 = P0.y;
			double num18 = num * (double)T0.z + num2 * (double)P0.z + num3 * (double)P1.z + num4 * (double)T1.z;
			double num19 = (double)T0.z + num5 * (double)P0.z + num6 * (double)P1.z + num7 * (double)T1.z;
			double num20 = num8 * (double)T0.z + num9 * (double)P1.z;
			double num21 = P0.z;
			float x = (float)(((num10 * (double)f + num11) * (double)f + num12) * (double)f + num13);
			float y = (float)(((num14 * (double)f + num15) * (double)f + num16) * (double)f + num17);
			float z = (float)(((num18 * (double)f + num19) * (double)f + num20) * (double)f + num21);
			return new Vector3(x, y, z);
		}

		public static Vector3 TCB(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f, float FT0, float FC0, float FB0, float FT1, float FC1, float FB1)
		{
			double num = (1f - FT0) * (1f + FC0) * (1f + FB0);
			double num2 = (1f - FT0) * (1f - FC0) * (1f - FB0);
			double num3 = (1f - FT1) * (1f - FC1) * (1f + FB1);
			double num4 = (1f - FT1) * (1f + FC1) * (1f - FB1);
			double num5 = 2.0;
			double num6 = (0.0 - num) / num5;
			double num7 = (4.0 + num - num2 - num3) / num5;
			double num8 = (-4.0 + num2 + num3 - num4) / num5;
			double num9 = num4 / num5;
			double num10 = 2.0 * num / num5;
			double num11 = (-6.0 - 2.0 * num + 2.0 * num2 + num3) / num5;
			double num12 = (6.0 - 2.0 * num2 - num3 + num4) / num5;
			double num13 = (0.0 - num4) / num5;
			double num14 = (0.0 - num) / num5;
			double num15 = (num - num2) / num5;
			double num16 = num2 / num5;
			double num17 = 2.0 / num5;
			double num18 = num6 * (double)T0.x + num7 * (double)P0.x + num8 * (double)P1.x + num9 * (double)T1.x;
			double num19 = num10 * (double)T0.x + num11 * (double)P0.x + num12 * (double)P1.x + num13 * (double)T1.x;
			double num20 = num14 * (double)T0.x + num15 * (double)P0.x + num16 * (double)P1.x;
			double num21 = num17 * (double)P0.x;
			double num22 = num6 * (double)T0.y + num7 * (double)P0.y + num8 * (double)P1.y + num9 * (double)T1.y;
			double num23 = num10 * (double)T0.y + num11 * (double)P0.y + num12 * (double)P1.y + num13 * (double)T1.y;
			double num24 = num14 * (double)T0.y + num15 * (double)P0.y + num16 * (double)P1.y;
			double num25 = num17 * (double)P0.y;
			double num26 = num6 * (double)T0.z + num7 * (double)P0.z + num8 * (double)P1.z + num9 * (double)T1.z;
			double num27 = num10 * (double)T0.z + num11 * (double)P0.z + num12 * (double)P1.z + num13 * (double)T1.z;
			double num28 = num14 * (double)T0.z + num15 * (double)P0.z + num16 * (double)P1.z;
			double num29 = num17 * (double)P0.z;
			float x = (float)(((num18 * (double)f + num19) * (double)f + num20) * (double)f + num21);
			float y = (float)(((num22 * (double)f + num23) * (double)f + num24) * (double)f + num25);
			float z = (float)(((num26 * (double)f + num27) * (double)f + num28) * (double)f + num29);
			return new Vector3(x, y, z);
		}

		public override Vector3 Interpolate(float tf)
		{
			return Interpolate(tf, Interpolation);
		}

		public override Vector3 Interpolate(float tf, CurvyInterpolation interpolation)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.Interpolate(localF, interpolation);
		}

		public override Vector3 InterpolateFast(float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.InterpolateFast(localF);
		}

		public override Component GetMetadata(Type type, float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? null : curvySplineSegment.GetMetaData(type);
		}

		public override U InterpolateMetadata<T, U>(float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? default(U) : curvySplineSegment.InterpolateMetadata<T, U>(localF);
		}

		public override object InterpolateMetadata(Type type, float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? null : curvySplineSegment.InterpolateMetadata(type, localF);
		}

		public override Vector3 InterpolateScale(float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.InterpolateScale(localF);
		}

		public override Vector3 GetOrientationUpFast(float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.GetOrientationUpFast(localF);
		}

		public override Quaternion GetOrientationFast(float tf, bool inverse)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Quaternion.identity : curvySplineSegment.GetOrientationFast(localF, inverse);
		}

		public override Vector3 GetTangent(float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.GetTangent(localF);
		}

		public override Vector3 GetTangent(float tf, Vector3 position)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if (curvySplineSegment.GetTangent(localF, ref position).x < 0f)
			{
				Vector3 tangent = curvySplineSegment.GetTangent(localF, ref position);
			}
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.GetTangent(localF, ref position);
		}

		public override Vector3 GetTangentFast(float tf)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			return (!curvySplineSegment) ? Vector3.zero : curvySplineSegment.GetTangentFast(localF);
		}

		public override float TFToDistance(float tf, CurvyClamping clamping)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF, clamping);
			return (!curvySplineSegment) ? 0f : (curvySplineSegment.Distance + curvySplineSegment.LocalFToDistance(localF));
		}

		public override CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
		{
			tf = CurvyUtility.ClampTF(tf, clamping);
			localF = 0f;
			if (Count == 0)
			{
				return null;
			}
			float num = tf * (float)Count;
			int num2 = (int)num;
			localF = num - (float)num2;
			if (num2 == Count)
			{
				num2--;
				localF = 1f;
			}
			return this[num2];
		}

		public int TFToSegmentIndex(float tf, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			if (Count == 0)
			{
				return -1;
			}
			int num = (int)(CurvyUtility.ClampTF(tf, clamping) * (float)Count);
			return (num != Count) ? num : (num - 1);
		}

		public override float SegmentToTF(CurvySplineSegment segment)
		{
			return SegmentToTF(segment, 0f);
		}

		public override float SegmentToTF(CurvySplineSegment segment, float localF)
		{
			if (!segment)
			{
				return 0f;
			}
			if (segment.SegmentIndex == -1)
			{
				return (segment.ControlPointIndex > 0) ? 1 : 0;
			}
			return (float)segment.SegmentIndex / (float)Count + 1f / (float)Count * localF;
		}

		public override float DistanceToTF(float distance, CurvyClamping clamping)
		{
			if (distance == base.Length)
			{
				return 1f;
			}
			float localDistance;
			CurvySplineSegment curvySplineSegment = DistanceToSegment(distance, out localDistance, clamping);
			return (!curvySplineSegment) ? 0f : SegmentToTF(curvySplineSegment, curvySplineSegment.DistanceToLocalF(localDistance));
		}

		public CurvySplineSegment DistanceToSegment(float distance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			float localDistance;
			return DistanceToSegment(distance, out localDistance, clamping);
		}

		public CurvySplineSegment DistanceToSegment(float distance, out float localDistance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			distance = CurvyUtility.ClampDistance(distance, clamping, base.Length);
			CurvySplineSegment curvySplineSegment = null;
			localDistance = -1f;
			if (Count > 0)
			{
				distance = Mathf.Clamp(distance, 0f, base.Length);
				if (distance == base.Length)
				{
					curvySplineSegment = this[Count - 1];
					localDistance = curvySplineSegment.Distance + curvySplineSegment.Length;
					return curvySplineSegment;
				}
				localDistance = 0f;
				curvySplineSegment = ((!(_lastDistToSeg != null) || !(_lastDistToSeg.Distance < distance)) ? mSegments[0] : _lastDistToSeg);
				int count = Count;
				while ((bool)curvySplineSegment && curvySplineSegment.Distance + curvySplineSegment.Length < distance && count-- > 0)
				{
					curvySplineSegment = curvySplineSegment.NextSegment;
				}
				if (count <= 0)
				{
					Debug.LogError("[Curvy] CurvySpline.DistanceToSegment() caused a deadloop! This shouldn't happen at all. Please raise a bug report!");
				}
				if (curvySplineSegment == null)
				{
					curvySplineSegment = this[Count - 1];
				}
				localDistance = distance - curvySplineSegment.Distance;
				_lastDistToSeg = curvySplineSegment;
			}
			return curvySplineSegment;
		}

		public override Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
		{
			if (!base.OnMoveControlPointReached.HasListeners() && !base.OnMoveEndReached.HasListeners())
			{
				return base.Move(ref tf, ref direction, fDistance, clamping);
			}
			return eventAwareMove(ref tf, ref direction, fDistance, clamping, false);
		}

		public override Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
		{
			if (!base.OnMoveControlPointReached.HasListeners() && !base.OnMoveEndReached.HasListeners())
			{
				return base.MoveFast(ref tf, ref direction, fDistance, clamping);
			}
			return eventAwareMove(ref tf, ref direction, fDistance, clamping, true);
		}

		public override Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
		{
			if (!base.OnMoveControlPointReached.HasListeners() && !base.OnMoveEndReached.HasListeners())
			{
				return base.MoveByLengthFast(ref tf, ref direction, distance, clamping);
			}
			return eventAwareMoveDistance(ref tf, ref direction, distance, clamping, false);
		}

		public CurvySplineSegment Add()
		{
			return InsertAfter(null);
		}

		public CurvySplineSegment[] Add(params Vector3[] controlPoints)
		{
			if (!OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this)).Cancel)
			{
				CurvySplineSegment[] array = new CurvySplineSegment[controlPoints.Length];
				for (int i = 0; i < controlPoints.Length; i++)
				{
					array[i] = Add();
					array[i].transform.localPosition = controlPoints[i];
					array[i].TTransform.localPosition = controlPoints[i];
					array[i].AutoHandleDistance = AutoHandleDistance;
				}
				for (int j = 0; j < controlPoints.Length; j++)
				{
					OnAfterControlPointAddEvent(new CurvyControlPointEventArgs(this, this, array[j], CurvyControlPointEventArgs.ModeEnum.AddAfter));
				}
				OnAfterControlPointChangesEvent(new CurvySplineEventArgs(this, this));
				return array;
			}
			return new CurvySplineSegment[0];
		}

		public CurvySplineSegment InsertBefore(CurvySplineSegment controlPoint)
		{
			if (!OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this, this, controlPoint, CurvyControlPointEventArgs.ModeEnum.AddBefore)).Cancel)
			{
				CurvySplineSegment curvySplineSegment;
				GameObject gameObject;
				if (UsePooling && Application.isPlaying)
				{
					curvySplineSegment = DTSingleton<CurvyGlobalManager>.Instance.ControlPointPool.Pop<CurvySplineSegment>(base.transform);
					gameObject = curvySplineSegment.gameObject;
				}
				else
				{
					gameObject = new GameObject("NewCP", typeof(CurvySplineSegment));
					curvySplineSegment = gameObject.GetComponent<CurvySplineSegment>();
				}
				gameObject.layer = base.gameObject.layer;
				gameObject.transform.parent = base.transform;
				int index = 0;
				if ((bool)controlPoint)
				{
					if ((bool)controlPoint.PreviousSegment)
					{
						curvySplineSegment.transform.localPosition = controlPoint.PreviousSegment.Interpolate(0.5f);
					}
					else if ((bool)controlPoint.PreviousTransform)
					{
						gameObject.transform.position = Vector3.Lerp(controlPoint.PreviousTransform.position, controlPoint.transform.position, 0.5f);
					}
					index = Mathf.Max(0, controlPoint.ControlPointIndex);
				}
				ControlPoints.Insert(index, curvySplineSegment);
				SyncHierarchyFromSpline();
				curvySplineSegment.AutoHandleDistance = AutoHandleDistance;
				if ((bool)controlPoint)
				{
					controlPoint.SetDirty();
				}
				curvySplineSegment.SetDirty();
				mLastCPCount++;
				CurvyControlPointEventArgs e = new CurvyControlPointEventArgs(this, this, curvySplineSegment);
				OnAfterControlPointAddEvent(e);
				OnAfterControlPointChangesEvent(e);
				return curvySplineSegment;
			}
			return null;
		}

		public CurvySplineSegment InsertAfter(CurvySplineSegment controlPoint)
		{
			if (!OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this, this, controlPoint, CurvyControlPointEventArgs.ModeEnum.AddAfter)).Cancel)
			{
				CurvySplineSegment curvySplineSegment;
				GameObject gameObject;
				if (UsePooling && Application.isPlaying)
				{
					curvySplineSegment = DTSingleton<CurvyGlobalManager>.Instance.ControlPointPool.Pop<CurvySplineSegment>(base.transform);
					gameObject = curvySplineSegment.gameObject;
				}
				else
				{
					gameObject = new GameObject("NewCP", typeof(CurvySplineSegment));
					curvySplineSegment = gameObject.GetComponent<CurvySplineSegment>();
				}
				gameObject.layer = base.gameObject.layer;
				gameObject.transform.SetParent(base.transform);
				int index = ControlPoints.Count;
				if ((bool)controlPoint)
				{
					if (controlPoint.IsValidSegment)
					{
						curvySplineSegment.transform.localPosition = controlPoint.Interpolate(0.5f);
					}
					else if ((bool)controlPoint.NextTransform)
					{
						gameObject.transform.position = Vector3.Lerp(controlPoint.NextTransform.position, controlPoint.transform.position, 0.5f);
					}
					index = controlPoint.ControlPointIndex + 1;
				}
				ControlPoints.Insert(index, curvySplineSegment);
				SyncHierarchyFromSpline();
				curvySplineSegment.AutoHandleDistance = AutoHandleDistance;
				if ((bool)controlPoint)
				{
					controlPoint.SetDirty();
				}
				curvySplineSegment.SetDirty();
				SetDirty(curvySplineSegment.PreviousControlPoint);
				mLastCPCount++;
				CurvyControlPointEventArgs e = new CurvyControlPointEventArgs(this, this, curvySplineSegment, CurvyControlPointEventArgs.ModeEnum.AddAfter);
				OnAfterControlPointAddEvent(e);
				OnAfterControlPointChangesEvent(e);
				return curvySplineSegment;
			}
			return null;
		}

		public override void Clear()
		{
			foreach (CurvySplineSegment controlPoint in ControlPoints)
			{
				if (OnBeforeControlPointDeleteEvent(new CurvyControlPointEventArgs(this, this, controlPoint, CurvyControlPointEventArgs.ModeEnum.Delete)).Cancel)
				{
					return;
				}
			}
			for (int num = ControlPointCount - 1; num >= 0; num--)
			{
				if (UsePooling && Application.isPlaying)
				{
					DTSingleton<CurvyGlobalManager>.Instance.ControlPointPool.Push(ControlPoints[num]);
				}
				else
				{
					UnityEngine.Object.Destroy(ControlPoints[num].gameObject);
				}
			}
			ControlPoints.Clear();
			mDirtyControlPoints.Clear();
			mSegments.Clear();
			mCacheSize = 0;
			mLength = 0f;
			SetDirtyAll();
			Refresh();
			OnAfterControlPointChangesEvent(new CurvySplineEventArgs(this));
		}

		public void Delete(CurvySplineSegment controlPoint)
		{
			if ((bool)controlPoint && !OnBeforeControlPointDeleteEvent(new CurvyControlPointEventArgs(this, this, controlPoint, CurvyControlPointEventArgs.ModeEnum.Delete)).Cancel)
			{
				CurvySplineSegment previousControlPoint = controlPoint.PreviousControlPoint;
				CurvySplineSegment nextControlPoint = controlPoint.NextControlPoint;
				if ((bool)previousControlPoint)
				{
					previousControlPoint.SetDirty();
				}
				if ((bool)nextControlPoint)
				{
					nextControlPoint.SetDirty();
				}
				ControlPoints.Remove(controlPoint);
				mDirtyControlPoints.Remove(controlPoint);
				controlPoint.transform.SetAsLastSibling();
				if (UsePooling && Application.isPlaying)
				{
					controlPoint.StripComponents();
					DTSingleton<CurvyGlobalManager>.Instance.ControlPointPool.Push(controlPoint);
					mDirtyControlPoints.Remove(controlPoint);
				}
				else
				{
					UnityEngine.Object.Destroy(controlPoint.gameObject);
				}
				SyncHierarchyFromSpline();
				mLastCPCount--;
				OnAfterControlPointChangesEvent(new CurvyControlPointEventArgs(this, this, null));
			}
		}

		public override Vector3[] GetApproximation(Space space = Space.Self)
		{
			Vector3[] array = new Vector3[CacheSize + 1];
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				this[i].Approximation.CopyTo(array, num);
				num += Mathf.Max(0, this[i].Approximation.Length - 1);
			}
			if (space == Space.World)
			{
				Matrix4x4 localToWorldMatrix = base.TTransform.localToWorldMatrix;
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = localToWorldMatrix.MultiplyPoint3x4(array[j]);
				}
			}
			return array;
		}

		public override Vector3[] GetApproximationT()
		{
			Vector3[] array = new Vector3[CacheSize + 1];
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				this[i].ApproximationT.CopyTo(array, num);
				num += Mathf.Max(0, this[i].ApproximationT.Length - 1);
			}
			return array;
		}

		public override Vector3[] GetApproximationUpVectors()
		{
			Vector3[] array = new Vector3[CacheSize + 1];
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				this[i].ApproximationUp.CopyTo(array, num);
				num += Mathf.Max(0, this[i].ApproximationUp.Length - 1);
			}
			return array;
		}

		public override float GetNearestPointTF(Vector3 p)
		{
			Vector3 nearest;
			return GetNearestPointTF(p, out nearest);
		}

		public override float GetNearestPointTF(Vector3 p, out Vector3 nearest)
		{
			return GetNearestPointTF(p, out nearest);
		}

		public float GetNearestPointTF(Vector3 p, int startSegmentIndex = 0, int stopSegmentIndex = -1)
		{
			Vector3 nearest;
			return GetNearestPointTF(p, out nearest, startSegmentIndex, stopSegmentIndex);
		}

		public float GetNearestPointTF(Vector3 p, out Vector3 nearest, int startSegmentIndex = 0, int stopSegmentIndex = -1)
		{
			nearest = Vector3.zero;
			if (Count == 0)
			{
				return -1f;
			}
			float num = float.MaxValue;
			float localF = 0f;
			CurvySplineSegment curvySplineSegment = null;
			if (stopSegmentIndex == -1)
			{
				stopSegmentIndex = Count - 1;
			}
			startSegmentIndex = Mathf.Clamp(startSegmentIndex, 0, Count - 1);
			stopSegmentIndex = Mathf.Clamp(stopSegmentIndex + 1, startSegmentIndex + 1, Count);
			for (int i = startSegmentIndex; i < stopSegmentIndex; i++)
			{
				float nearestPointF = this[i].GetNearestPointF(p);
				Vector3 vector = this[i].Interpolate(nearestPointF);
				float sqrMagnitude = (vector - p).sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					curvySplineSegment = this[i];
					localF = nearestPointF;
					nearest = vector;
					num = sqrMagnitude;
				}
			}
			return curvySplineSegment.LocalFToTF(localF);
		}

		public override void Refresh()
		{
			_lastDistToSeg = null;
			if (mForceRefresh)
			{
				mDirtyControlPoints.Clear();
				mDirtyControlPoints.AddRange(ControlPoints);
			}
			if (mDirtyControlPoints.Count > 0)
			{
				mDirtyControlPoints.Sort();
				if (mDirtyCurve)
				{
					mCacheSize = 0;
					mLength = 0f;
					mSegments.Clear();
					for (int i = mDirtyControlPoints[0].ControlPointIndex; i < ControlPointCount; i++)
					{
						ControlPoints[i].TTransform.FromTransform(ControlPoints[i].transform);
						ControlPoints[i].ClearSegmentIndexINTERNAL();
					}
					if (Interpolation == CurvyInterpolation.Bezier)
					{
						for (int j = 0; j < mDirtyControlPoints.Count; j++)
						{
							if (mDirtyControlPoints[j].AutoHandles)
							{
								mDirtyControlPoints[j].SetBezierHandles();
							}
						}
					}
					mBounds = null;
					if (UseThreading)
					{
						for (int k = 0; k < mDirtyControlPoints.Count; k++)
						{
							CurvySplineSegment @object = mDirtyControlPoints[k];
							mThreadWorker.QueueWorkItem(@object.refreshCurveINTERNAL);
						}
						mThreadWorker.WaitAll();
					}
					else
					{
						for (int l = 0; l < mDirtyControlPoints.Count; l++)
						{
							mDirtyControlPoints[l].refreshCurveINTERNAL();
						}
					}
					for (int m = 0; m < mDirtyControlPoints.Count; m++)
					{
						CurvySplineSegment curvySplineSegment = mDirtyControlPoints[m];
						CurvySplineSegment previousControlPoint = curvySplineSegment.PreviousControlPoint;
						if ((bool)previousControlPoint)
						{
							previousControlPoint.ApproximationT[previousControlPoint.CacheSize] = curvySplineSegment.ApproximationT[0];
						}
					}
					if (ControlPointCount > 0)
					{
						if (ControlPoints[0].IsValidSegment)
						{
							mSegments.Add(ControlPoints[0]);
						}
						mCacheSize += ControlPoints[0].CacheSize;
						ControlPoints[0].Distance = 0f;
						for (int n = 1; n < ControlPoints.Count; n++)
						{
							ControlPoints[n].Distance = ControlPoints[n - 1].Distance + ControlPoints[n - 1].Length;
							if (ControlPoints[n].IsValidSegment)
							{
								mSegments.Add(ControlPoints[n]);
							}
							mCacheSize += ControlPoints[n].CacheSize;
						}
						for (int num = 1; num < ControlPoints.Count; num++)
						{
							if (!ControlPoints[num].CanHaveFollowUp)
							{
								ControlPoints[num].SetFollowUp(null);
							}
						}
						if (Count > 0)
						{
							mLength = ((!Closed) ? LastVisibleControlPoint.Distance : (this[Count - 1].Distance + this[Count - 1].Length));
							CurvySplineSegment curvySplineSegment2 = this[Count - 1];
							if (Closed)
							{
								curvySplineSegment2.ApproximationT[curvySplineSegment2.CacheSize] = this[0].ApproximationT[0];
							}
							else
							{
								curvySplineSegment2.ApproximationT[curvySplineSegment2.CacheSize] = curvySplineSegment2.ApproximationT[curvySplineSegment2.CacheSize - 1];
								CurvySplineSegment lastVisibleControlPoint = LastVisibleControlPoint;
								if ((bool)lastVisibleControlPoint)
								{
									lastVisibleControlPoint.Approximation[0] = curvySplineSegment2.Approximation[curvySplineSegment2.CacheSize];
									lastVisibleControlPoint.ApproximationT[0] = curvySplineSegment2.ApproximationT[curvySplineSegment2.CacheSize];
								}
							}
						}
					}
				}
				if (mDirtyOrientation && Count > 0)
				{
					if (Orientation == CurvyOrientation.Static)
					{
						if (UseThreading)
						{
							for (int num2 = 0; num2 < mDirtyControlPoints.Count; num2++)
							{
								CurvySplineSegment object2 = mDirtyControlPoints[num2];
								mThreadWorker.QueueWorkItem(object2.refreshOrientationStaticINTERNAL);
							}
							mThreadWorker.WaitAll();
						}
						else
						{
							for (int num3 = 0; num3 < mDirtyControlPoints.Count; num3++)
							{
								mDirtyControlPoints[num3].refreshOrientationStaticINTERNAL();
							}
						}
					}
					else if (Orientation == CurvyOrientation.Dynamic)
					{
						this[0].OrientationAnchor = true;
						if (!AutoEndTangents && ControlPointCount > 1)
						{
							mDirtyControlPoints.Remove(ControlPoints[0]);
							mDirtyControlPoints.Remove(ControlPoints[ControlPointCount - 1]);
						}
						int num4 = ControlPointCount + 1;
						do
						{
							CurvySplineSegment currentAnchorGroup = getCurrentAnchorGroup(mDirtyControlPoints[0]);
							Vector3 lastUpVector = currentAnchorGroup.getOrthoUp0INTERNAL();
							CurvySplineSegment curvySplineSegment3 = currentAnchorGroup;
							int num5 = 0;
							int num6 = 0;
							float num7 = 0f;
							bool isValidSegment;
							do
							{
								num5 += curvySplineSegment3.CacheSize;
								num6++;
								num7 += curvySplineSegment3.Length;
								curvySplineSegment3.refreshOrientationPTFINTERNAL(ref lastUpVector);
								mDirtyControlPoints.Remove(curvySplineSegment3);
								curvySplineSegment3 = curvySplineSegment3.NextControlPoint;
								isValidSegment = curvySplineSegment3.IsValidSegment;
							}
							while ((bool)curvySplineSegment3 && !curvySplineSegment3.OrientationAnchor && isValidSegment);
							if (!isValidSegment)
							{
								mDirtyControlPoints.Remove(curvySplineSegment3);
							}
							if (!curvySplineSegment3 || (!isValidSegment && !curvySplineSegment3.OrientationAnchor && !curvySplineSegment3.IsLastVisibleControlPoint))
							{
								continue;
							}
							float num8 = lastUpVector.AngleSigned(curvySplineSegment3.getOrthoUp0INTERNAL(), curvySplineSegment3.ApproximationT[0]) / (float)num5;
							float angleaccu = num8;
							curvySplineSegment3 = currentAnchorGroup;
							lastUpVector = curvySplineSegment3.ApproximationUp[0];
							do
							{
								float num9 = num8;
								switch (currentAnchorGroup.Swirl)
								{
								case CurvyOrientationSwirl.Segment:
									num9 += currentAnchorGroup.SwirlTurns * 360f / (float)curvySplineSegment3.CacheSize;
									break;
								case CurvyOrientationSwirl.AnchorGroup:
									num9 += currentAnchorGroup.SwirlTurns * 360f / (float)num6 / (float)curvySplineSegment3.CacheSize;
									break;
								case CurvyOrientationSwirl.AnchorGroupAbs:
									num9 += currentAnchorGroup.SwirlTurns * 360f * (curvySplineSegment3.Length / num7) / (float)curvySplineSegment3.CacheSize;
									break;
								}
								curvySplineSegment3.smoothOrientationINTERNAL(ref lastUpVector, ref angleaccu, num9);
								curvySplineSegment3 = curvySplineSegment3.NextSegment;
							}
							while ((bool)curvySplineSegment3 && !curvySplineSegment3.OrientationAnchor);
						}
						while (mDirtyControlPoints.Count > 0 && num4-- > 0);
						if (num4 <= 0)
						{
							Debug.Log("[Curvy] Deadloop in CurvySpline.Refresh! Please raise a bugreport!");
						}
					}
					if (!Closed && Orientation != 0)
					{
						CurvySplineSegment lastVisibleControlPoint2 = LastVisibleControlPoint;
						CurvySplineSegment previousControlPoint2 = lastVisibleControlPoint2.PreviousControlPoint;
						lastVisibleControlPoint2.ApproximationUp[0] = previousControlPoint2.ApproximationUp[previousControlPoint2.CacheSize];
						if (lastVisibleControlPoint2.AutoBakeOrientation)
						{
							lastVisibleControlPoint2.BakeOrientation(false);
						}
					}
				}
			}
			mDirtyControlPoints.Clear();
			mDirtyCurve = false;
			mDirtyOrientation = false;
			mForceRefresh = false;
			mIsInitialized = true;
			OnRefreshEvent(new CurvySplineEventArgs(this, this));
		}

		public override void SetDirtyAll()
		{
			SetDirtyAll();
		}

		public void SetDirtyAll(bool dirtyCurve = true, bool dirtyOrientation = true)
		{
			mForceRefresh = true;
			mDirtyCurve = dirtyCurve;
			mDirtyOrientation = dirtyOrientation;
		}

		public void SetDirty(CurvySplineSegment controlPoint, bool dirtyCurve = true, bool dirtyOrientation = true)
		{
			if ((bool)controlPoint)
			{
				if (controlPoint.Spline != this && controlPoint.Spline != null)
				{
					controlPoint.Spline.SetDirty(controlPoint, dirtyCurve, dirtyOrientation);
					return;
				}
				if (!mDirtyControlPoints.Contains(controlPoint))
				{
					mDirtyControlPoints.Add(controlPoint);
					if ((bool)controlPoint.FollowUp)
					{
						controlPoint.FollowUp.SetDirty();
					}
				}
			}
			mDirtyCurve = mDirtyCurve || dirtyCurve;
			mDirtyOrientation = mDirtyCurve || dirtyOrientation;
		}

		public void SyncHierarchyFromSpline(bool renameControlPoints = true)
		{
			for (int num = ControlPoints.Count - 1; num >= 0; num--)
			{
				if (ControlPoints[num] == null)
				{
					ControlPoints.RemoveAt(num);
				}
			}
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				ControlPoints[i].ControlPointIndex = i;
				ControlPoints[i].transform.SetSiblingIndex(i);
				if (renameControlPoints)
				{
					ControlPoints[i].ApplyName();
				}
			}
		}

		public Vector3 ToWorldPosition(Vector3 localPosition)
		{
			return base.transform.TransformPoint(localPosition);
		}

		public void ApplyControlPointsNames()
		{
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				ControlPoints[i].ApplyName();
			}
		}

		public void SyncSplineFromHierarchy()
		{
			Segments.Clear();
			ControlPoints.Clear();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				CurvySplineSegment component = base.transform.GetChild(i).GetComponent<CurvySplineSegment>();
				if ((bool)component)
				{
					component.reSettleINTERNAL(false);
					ControlPoints.Add(component);
					component.TTransform.FromTransform(component.transform);
				}
			}
		}

		public bool IsPlanar(out int ignoreAxis)
		{
			bool xplanar;
			bool yplanar;
			bool zplanar;
			bool result = IsPlanar(out xplanar, out yplanar, out zplanar);
			if (xplanar)
			{
				ignoreAxis = 0;
			}
			else if (yplanar)
			{
				ignoreAxis = 1;
			}
			else
			{
				ignoreAxis = 2;
			}
			return result;
		}

		public bool IsPlanar(out bool xplanar, out bool yplanar, out bool zplanar)
		{
			xplanar = true;
			yplanar = true;
			zplanar = true;
			if (ControlPointCount == 0)
			{
				return true;
			}
			Vector3 localPosition = ControlPoints[0].TTransform.localPosition;
			for (int i = 1; i < ControlPointCount; i++)
			{
				if (!Mathf.Approximately(ControlPoints[i].TTransform.localPosition.x, localPosition.x))
				{
					xplanar = false;
				}
				if (!Mathf.Approximately(ControlPoints[i].TTransform.localPosition.y, localPosition.y))
				{
					yplanar = false;
				}
				if (!Mathf.Approximately(ControlPoints[i].TTransform.localPosition.z, localPosition.z))
				{
					zplanar = false;
				}
				if (!xplanar && !yplanar && !zplanar)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsPlanar(CurvyPlane plane)
		{
			switch (plane)
			{
			case CurvyPlane.XY:
			{
				for (int j = 0; j < ControlPointCount; j++)
				{
					if (ControlPoints[j].localPosition.z != 0f)
					{
						return false;
					}
				}
				break;
			}
			case CurvyPlane.XZ:
			{
				for (int k = 0; k < ControlPointCount; k++)
				{
					if (ControlPoints[k].localPosition.y != 0f)
					{
						return false;
					}
				}
				break;
			}
			case CurvyPlane.YZ:
			{
				for (int i = 0; i < ControlPointCount; i++)
				{
					if (ControlPoints[i].localPosition.x != 0f)
					{
						return false;
					}
				}
				break;
			}
			}
			return true;
		}

		public void MakePlanar(CurvyPlane plane)
		{
			switch (plane)
			{
			case CurvyPlane.XY:
			{
				for (int j = 0; j < ControlPointCount; j++)
				{
					if (ControlPoints[j].localPosition.z != 0f)
					{
						ControlPoints[j].localPosition = new Vector3(ControlPoints[j].localPosition.x, ControlPoints[j].localPosition.y, 0f);
					}
				}
				break;
			}
			case CurvyPlane.XZ:
			{
				for (int k = 0; k < ControlPointCount; k++)
				{
					if (ControlPoints[k].localPosition.y != 0f)
					{
						ControlPoints[k].localPosition = new Vector3(ControlPoints[k].localPosition.x, 0f, ControlPoints[k].localPosition.z);
					}
				}
				break;
			}
			case CurvyPlane.YZ:
			{
				for (int i = 0; i < ControlPointCount; i++)
				{
					if (ControlPoints[i].localPosition.x != 0f)
					{
						ControlPoints[i].localPosition = new Vector3(0f, ControlPoints[i].localPosition.y, ControlPoints[i].localPosition.z);
					}
				}
				break;
			}
			}
		}

		public void Subdivide(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
		{
			if (!fromCP)
			{
				fromCP = FirstVisibleControlPoint;
			}
			if (!toCP)
			{
				toCP = LastVisibleControlPoint;
			}
			if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
			{
				Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
				return;
			}
			int num = Mathf.Clamp(fromCP.ControlPointIndex, 0, ControlPointCount - 2);
			int num2 = Mathf.Clamp(toCP.ControlPointIndex, num + 1, ControlPointCount - 1);
			if (num2 - num < 1)
			{
				Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
				return;
			}
			for (int num3 = num2 - 1; num3 >= num; num3--)
			{
				Vector3 localPosition = ControlPoints[num3].Interpolate(0.5f);
				CurvySplineSegment curvySplineSegment = InsertAfter(ControlPoints[num3]);
				curvySplineSegment.localPosition = localPosition;
			}
		}

		public void Simplify(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
		{
			if (!fromCP)
			{
				fromCP = FirstVisibleControlPoint;
			}
			if (!toCP)
			{
				toCP = LastVisibleControlPoint;
			}
			if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
			{
				Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
				return;
			}
			int num = Mathf.Clamp(fromCP.ControlPointIndex, 0, ControlPointCount - 2);
			int num2 = Mathf.Clamp(toCP.ControlPointIndex, num + 2, ControlPointCount - 1);
			if (num2 - num < 2)
			{
				Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
				return;
			}
			for (int num3 = num2 - 2; num3 >= num; num3 -= 2)
			{
				ControlPoints[num3 + 1].Delete();
			}
		}

		public void Equalize(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
		{
			if (!fromCP)
			{
				fromCP = FirstVisibleControlPoint;
			}
			if (!toCP)
			{
				toCP = LastVisibleControlPoint;
			}
			if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
			{
				Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
				return;
			}
			int num = Mathf.Clamp(fromCP.ControlPointIndex, 0, ControlPointCount - 2);
			int num2 = Mathf.Clamp(toCP.ControlPointIndex, num + 2, ControlPointCount - 1);
			if (num2 - num < 2)
			{
				Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
				return;
			}
			float num3 = ControlPoints[num2].Distance - ControlPoints[num].Distance;
			float num4 = num3 / (float)(num2 - num);
			float num5 = ControlPoints[num].Distance;
			for (int i = num + 1; i < num2; i++)
			{
				num5 += num4;
				ControlPoints[i].localPosition = InterpolateByDistance(num5);
			}
		}

		public void Normalize()
		{
			Vector3 localScale = base.transform.localScale;
			if (localScale != Vector3.one)
			{
				base.transform.localScale = Vector3.one;
				for (int i = 0; i < ControlPointCount; i++)
				{
					ControlPoints[i].localPosition = Vector3.Scale(ControlPoints[i].localPosition, localScale);
				}
			}
		}

		public void MakePlanar(int axis)
		{
			Vector3 localPosition = ControlPoints[0].transform.localPosition;
			for (int i = 1; i < ControlPointCount; i++)
			{
				Vector3 localPosition2 = ControlPoints[i].transform.localPosition;
				switch (axis)
				{
				case 0:
					localPosition2.x = localPosition.x;
					break;
				case 1:
					localPosition2.y = localPosition.y;
					break;
				case 2:
					localPosition2.z = localPosition.z;
					break;
				}
				ControlPoints[i].transform.localPosition = localPosition2;
			}
			SetDirtyAll();
		}

		public Vector3 SetPivot(float xRel = 0f, float yRel = 0f, float zRel = 0f, bool preview = false)
		{
			Bounds bounds = Bounds;
			Vector3 vector = new Vector3(bounds.min.x + bounds.size.x * ((xRel + 1f) / 2f), bounds.max.y - bounds.size.y * ((yRel + 1f) / 2f), bounds.min.z + bounds.size.z * ((zRel + 1f) / 2f));
			Vector3 vector2 = base.transform.position - vector;
			if (preview)
			{
				return base.transform.position - vector2;
			}
			foreach (CurvySplineSegment controlPoint in ControlPoints)
			{
				controlPoint.transform.position += vector2;
			}
			base.transform.position -= vector2;
			SetDirtyAll();
			return base.transform.position;
		}

		public void Flip()
		{
			if (ControlPointCount <= 1)
			{
				return;
			}
			switch (Interpolation)
			{
			case CurvyInterpolation.TCB:
			{
				Bias *= -1f;
				for (int num2 = ControlPointCount - 1; num2 >= 0; num2--)
				{
					CurvySplineSegment curvySplineSegment2 = ControlPoints[num2];
					int num3 = num2 - 1;
					if (num3 >= 0)
					{
						CurvySplineSegment curvySplineSegment3 = ControlPoints[num3];
						curvySplineSegment2.EndBias = curvySplineSegment3.StartBias * -1f;
						curvySplineSegment2.EndContinuity = curvySplineSegment3.StartContinuity;
						curvySplineSegment2.EndTension = curvySplineSegment3.StartTension;
						curvySplineSegment2.StartBias = curvySplineSegment3.EndBias * -1f;
						curvySplineSegment2.StartContinuity = curvySplineSegment3.EndContinuity;
						curvySplineSegment2.StartTension = curvySplineSegment3.EndTension;
						curvySplineSegment2.OverrideGlobalBias = curvySplineSegment3.OverrideGlobalBias;
						curvySplineSegment2.OverrideGlobalContinuity = curvySplineSegment3.OverrideGlobalContinuity;
						curvySplineSegment2.OverrideGlobalTension = curvySplineSegment3.OverrideGlobalTension;
						curvySplineSegment2.SynchronizeTCB = curvySplineSegment3.SynchronizeTCB;
					}
				}
				break;
			}
			case CurvyInterpolation.Bezier:
			{
				for (int num = ControlPointCount - 1; num >= 0; num--)
				{
					CurvySplineSegment curvySplineSegment = ControlPoints[num];
					Vector3 handleIn = curvySplineSegment.HandleIn;
					curvySplineSegment.HandleIn = curvySplineSegment.HandleOut;
					curvySplineSegment.HandleOut = handleIn;
				}
				break;
			}
			}
			ControlPoints.Reverse();
			SetDirtyAll();
			SyncHierarchyFromSpline();
			Refresh();
		}

		public void MoveControlPoints(int startIndex, int count, CurvySplineSegment destCP)
		{
			if ((bool)destCP && !(this == destCP.Spline) && destCP.ControlPointIndex != -1)
			{
				startIndex = Mathf.Clamp(startIndex, 0, ControlPointCount - 1);
				count = Mathf.Clamp(count, startIndex, ControlPointCount - startIndex);
				for (int i = 0; i < count; i++)
				{
					CurvySplineSegment curvySplineSegment = ControlPoints[startIndex];
					curvySplineSegment.reSettleINTERNAL();
					curvySplineSegment.transform.SetParent(destCP.Spline.transform, true);
					destCP.Spline.ControlPoints.Insert(destCP.ControlPointIndex + i + 1, curvySplineSegment);
				}
				destCP.SetDirty();
				Refresh();
				destCP.Spline.Refresh();
			}
		}

		public void JoinWith(CurvySplineSegment destCP)
		{
			if (!(destCP.Spline == this))
			{
				MoveControlPoints(0, ControlPointCount, destCP);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void SetFromString(string fieldAndValue)
		{
			string[] array = fieldAndValue.Split('=');
			if (array.Length != 2)
			{
				return;
			}
			FieldInfo fieldInfo = GetType().FieldByName(array[0], true);
			if (fieldInfo != null)
			{
				try
				{
					if (fieldInfo.FieldType.IsEnum)
					{
						fieldInfo.SetValue(this, Enum.Parse(fieldInfo.FieldType, array[1]));
					}
					else
					{
						fieldInfo.SetValue(this, Convert.ChangeType(array[1], fieldInfo.FieldType));
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogWarning(base.name + ".SetFromString(): " + ex.ToString());
					return;
				}
			}
			PropertyInfo propertyInfo = GetType().PropertyByName(array[0], true);
			if (propertyInfo == null)
			{
				return;
			}
			try
			{
				if (propertyInfo.PropertyType.IsEnum)
				{
					propertyInfo.SetValue(this, Enum.Parse(propertyInfo.PropertyType, array[1]), null);
				}
				else
				{
					propertyInfo.SetValue(this, Convert.ChangeType(array[1], propertyInfo.PropertyType), null);
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning(base.name + ".SetFromString(): " + ex2.ToString());
			}
		}

		protected override bool UpgradeVersion(string oldVersion, string newVersion)
		{
			if (string.IsNullOrEmpty(oldVersion))
			{
				ControlPoints.Sort((CurvySplineSegment x, CurvySplineSegment y) => x.name.CompareTo(y.name));
			}
			return true;
		}

		private void doUpdate()
		{
			if (base.TTransform != base.transform)
			{
				base.TTransform.FromTransform(base.transform);
				clearBounds();
			}
			if (!base.IsInitialized)
			{
				SetDirtyAll();
			}
			if (CheckTransform || !Application.isPlaying)
			{
				for (int i = 0; i < ControlPointCount; i++)
				{
					ControlPoints[i].RefreshTransform();
				}
			}
			if (Dirty)
			{
				Refresh();
			}
		}

		private Bounds getBounds()
		{
			if (Count > 0)
			{
				Bounds bounds = this[0].Bounds;
				for (int i = 1; i < Count; i++)
				{
					bounds.Encapsulate(this[i].Bounds);
				}
				return bounds;
			}
			return new Bounds(base.TTransform.position, Vector3.zero);
		}

		private void clearBounds()
		{
			mBounds = null;
			for (int i = 0; i < ControlPointCount; i++)
			{
				ControlPoints[i].ClearBoundsINTERNAL();
			}
		}

		private bool canHaveManualEndCP()
		{
			return !Closed && (Interpolation == CurvyInterpolation.CatmullRom || Interpolation == CurvyInterpolation.TCB);
		}

		internal void setLengthINTERNAL(float length)
		{
			mLength = length;
		}

		private bool getPreviousApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
		{
			residx = idx - 1;
			res = seg;
			if (residx < 0)
			{
				res = seg.PreviousSegment;
				if ((bool)res)
				{
					residx = res.Approximation.Length - 2;
					for (int i = 0; i < validSegments.Length; i++)
					{
						if (validSegments[i] == res)
						{
							return true;
						}
					}
					return false;
				}
				return false;
			}
			return true;
		}

		private bool getNextApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
		{
			residx = idx + 1;
			res = seg;
			if (residx == seg.Approximation.Length)
			{
				res = seg.NextSegment;
				if ((bool)res)
				{
					residx = 1;
					for (int i = 0; i < validSegments.Length; i++)
					{
						if (validSegments[i] == res)
						{
							return true;
						}
					}
				}
				return false;
			}
			return true;
		}

		private CurvySplineSegment getNextSegmentWithinRange(CurvySplineSegment seg, float tf, float fDistance, out float segTF)
		{
			segTF = -1f;
			if ((bool)seg)
			{
				if (fDistance > 0f && tf < 1f)
				{
					CurvySplineSegment nextControlPoint = seg.NextControlPoint;
					segTF = SegmentToTF(nextControlPoint);
					if (segTF == 0f && Closed)
					{
						segTF = 1f;
					}
					return (!(segTF - tf <= fDistance)) ? null : nextControlPoint;
				}
				if (fDistance < 0f && tf > 0f)
				{
					if (tf == 1f && Closed)
					{
						tf = 0f;
					}
					CurvySplineSegment curvySplineSegment = ((SegmentToTF(seg) != tf) ? seg : seg.PreviousControlPoint);
					segTF = SegmentToTF(curvySplineSegment);
					return (!(Mathf.Abs(tf - segTF) <= 0f - fDistance)) ? null : curvySplineSegment;
				}
			}
			return null;
		}

		private CurvySplineSegment getNextSegmentWithinRangeDistance(CurvySplineSegment seg, float tf, float tfDist, float distance, out float segDistance)
		{
			segDistance = -1f;
			if ((bool)seg)
			{
				if (distance > 0f && tf < 1f)
				{
					CurvySplineSegment nextControlPoint = seg.NextControlPoint;
					segDistance = ((nextControlPoint.Distance != 0f || !Closed) ? nextControlPoint.Distance : base.Length);
					return (!nextControlPoint || !(segDistance - tfDist <= distance)) ? null : nextControlPoint;
				}
				if (distance < 0f && tf > 0f)
				{
					if (tf == 1f && Closed)
					{
						tf = 0f;
					}
					CurvySplineSegment curvySplineSegment = ((SegmentToTF(seg) != tf) ? seg : seg.PreviousControlPoint);
					if (!curvySplineSegment)
					{
						return null;
					}
					segDistance = curvySplineSegment.Distance;
					return (!curvySplineSegment || !(Mathf.Abs(tfDist - segDistance) <= 0f - distance)) ? null : curvySplineSegment;
				}
			}
			return null;
		}

		private Vector3 eventAwareMove(ref float tf, ref int direction, float fDistance, CurvyClamping clamping, bool fastMode)
		{
			CurvySplineSegment controlPoint = TFToSegment(tf);
			CurvySplineMoveEventArgs moveEventArgs = _moveEventArgs;
			moveEventArgs.Sender = this;
			moveEventArgs.Spline = this;
			moveEventArgs.ControlPoint = controlPoint;
			moveEventArgs.TF = tf;
			moveEventArgs.Delta = fDistance;
			moveEventArgs.Direction = direction;
			moveEventArgs.WorldUnits = false;
			moveEventArgs.Cancel = false;
			int num = 2000;
			while (!moveEventArgs.Cancel && moveEventArgs.Delta > 0f && num-- > 0)
			{
				float segTF;
				CurvySplineSegment nextSegmentWithinRange = moveEventArgs.Spline.getNextSegmentWithinRange(moveEventArgs.ControlPoint, moveEventArgs.TF, moveEventArgs.Delta * (float)moveEventArgs.Direction, out segTF);
				if ((bool)nextSegmentWithinRange)
				{
					moveEventArgs.ControlPoint = nextSegmentWithinRange;
					moveEventArgs.Delta -= Mathf.Abs(segTF - moveEventArgs.TF);
					bool flag = moveEventArgs.TF != segTF;
					moveEventArgs.TF = segTF;
					if (flag)
					{
						moveEventArgs.Spline.OnMoveControlPointReachedEvent(moveEventArgs);
					}
					if (moveEventArgs.Direction < 0)
					{
						if (moveEventArgs.TF != 0f)
						{
							continue;
						}
						switch (clamping)
						{
						case CurvyClamping.Clamp:
							moveEventArgs.Delta = 0f;
							break;
						case CurvyClamping.Loop:
							moveEventArgs.TF = 1f;
							moveEventArgs.ControlPoint = LastVisibleControlPoint;
							if (!Closed)
							{
								OnMoveControlPointReachedEvent(moveEventArgs);
							}
							break;
						case CurvyClamping.PingPong:
							moveEventArgs.Direction *= -1;
							break;
						}
					}
					else
					{
						if (moveEventArgs.Direction < 0 || moveEventArgs.TF != 1f)
						{
							continue;
						}
						switch (clamping)
						{
						case CurvyClamping.Clamp:
							moveEventArgs.Delta = 0f;
							break;
						case CurvyClamping.Loop:
							moveEventArgs.TF = 0f;
							moveEventArgs.ControlPoint = moveEventArgs.Spline.FirstVisibleControlPoint;
							if (!Closed)
							{
								moveEventArgs.Spline.OnMoveControlPointReachedEvent(moveEventArgs);
							}
							break;
						case CurvyClamping.PingPong:
							moveEventArgs.Direction *= -1;
							break;
						}
					}
				}
				else
				{
					moveEventArgs.TF += moveEventArgs.Delta * (float)moveEventArgs.Direction;
					moveEventArgs.Delta = 0f;
				}
			}
			if (num <= 0)
			{
				Debug.Log("[Curvy] HE'S DEAD, JIM! Infinite loops shouldn't happen! Please raise a Bug Report!");
			}
			tf = moveEventArgs.TF;
			direction = moveEventArgs.Direction;
			return (!fastMode) ? moveEventArgs.Spline.Interpolate(tf) : moveEventArgs.Spline.InterpolateFast(tf);
		}

		private Vector3 eventAwareMoveDistance(ref float tf, ref int direction, float distance, CurvyClamping clamping, bool fastMode)
		{
			CurvySplineSegment controlPoint = TFToSegment(tf);
			CurvySplineMoveEventArgs moveEventArgs = _moveEventArgs;
			moveEventArgs.Sender = this;
			moveEventArgs.Spline = this;
			moveEventArgs.ControlPoint = controlPoint;
			moveEventArgs.TF = tf;
			moveEventArgs.Delta = distance;
			moveEventArgs.Direction = direction;
			moveEventArgs.WorldUnits = true;
			moveEventArgs.Cancel = false;
			int num = 2000;
			while (!moveEventArgs.Cancel && moveEventArgs.Delta > 0f && num-- > 0)
			{
				float num2 = moveEventArgs.Spline.TFToDistance(moveEventArgs.TF);
				float segDistance;
				CurvySplineSegment nextSegmentWithinRangeDistance = moveEventArgs.Spline.getNextSegmentWithinRangeDistance(moveEventArgs.ControlPoint, moveEventArgs.TF, num2, moveEventArgs.Delta * (float)moveEventArgs.Direction, out segDistance);
				if ((bool)nextSegmentWithinRangeDistance)
				{
					moveEventArgs.ControlPoint = nextSegmentWithinRangeDistance;
					moveEventArgs.Delta -= Mathf.Abs(segDistance - num2);
					bool flag = moveEventArgs.TF != segDistance;
					moveEventArgs.TF = moveEventArgs.Spline.SegmentToTF(nextSegmentWithinRangeDistance);
					if (flag)
					{
						OnMoveControlPointReachedEvent(moveEventArgs);
					}
					if (moveEventArgs.Spline != this)
					{
						num2 = moveEventArgs.Spline.TFToDistance(moveEventArgs.TF);
					}
					if (moveEventArgs.Direction < 0)
					{
						if (moveEventArgs.TF != 0f)
						{
							continue;
						}
						switch (clamping)
						{
						case CurvyClamping.Clamp:
							moveEventArgs.Delta = 0f;
							break;
						case CurvyClamping.Loop:
							moveEventArgs.TF = 1f;
							moveEventArgs.ControlPoint = LastVisibleControlPoint;
							if (!Closed)
							{
								OnMoveControlPointReachedEvent(moveEventArgs);
							}
							break;
						case CurvyClamping.PingPong:
							moveEventArgs.Direction *= -1;
							break;
						}
					}
					else
					{
						if (moveEventArgs.Direction < 0 || moveEventArgs.TF != 1f)
						{
							continue;
						}
						switch (clamping)
						{
						case CurvyClamping.Clamp:
							moveEventArgs.Delta = 0f;
							break;
						case CurvyClamping.Loop:
							moveEventArgs.TF = 0f;
							moveEventArgs.ControlPoint = FirstVisibleControlPoint;
							if (!Closed)
							{
								OnMoveControlPointReachedEvent(moveEventArgs);
							}
							break;
						case CurvyClamping.PingPong:
							moveEventArgs.Direction *= -1;
							break;
						}
					}
				}
				else
				{
					moveEventArgs.TF = moveEventArgs.Spline.DistanceToTF(num2 + moveEventArgs.Delta * (float)moveEventArgs.Direction);
					moveEventArgs.Delta = 0f;
				}
			}
			if (num <= 0)
			{
				Debug.Log("[Curvy] HE'S DEAD, JIM! Infinite loops shouldn't happen! Please raise a Bug Report!");
			}
			tf = moveEventArgs.TF;
			direction = moveEventArgs.Direction;
			return (!fastMode) ? moveEventArgs.Spline.Interpolate(tf) : moveEventArgs.Spline.InterpolateFast(tf);
		}

		internal bool MoveByAngleExtINTERNAL(ref float tf, float minDistance, float maxDistance, float maxAngle, out Vector3 pos, out Vector3 tan, out float movedDistance, float stopTF = float.MaxValue, bool loop = true, float stepDist = -1f)
		{
			if (stepDist == -1f)
			{
				stepDist = 1f / (float)CacheSize;
			}
			minDistance = Mathf.Max(0f, minDistance);
			maxDistance = Mathf.Max(minDistance, maxDistance);
			if (!loop)
			{
				tf = Mathf.Clamp01(tf);
			}
			float tf2 = ((!loop) ? tf : (tf % 1f));
			pos = Interpolate(tf2);
			tan = GetTangent(tf2, pos);
			Vector3 vector = pos;
			Vector3 vector2 = tan;
			movedDistance = 0f;
			float num = 0f;
			int num2 = 0;
			if (stopTF < tf && loop)
			{
				stopTF += 1f;
			}
			while (tf < stopTF)
			{
				tf = Mathf.Min(stopTF, tf + stepDist);
				tf2 = ((!loop) ? tf : (tf % 1f));
				pos = Interpolate(tf2);
				tan = GetTangent(tf2, pos);
				movedDistance += (pos - vector).magnitude;
				num += Vector3.Angle(vector2, tan);
				if (tan == vector2)
				{
					num2++;
				}
				if (movedDistance >= minDistance)
				{
					if (movedDistance >= maxDistance)
					{
						break;
					}
					if (num >= maxAngle)
					{
						num = 0f;
						break;
					}
					if (num2 > 0 && num > 0f)
					{
						break;
					}
				}
				vector = pos;
				vector2 = tan;
			}
			return Mathf.Approximately(tf, stopTF);
		}

		private CurvySplineSegment getCurrentAnchorGroup(CurvySplineSegment seg)
		{
			if (!AutoEndTangents && seg.IsFirstControlPoint)
			{
				return FirstVisibleControlPoint;
			}
			while ((bool)seg && !seg.OrientationAnchor)
			{
				seg = seg.PreviousSegment;
			}
			return seg;
		}
	}
}
