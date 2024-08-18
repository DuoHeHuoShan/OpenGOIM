using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(PoolManager))]
	public class CurvyGlobalManager : DTSingleton<CurvyGlobalManager>, IDTSingleton
	{
		public static bool HideManager = false;

		public static int MaxCachePPU = 8;

		public static float SceneViewResolution = 0.5f;

		public static Color DefaultGizmoColor = new Color(0.71f, 0.71f, 0.71f);

		public static Color DefaultGizmoSelectionColor = new Color(0.15f, 0.35f, 0.68f);

		public static CurvyInterpolation DefaultInterpolation = CurvyInterpolation.CatmullRom;

		public static float GizmoControlPointSize = 0.15f;

		public static float GizmoOrientationLength = 1f;

		public static Color GizmoOrientationColor = new Color(0.75f, 0.75f, 0.4f);

		public static int SplineLayer = 0;

		public static CurvySplineGizmos Gizmos = CurvySplineGizmos.Curve | CurvySplineGizmos.Orientation;

		private PoolManager mPoolManager;

		private ComponentPool mControlPointPool;

		public static bool ShowCurveGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Curve) == CurvySplineGizmos.Curve;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Curve;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Curve;
				}
			}
		}

		public static bool ShowApproximationGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Approximation) == CurvySplineGizmos.Approximation;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Approximation;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Approximation;
				}
			}
		}

		public static bool ShowTangentsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Tangents) == CurvySplineGizmos.Tangents;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Tangents;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Tangents;
				}
			}
		}

		public static bool ShowOrientationGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Orientation) == CurvySplineGizmos.Orientation;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Orientation;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Orientation;
				}
			}
		}

		public static bool ShowLabelsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Labels) == CurvySplineGizmos.Labels;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Labels;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Labels;
				}
			}
		}

		public static bool ShowMetadataGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Metadata) == CurvySplineGizmos.Metadata;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Metadata;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Metadata;
				}
			}
		}

		public static bool ShowBoundsGizmo
		{
			get
			{
				return (Gizmos & CurvySplineGizmos.Bounds) == CurvySplineGizmos.Bounds;
			}
			set
			{
				if (value)
				{
					Gizmos |= CurvySplineGizmos.Bounds;
				}
				else
				{
					Gizmos &= ~CurvySplineGizmos.Bounds;
				}
			}
		}

		public PoolManager PoolManager
		{
			get
			{
				if (mPoolManager == null)
				{
					mPoolManager = GetComponent<PoolManager>();
				}
				return mPoolManager;
			}
		}

		public ComponentPool ControlPointPool
		{
			get
			{
				return mControlPointPool;
			}
		}

		public CurvyConnection[] Connections
		{
			get
			{
				return GetComponentsInChildren<CurvyConnection>();
			}
		}

		public CurvyConnection[] GetContainingConnections(params CurvySpline[] splines)
		{
			List<CurvyConnection> list = new List<CurvyConnection>();
			List<CurvySpline> list2 = new List<CurvySpline>(splines);
			foreach (CurvySpline item in list2)
			{
				foreach (CurvySplineSegment controlPoint in item.ControlPoints)
				{
					if (!(controlPoint.Connection != null) || list.Contains(controlPoint.Connection))
					{
						continue;
					}
					bool flag = true;
					foreach (CurvySplineSegment controlPoint2 in controlPoint.Connection.ControlPoints)
					{
						if (!list2.Contains(controlPoint2.Spline))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(controlPoint.Connection);
					}
				}
			}
			return list.ToArray();
		}

		public override void Awake()
		{
			base.Awake();
			base.name = "_CurvyGlobal_";
			base.transform.SetAsLastSibling();
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad(this);
			}
			mPoolManager = GetComponent<PoolManager>();
			PoolSettings poolSettings = new PoolSettings();
			poolSettings.MinItems = 0;
			poolSettings.Threshold = 50;
			poolSettings.Prewarm = true;
			poolSettings.AutoCreate = true;
			poolSettings.AutoEnableDisable = true;
			PoolSettings settings = poolSettings;
			mControlPointPool = mPoolManager.CreateComponentPool<CurvySplineSegment>(settings);
		}

		private void Start()
		{
			if (HideManager)
			{
				base.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			else
			{
				base.gameObject.hideFlags = HideFlags.None;
			}
		}

		[RuntimeInitializeOnLoadMethod]
		private static void LoadRuntimeSettings()
		{
			if (!PlayerPrefs.HasKey("Curvy_MaxCachePPU"))
			{
				SaveRuntimeSettings();
			}
			MaxCachePPU = DTUtility.GetPlayerPrefs("Curvy_MaxCachePPU", MaxCachePPU);
			SceneViewResolution = DTUtility.GetPlayerPrefs("Curvy_SceneViewResolution", SceneViewResolution);
			HideManager = DTUtility.GetPlayerPrefs("Curvy_HideManager", HideManager);
			DefaultGizmoColor = DTUtility.GetPlayerPrefs("Curvy_DefaultGizmoColor", DefaultGizmoColor);
			DefaultGizmoSelectionColor = DTUtility.GetPlayerPrefs("Curvy_DefaultGizmoSelectionColor", DefaultGizmoColor);
			DefaultInterpolation = DTUtility.GetPlayerPrefs("Curvy_DefaultInterpolation", DefaultInterpolation);
			GizmoControlPointSize = DTUtility.GetPlayerPrefs("Curvy_ControlPointSize", GizmoControlPointSize);
			GizmoOrientationLength = DTUtility.GetPlayerPrefs("Curvy_OrientationLength", GizmoOrientationLength);
			GizmoOrientationColor = DTUtility.GetPlayerPrefs("Curvy_OrientationColor", GizmoOrientationColor);
			Gizmos = DTUtility.GetPlayerPrefs("Curvy_Gizmos", Gizmos);
			SplineLayer = DTUtility.GetPlayerPrefs("Curvy_SplineLayer", SplineLayer);
		}

		public static void SaveRuntimeSettings()
		{
			DTUtility.SetPlayerPrefs("Curvy_MaxCachePPU", MaxCachePPU);
			DTUtility.SetPlayerPrefs("Curvy_SceneViewResolution", SceneViewResolution);
			DTUtility.SetPlayerPrefs("Curvy_HideManager", HideManager);
			DTUtility.SetPlayerPrefs("Curvy_DefaultGizmoColor", DefaultGizmoColor);
			DTUtility.SetPlayerPrefs("Curvy_DefaultGizmoSelectionColor", DefaultGizmoSelectionColor);
			DTUtility.SetPlayerPrefs("Curvy_DefaultInterpolation", DefaultInterpolation);
			DTUtility.SetPlayerPrefs("Curvy_ControlPointSize", GizmoControlPointSize);
			DTUtility.SetPlayerPrefs("Curvy_OrientationLength", GizmoOrientationLength);
			DTUtility.SetPlayerPrefs("Curvy_OrientationColor", GizmoOrientationColor);
			DTUtility.SetPlayerPrefs("Curvy_Gizmos", Gizmos);
			DTUtility.SetPlayerPrefs("Curvy_SplineLayer", SplineLayer);
			PlayerPrefs.Save();
		}

		public new void MergeDoubleLoaded(IDTSingleton newInstance)
		{
			CurvyGlobalManager curvyGlobalManager = newInstance as CurvyGlobalManager;
			CurvyConnection[] connections = curvyGlobalManager.Connections;
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i].transform.SetParent(base.transform);
			}
		}
	}
}
