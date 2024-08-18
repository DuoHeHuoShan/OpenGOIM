using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/curvysplinesegment")]
	public class CurvySplineSegment : MonoBehaviour, IComparable, IPoolable
	{
		[Group("General")]
		[FieldAction("CBBakeOrientation", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[Label("Bake Orientation", "Automatically apply orientation to CP transforms?")]
		[SerializeField]
		private bool m_AutoBakeOrientation;

		[Group("General")]
		[Tooltip("Check to use this transform's rotation")]
		[FieldCondition("IsFirstSegment", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[FieldCondition("isDynamicOrientation", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_OrientationAnchor;

		[Label("Swirl", "Add Swirl to orientation?")]
		[Group("General")]
		[FieldCondition("canHaveSwirl", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private CurvyOrientationSwirl m_Swirl;

		[Label("Turns", "Number of swirl turns")]
		[Group("General")]
		[FieldCondition("canHaveSwirl", true, false, ConditionalAttribute.OperatorEnum.AND, "m_Swirl", CurvyOrientationSwirl.None, true)]
		[SerializeField]
		private float m_SwirlTurns;

		[Section("Bezier Options", true, false, 100, Sort = 1, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvysplinesegment_bezier")]
		[GroupCondition("interpolation", CurvyInterpolation.Bezier, false)]
		[SerializeField]
		private bool m_AutoHandles = true;

		[RangeEx(0f, 1f, "Distance %", "Handle length by distance to neighbours")]
		[FieldCondition("m_AutoHandles", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		private float m_AutoHandleDistance = 0.39f;

		[VectorEx("", "", Precision = 3, Options = (AttributeOptionsFlags)1152, Color = "#FFFF00")]
		[SerializeField]
		[FormerlySerializedAs("HandleIn")]
		private Vector3 m_HandleIn = new Vector3(-1f, 0f, 0f);

		[VectorEx("", "", Precision = 3, Options = (AttributeOptionsFlags)1152, Color = "#00FF00")]
		[SerializeField]
		[FormerlySerializedAs("HandleOut")]
		private Vector3 m_HandleOut = new Vector3(1f, 0f, 0f);

		[Section("TCB Options", true, false, 100, Sort = 1, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvysplinesegment_tcb")]
		[GroupCondition("interpolation", CurvyInterpolation.TCB, false)]
		[GroupAction("TCBOptionsGUI", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[Label("Local Tension", "Override Spline Tension?")]
		[SerializeField]
		[FormerlySerializedAs("OverrideGlobalTension")]
		private bool m_OverrideGlobalTension;

		[Label("Local Continuity", "Override Spline Continuity?")]
		[SerializeField]
		[FormerlySerializedAs("OverrideGlobalContinuity")]
		private bool m_OverrideGlobalContinuity;

		[Label("Local Bias", "Override Spline Bias?")]
		[SerializeField]
		[FormerlySerializedAs("OverrideGlobalBias")]
		private bool m_OverrideGlobalBias;

		[Tooltip("Synchronize Start and End Values")]
		[SerializeField]
		[FormerlySerializedAs("SynchronizeTCB")]
		private bool m_SynchronizeTCB = true;

		[Label("Tension", "")]
		[FieldCondition("m_OverrideGlobalTension", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("StartTension")]
		private float m_StartTension;

		[Label("Tension (End)", "")]
		[FieldCondition("m_OverrideGlobalTension", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
		[SerializeField]
		[FormerlySerializedAs("EndTension")]
		private float m_EndTension;

		[Label("Continuity", "")]
		[FieldCondition("m_OverrideGlobalContinuity", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("StartContinuity")]
		private float m_StartContinuity;

		[Label("Continuity (End)", "")]
		[FieldCondition("m_OverrideGlobalContinuity", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
		[SerializeField]
		[FormerlySerializedAs("EndContinuity")]
		private float m_EndContinuity;

		[Label("Bias", "")]
		[FieldCondition("m_OverrideGlobalBias", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("StartBias")]
		private float m_StartBias;

		[Label("Bias (End)", "")]
		[FieldCondition("m_OverrideGlobalBias", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
		[SerializeField]
		[FormerlySerializedAs("EndBias")]
		private float m_EndBias;

		[SerializeField]
		[HideInInspector]
		private CurvySplineSegment m_FollowUp;

		[SerializeField]
		[HideInInspector]
		private ConnectionHeadingEnum m_FollowUpHeading = ConnectionHeadingEnum.Auto;

		[SerializeField]
		[HideInInspector]
		private bool m_ConnectionSyncPosition;

		[SerializeField]
		[HideInInspector]
		private bool m_ConnectionSyncRotation;

		[SerializeField]
		[HideInInspector]
		private CurvyConnection m_Connection;

		[NonSerialized]
		public Vector3[] Approximation = new Vector3[0];

		[NonSerialized]
		public float[] ApproximationDistances = new float[0];

		[NonSerialized]
		public Vector3[] ApproximationUp = new Vector3[0];

		[NonSerialized]
		public Vector3[] ApproximationT = new Vector3[0];

		private TTransform mTTransform;

		private CurvySpline mSpline;

		private float mStepSize;

		private int mControlPointIndex = -1;

		private int mSegmentIndex = -1;

		private Bounds? mBounds;

		private int mCacheLastDistanceToLocalFIndex;

		private List<Component> mMetaData;

		public bool AutoBakeOrientation
		{
			get
			{
				return m_AutoBakeOrientation;
			}
			set
			{
				if (m_AutoBakeOrientation != value)
				{
					m_AutoBakeOrientation = value;
					SetDirty(false);
				}
			}
		}

		public bool OrientationAnchor
		{
			get
			{
				return m_OrientationAnchor;
			}
			set
			{
				if (m_OrientationAnchor != value)
				{
					m_OrientationAnchor = value;
					SetDirty(false);
				}
			}
		}

		public CurvyOrientationSwirl Swirl
		{
			get
			{
				return m_Swirl;
			}
			set
			{
				if (m_Swirl != value)
				{
					m_Swirl = value;
					SetDirty(false);
				}
			}
		}

		public float SwirlTurns
		{
			get
			{
				return m_SwirlTurns;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_SwirlTurns != num)
				{
					m_SwirlTurns = num;
					SetDirty(false);
				}
			}
		}

		public Vector3 HandleIn
		{
			get
			{
				return m_HandleIn;
			}
			set
			{
				if (m_HandleIn != value)
				{
					m_HandleIn = value;
					SetDirty();
				}
			}
		}

		public Vector3 HandleOut
		{
			get
			{
				return m_HandleOut;
			}
			set
			{
				if (m_HandleOut != value)
				{
					m_HandleOut = value;
					SetDirty();
				}
			}
		}

		public Vector3 HandleInPosition
		{
			get
			{
				return TTransform.position + Spline.TTransform.rotation * HandleIn;
			}
			set
			{
				HandleIn = Spline.transform.InverseTransformDirection(value - TTransform.position);
			}
		}

		public Vector3 HandleOutPosition
		{
			get
			{
				return TTransform.position + Spline.TTransform.rotation * HandleOut;
			}
			set
			{
				HandleOut = Spline.transform.InverseTransformDirection(value - TTransform.position);
			}
		}

		public bool AutoHandles
		{
			get
			{
				return m_AutoHandles;
			}
			set
			{
				if (m_AutoHandles != value)
				{
					m_AutoHandles = value;
					List<CurvySplineSegment> connectedControlPoints = ConnectedControlPoints;
					for (int i = 0; i < connectedControlPoints.Count; i++)
					{
						connectedControlPoints[i].m_AutoHandles = value;
						connectedControlPoints[i].SetDirty();
					}
				}
				SetDirty();
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
				}
				SetDirty();
			}
		}

		public bool SynchronizeTCB
		{
			get
			{
				return m_SynchronizeTCB;
			}
			set
			{
				if (m_SynchronizeTCB != value)
				{
					m_SynchronizeTCB = value;
					SetDirty();
				}
			}
		}

		public bool OverrideGlobalTension
		{
			get
			{
				return m_OverrideGlobalTension;
			}
			set
			{
				if (m_OverrideGlobalTension != value)
				{
					m_OverrideGlobalTension = value;
					SetDirty();
				}
			}
		}

		public bool OverrideGlobalContinuity
		{
			get
			{
				return m_OverrideGlobalContinuity;
			}
			set
			{
				if (m_OverrideGlobalContinuity != value)
				{
					m_OverrideGlobalContinuity = value;
					SetDirty();
				}
			}
		}

		public bool OverrideGlobalBias
		{
			get
			{
				return m_OverrideGlobalBias;
			}
			set
			{
				if (m_OverrideGlobalBias != value)
				{
					m_OverrideGlobalBias = value;
					SetDirty();
				}
			}
		}

		public float StartTension
		{
			get
			{
				return m_StartTension;
			}
			set
			{
				if (m_StartTension != value)
				{
					m_StartTension = value;
					SetDirty();
				}
			}
		}

		public float StartContinuity
		{
			get
			{
				return m_StartContinuity;
			}
			set
			{
				if (m_StartContinuity != value)
				{
					m_StartContinuity = value;
					SetDirty();
				}
			}
		}

		public float StartBias
		{
			get
			{
				return m_StartBias;
			}
			set
			{
				if (m_StartBias != value)
				{
					m_StartBias = value;
					SetDirty();
				}
			}
		}

		public float EndTension
		{
			get
			{
				return m_EndTension;
			}
			set
			{
				if (m_EndTension != value)
				{
					m_EndTension = value;
					SetDirty();
				}
			}
		}

		public float EndContinuity
		{
			get
			{
				return m_EndContinuity;
			}
			set
			{
				if (m_EndContinuity != value)
				{
					m_EndContinuity = value;
					SetDirty();
				}
			}
		}

		public float EndBias
		{
			get
			{
				return m_EndBias;
			}
			set
			{
				if (m_EndBias != value)
				{
					m_EndBias = value;
					SetDirty();
				}
			}
		}

		public bool CanHaveFollowUp
		{
			get
			{
				return IsFirstVisibleControlPoint || IsLastVisibleControlPoint;
			}
		}

		public CurvySplineSegment FollowUp
		{
			get
			{
				return m_FollowUp;
			}
			private set
			{
				if (m_FollowUp != value)
				{
					m_FollowUp = value;
					SetDirty();
				}
			}
		}

		public ConnectionHeadingEnum FollowUpHeading
		{
			get
			{
				return m_FollowUpHeading;
			}
			set
			{
				if (m_FollowUpHeading != value)
				{
					m_FollowUpHeading = value;
					SetDirty();
				}
			}
		}

		public bool ConnectionSyncPosition
		{
			get
			{
				return m_ConnectionSyncPosition;
			}
			set
			{
				if (m_ConnectionSyncPosition != value)
				{
					m_ConnectionSyncPosition = value;
				}
			}
		}

		public bool ConnectionSyncRotation
		{
			get
			{
				return m_ConnectionSyncRotation;
			}
			set
			{
				if (m_ConnectionSyncRotation != value)
				{
					m_ConnectionSyncRotation = value;
				}
			}
		}

		public CurvyConnection Connection
		{
			get
			{
				return m_Connection;
			}
			internal set
			{
				if (m_Connection != value)
				{
					m_Connection = value;
				}
				if (m_Connection == null)
				{
					m_FollowUp = null;
				}
			}
		}

		public List<CurvySplineSegment> ConnectedControlPoints
		{
			get
			{
				return (!Connection) ? new List<CurvySplineSegment>() : Connection.OtherControlPoints(this);
			}
		}

		public TTransform TTransform
		{
			get
			{
				return mTTransform;
			}
		}

		public Vector3 localPosition
		{
			get
			{
				return TTransform.localPosition;
			}
			set
			{
				if (TTransform.localPosition != value || TTransform.localPosition == Vector3.zero)
				{
					base.transform.localPosition = value;
					RefreshTransform();
				}
			}
		}

		public Vector3 position
		{
			get
			{
				return TTransform.position;
			}
			set
			{
				if (TTransform.position != value || TTransform.position == Vector3.zero)
				{
					base.transform.position = value;
					TTransform.FromTransform(base.transform);
					Spline.SetDirty(this);
				}
			}
		}

		public Quaternion localRotation
		{
			get
			{
				return TTransform.localRotation;
			}
			set
			{
				if (TTransform.localRotation != value || TTransform.localRotation == Quaternion.identity)
				{
					base.transform.localRotation = value;
					RefreshTransform();
				}
			}
		}

		public Quaternion rotation
		{
			get
			{
				return TTransform.rotation;
			}
			set
			{
				if (TTransform.rotation != value || TTransform.rotation == Quaternion.identity)
				{
					base.transform.rotation = value;
					SetDirty(false);
				}
			}
		}

		public int CacheSize { get; private set; }

		public Bounds Bounds
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

		public float Length { get; private set; }

		public float Distance { get; internal set; }

		public float TF
		{
			get
			{
				return LocalFToTF(0f);
			}
		}

		public bool IsValidSegment
		{
			get
			{
				switch (Spline.Interpolation)
				{
				case CurvyInterpolation.Bezier:
					return NextControlPoint;
				case CurvyInterpolation.Linear:
					return NextControlPoint;
				case CurvyInterpolation.CatmullRom:
				case CurvyInterpolation.TCB:
				{
					CurvySplineSegment nextControlPoint = GetNextControlPoint(false, false);
					return (bool)TTransform && (bool)GetPreviousTTransform(true) && (bool)nextControlPoint && (bool)nextControlPoint.GetNextTTransform(true);
				}
				default:
					return false;
				}
			}
		}

		public bool IsFirstSegment
		{
			get
			{
				return !PreviousSegment || (Spline.Closed && PreviousSegment == Spline[Spline.Count - 1]);
			}
		}

		public bool IsLastSegment
		{
			get
			{
				return !NextSegment || (Spline.Closed && NextSegment == Spline[0]);
			}
		}

		public bool IsFirstControlPoint
		{
			get
			{
				return ControlPointIndex == 0;
			}
		}

		public bool IsFirstVisibleControlPoint
		{
			get
			{
				return SegmentIndex == 0;
			}
		}

		public bool IsLastVisibleControlPoint
		{
			get
			{
				return this == Spline.LastVisibleControlPoint;
			}
		}

		public bool IsVisibleControlPoint
		{
			get
			{
				return SegmentIndex > -1 || IsLastVisibleControlPoint;
			}
		}

		public bool IsLastControlPoint
		{
			get
			{
				return ControlPointIndex == Spline.ControlPointCount - 1;
			}
		}

		public List<Component> MetaData
		{
			get
			{
				if (mMetaData == null)
				{
					ReloadMetaData();
				}
				return mMetaData;
			}
		}

		public CurvySplineSegment NextControlPoint
		{
			get
			{
				return GetNextControlPoint(false, false);
			}
		}

		public CurvySplineSegment PreviousControlPoint
		{
			get
			{
				return GetPreviousControlPoint(false, false);
			}
		}

		public CurvySplineSegment PreviousControlPointWithFollowUp
		{
			get
			{
				CurvySplineSegment curvySplineSegment = PreviousControlPoint;
				if (!curvySplineSegment && IsFirstControlPoint)
				{
					curvySplineSegment = FollowUp;
				}
				return curvySplineSegment;
			}
		}

		public CurvySplineSegment NextControlPointWithFollowUp
		{
			get
			{
				CurvySplineSegment curvySplineSegment = NextControlPoint;
				if (!curvySplineSegment && IsLastControlPoint)
				{
					curvySplineSegment = FollowUp;
				}
				return curvySplineSegment;
			}
		}

		public Transform NextTransform
		{
			get
			{
				return GetNextTransform(false);
			}
		}

		public TTransform NextTTransform
		{
			get
			{
				return GetNextTTransform(false);
			}
		}

		public Transform PreviousTransform
		{
			get
			{
				return GetPreviousTransform(false);
			}
		}

		public TTransform PreviousTTransform
		{
			get
			{
				return GetPreviousTTransform(false);
			}
		}

		public CurvySplineSegment NextSegment
		{
			get
			{
				return GetNextControlPoint(true, false);
			}
		}

		public CurvySplineSegment PreviousSegment
		{
			get
			{
				return GetPreviousControlPoint(true, false);
			}
		}

		public int SegmentIndex
		{
			get
			{
				if (mSegmentIndex == -1)
				{
					mSegmentIndex = Spline.Segments.IndexOf(this);
				}
				return mSegmentIndex;
			}
		}

		public int ControlPointIndex
		{
			get
			{
				if (mControlPointIndex == -1)
				{
					mControlPointIndex = Spline.ControlPoints.IndexOf(this);
				}
				return mControlPointIndex;
			}
			internal set
			{
				mControlPointIndex = value;
			}
		}

		public CurvySpline Spline
		{
			get
			{
				if (mSpline == null && base.transform.parent != null)
				{
					mSpline = base.transform.parent.GetComponent<CurvySpline>();
				}
				return mSpline;
			}
		}

		private CurvyInterpolation interpolation
		{
			get
			{
				return Spline.Interpolation;
			}
		}

		private bool isDynamicOrientation
		{
			get
			{
				return (bool)Spline && Spline.Orientation == CurvyOrientation.Dynamic;
			}
		}

		private bool canHaveSwirl
		{
			get
			{
				return isDynamicOrientation && OrientationAnchor;
			}
		}

		private void OnDrawGizmos()
		{
			if ((bool)Spline && Spline.ShowGizmos)
			{
				doGizmos(false);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if ((bool)Spline)
			{
				doGizmos(true);
			}
		}

		private void Awake()
		{
			mTTransform = new TTransform(base.transform);
		}

		private void OnEnable()
		{
		}

		private void OnDestroy()
		{
			if (true)
			{
				Disconnect();
				if (Spline != null)
				{
					Spline.ControlPoints.Remove(this);
					Spline.setLengthINTERNAL(Spline.Length - Length);
				}
			}
		}

		public void Reset()
		{
			mSegmentIndex = -1;
			mControlPointIndex = -1;
			OrientationAnchor = false;
			Swirl = CurvyOrientationSwirl.None;
			SwirlTurns = 0f;
			m_AutoHandles = true;
			m_AutoHandleDistance = 0.39f;
			HandleIn = new Vector3(-1f, 0f, 0f);
			HandleOut = new Vector3(1f, 0f, 0f);
			SynchronizeTCB = true;
			OverrideGlobalTension = false;
			OverrideGlobalContinuity = false;
			OverrideGlobalBias = false;
			StartTension = 0f;
			EndTension = 0f;
			StartContinuity = 0f;
			EndContinuity = 0f;
			StartBias = 0f;
			EndBias = 0f;
			FollowUp = null;
			FollowUpHeading = ConnectionHeadingEnum.Auto;
			ConnectionSyncPosition = false;
			ConnectionSyncRotation = false;
			if ((bool)Connection)
			{
				Disconnect();
			}
			SetDirty();
		}

		public void SetBezierHandleIn(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
		{
			if (space == Space.Self)
			{
				HandleIn = position;
			}
			else
			{
				HandleInPosition = position;
			}
			bool flag = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
			bool flag2 = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
			bool flag3 = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;
			if (flag)
			{
				HandleOut = HandleOut.magnitude * (HandleIn.normalized * -1f);
			}
			if (flag2)
			{
				HandleOut = HandleIn.magnitude * ((!(HandleOut == Vector3.zero)) ? HandleOut.normalized : (HandleIn.normalized * -1f));
			}
			if (flag3)
			{
				List<CurvySplineSegment> connectedControlPoints = ConnectedControlPoints;
				foreach (CurvySplineSegment item in connectedControlPoints)
				{
					if ((flag || flag2) && item.HandleIn.magnitude == 0f)
					{
						item.HandleIn = HandleIn;
					}
					if (Vector3.Angle(HandleIn, item.HandleIn) > 90f)
					{
						if (flag)
						{
							item.SetBezierHandleOut(item.HandleIn.magnitude * HandleIn.normalized, Space.Self, CurvyBezierModeEnum.Direction);
						}
						if (flag2)
						{
							item.SetBezierHandleOut(item.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
						}
					}
					else
					{
						if (flag)
						{
							item.SetBezierHandleIn(item.HandleIn.magnitude * HandleIn.normalized, Space.Self, CurvyBezierModeEnum.Direction);
						}
						if (flag2)
						{
							item.SetBezierHandleIn(item.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
						}
					}
				}
			}
			CurvySplineSegment previousControlPoint = PreviousControlPoint;
			if ((bool)previousControlPoint)
			{
				previousControlPoint.SetDirty();
			}
		}

		public void SetBezierHandleOut(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
		{
			if (space == Space.Self)
			{
				HandleOut = position;
			}
			else
			{
				HandleOutPosition = position;
			}
			bool flag = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
			bool flag2 = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
			bool flag3 = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;
			if (flag)
			{
				HandleIn = HandleIn.magnitude * (HandleOut.normalized * -1f);
			}
			if (flag2)
			{
				HandleIn = HandleOut.magnitude * ((!(HandleIn == Vector3.zero)) ? HandleIn.normalized : (HandleOut.normalized * -1f));
			}
			if (flag3)
			{
				List<CurvySplineSegment> connectedControlPoints = ConnectedControlPoints;
				foreach (CurvySplineSegment item in connectedControlPoints)
				{
					if ((flag || flag2) && item.HandleOut.magnitude == 0f)
					{
						item.HandleOut = HandleOut;
					}
					if (Vector3.Angle(HandleOut, item.HandleOut) > 90f)
					{
						if (flag)
						{
							item.SetBezierHandleIn(item.HandleOut.magnitude * HandleOut.normalized, Space.Self, CurvyBezierModeEnum.Direction);
						}
						if (flag2)
						{
							item.SetBezierHandleIn(item.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
						}
					}
					else
					{
						if (flag)
						{
							item.SetBezierHandleOut(item.HandleOut.magnitude * HandleOut.normalized, Space.Self, CurvyBezierModeEnum.Direction);
						}
						if (flag2)
						{
							item.SetBezierHandleOut(item.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
						}
					}
				}
			}
			SetDirty();
		}

		public void SetBezierHandles(float distanceFrag = -1f, bool setIn = true, bool setOut = true)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (distanceFrag == -1f)
			{
				distanceFrag = AutoHandleDistance;
			}
			if (distanceFrag > 0f)
			{
				TTransform nextTTransform = NextTTransform;
				TTransform previousTTransform = PreviousTTransform;
				if ((bool)previousTTransform || (bool)nextTTransform)
				{
					Vector3 vector = TTransform.localPosition;
					Vector3 p = ((!previousTTransform) ? (vector - nextTTransform.localPosition) : (previousTTransform.localPosition - vector));
					Vector3 n = ((!nextTTransform) ? (vector - previousTTransform.localPosition) : (nextTTransform.localPosition - vector));
					SetBezierHandles(distanceFrag, p, n, setIn, setOut);
					return;
				}
			}
			if (setIn)
			{
				HandleIn = zero;
				CurvySplineSegment previousControlPoint = PreviousControlPoint;
				if ((bool)previousControlPoint)
				{
					previousControlPoint.SetDirty();
				}
			}
			if (setOut)
			{
				HandleOut = zero2;
				SetDirty();
			}
		}

		public void SetBezierHandles(float distanceFrag, Vector3 p, Vector3 n, bool setIn = true, bool setOut = true)
		{
			float magnitude = p.magnitude;
			float magnitude2 = n.magnitude;
			Vector3 handleIn = Vector3.zero;
			Vector3 handleOut = Vector3.zero;
			if (magnitude != 0f || magnitude2 != 0f)
			{
				Vector3 normalized = (magnitude / magnitude2 * n - p).normalized;
				handleIn = -normalized * (magnitude * distanceFrag);
				handleOut = normalized * (magnitude2 * distanceFrag);
			}
			if (setIn)
			{
				HandleIn = handleIn;
				CurvySplineSegment previousControlPoint = PreviousControlPoint;
				if ((bool)previousControlPoint)
				{
					previousControlPoint.SetDirty();
				}
			}
			if (setOut)
			{
				HandleOut = handleOut;
				SetDirty();
			}
		}

		public void RefreshTransform(bool refreshConnection = true, bool forceCurve = false, bool forceOrientation = false)
		{
			bool flag = TTransform.localPosition != base.transform.localPosition || forceCurve;
			bool flag2 = (Spline.Orientation == CurvyOrientation.Static || OrientationAnchor || forceOrientation) && (TTransform.localRotation != base.transform.localRotation || TTransform.rotation != base.transform.rotation);
			TTransform.FromTransform(base.transform);
			if (refreshConnection && Connection != null && (flag || flag2))
			{
				Connection.SynchronizeINTERNAL(base.transform);
			}
			SetDirty(flag, flag2);
		}

		public void ReloadMetaData()
		{
			mMetaData = new List<Component>();
			GetComponents(typeof(ICurvyMetadata), mMetaData);
		}

		public void ApplyName()
		{
			base.name = "CP" + ControlPointIndex.ToString("D4");
		}

		public bool ConnectTo(CurvySplineSegment targetCP, bool syncPosition = true, bool syncRotation = true, ConnectionHeadingEnum heading = ConnectionHeadingEnum.Auto)
		{
			if (!targetCP)
			{
				Disconnect();
				return false;
			}
			if ((bool)Connection && (bool)targetCP.Connection)
			{
				return false;
			}
			ConnectionSyncPosition = syncPosition;
			ConnectionSyncRotation = syncRotation;
			FollowUpHeading = heading;
			if (ConnectionSyncPosition)
			{
				position = targetCP.position;
			}
			if ((bool)Connection)
			{
				CurvyConnection connection = Connection;
				connection.AddControlPoints(targetCP);
			}
			else if ((bool)targetCP.Connection)
			{
				CurvyConnection connection = targetCP.Connection;
				connection.AddControlPoints(this);
			}
			else
			{
				targetCP.ConnectionSyncPosition = syncPosition;
				targetCP.ConnectionSyncRotation = syncRotation;
				CurvyConnection connection = CurvyConnection.Create(this, targetCP);
			}
			SetDirty();
			if ((bool)targetCP)
			{
				targetCP.SetDirty();
			}
			return true;
		}

		public void SetFollowUp(CurvySplineSegment target, ConnectionHeadingEnum heading = ConnectionHeadingEnum.Auto)
		{
			if (CanHaveFollowUp || target == null)
			{
				FollowUp = target;
				FollowUpHeading = heading;
				SetDirty();
			}
		}

		public void Disconnect()
		{
			if ((bool)Connection)
			{
				Connection.RemoveControlPoint(this);
			}
			Connection = null;
			FollowUp = null;
			FollowUpHeading = ConnectionHeadingEnum.Auto;
			ConnectionSyncPosition = false;
			ConnectionSyncRotation = false;
			SetDirty();
		}

		public void Delete()
		{
			Spline.Delete(this);
		}

		public CurvySplineSegment GetNextControlPoint(bool segmentsOnly, bool useFollowUp)
		{
			CurvySpline spline = Spline;
			if (!spline || spline.ControlPoints.Count == 0)
			{
				return null;
			}
			int num = ControlPointIndex + 1;
			if (num >= spline.ControlPointCount)
			{
				if (spline.Closed)
				{
					return spline.ControlPoints[0];
				}
				if (useFollowUp && FollowUp != null)
				{
					return getFollowUpCP();
				}
				return null;
			}
			if (num < 0)
			{
				return null;
			}
			return (!segmentsOnly || spline.ControlPoints[num].SegmentIndex != -1) ? spline.ControlPoints[num] : null;
		}

		public CurvySplineSegment GetPreviousControlPoint(bool segmentsOnly, bool useFollowUp)
		{
			CurvySpline spline = Spline;
			if (!spline || spline.ControlPoints.Count == 0)
			{
				return null;
			}
			int num = ControlPointIndex - 1;
			if (num < 0)
			{
				if (spline.Closed)
				{
					return spline.ControlPoints[spline.ControlPointCount - 1];
				}
				if (useFollowUp && FollowUp != null)
				{
					return getFollowUpCP();
				}
				return null;
			}
			if (num >= spline.ControlPointCount)
			{
				return null;
			}
			return (!segmentsOnly || spline.ControlPoints[num].SegmentIndex != -1) ? spline.ControlPoints[num] : null;
		}

		public Transform GetNextTransform(bool useFollowUp)
		{
			CurvySplineSegment nextControlPoint = GetNextControlPoint(false, useFollowUp);
			if ((bool)nextControlPoint)
			{
				return nextControlPoint.transform;
			}
			return (!Spline.AutoEndTangents) ? null : base.transform;
		}

		public TTransform GetNextTTransform(bool useFollowUp)
		{
			CurvySplineSegment nextControlPoint = GetNextControlPoint(false, useFollowUp);
			if ((bool)nextControlPoint)
			{
				return nextControlPoint.TTransform;
			}
			return (!Spline.AutoEndTangents) ? null : TTransform;
		}

		public Transform GetPreviousTransform(bool useFollowUp)
		{
			CurvySplineSegment previousControlPoint = GetPreviousControlPoint(false, useFollowUp);
			if ((bool)previousControlPoint)
			{
				return previousControlPoint.transform;
			}
			return (!Spline.AutoEndTangents) ? null : base.transform;
		}

		public TTransform GetPreviousTTransform(bool useFollowUp)
		{
			CurvySplineSegment previousControlPoint = GetPreviousControlPoint(false, useFollowUp);
			if ((bool)previousControlPoint)
			{
				return previousControlPoint.TTransform;
			}
			return (!Spline.AutoEndTangents) ? null : TTransform;
		}

		public void SetAsFirstCP()
		{
			if (ControlPointIndex > 0)
			{
				CurvySplineSegment[] array = new CurvySplineSegment[ControlPointIndex];
				for (int i = 0; i < ControlPointIndex; i++)
				{
					array[i] = Spline.ControlPoints[i];
				}
				CurvySplineSegment[] array2 = array;
				foreach (CurvySplineSegment item in array2)
				{
					Spline.ControlPoints.Remove(item);
					Spline.ControlPoints.Add(item);
				}
				Spline.SetDirtyAll();
				Spline.SyncHierarchyFromSpline();
				Spline.Refresh();
			}
		}

		public Vector3 GetPreviousPosition()
		{
			CurvySplineSegment previousControlPoint = PreviousControlPoint;
			if ((bool)previousControlPoint)
			{
				return previousControlPoint.TTransform.localPosition;
			}
			if (FollowUp != null)
			{
				ConnectionHeadingEnum connectionHeadingEnum = FollowUpHeading;
				if (connectionHeadingEnum == ConnectionHeadingEnum.Auto)
				{
					connectionHeadingEnum = ((!FollowUp.PreviousControlPoint) ? ConnectionHeadingEnum.Plus : ((!FollowUp.NextControlPoint) ? ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Sharp));
				}
				switch (connectionHeadingEnum)
				{
				case ConnectionHeadingEnum.Minus:
				{
					Transform previousTransform = FollowUp.PreviousTransform;
					if ((bool)previousTransform)
					{
						return previousTransform.localPosition;
					}
					break;
				}
				case ConnectionHeadingEnum.Plus:
				{
					Transform nextTransform = FollowUp.NextTransform;
					if ((bool)nextTransform)
					{
						return nextTransform.localPosition;
					}
					break;
				}
				}
			}
			if ((bool)PreviousTTransform)
			{
				return PreviousTTransform.localPosition;
			}
			return TTransform.localPosition;
		}

		public Vector3 GetNextPosition()
		{
			CurvySplineSegment nextControlPoint = NextControlPoint;
			if ((bool)nextControlPoint)
			{
				return nextControlPoint.TTransform.localPosition;
			}
			if (FollowUp != null)
			{
				ConnectionHeadingEnum connectionHeadingEnum = FollowUpHeading;
				if (connectionHeadingEnum == ConnectionHeadingEnum.Auto)
				{
					connectionHeadingEnum = ((!FollowUp.PreviousControlPoint) ? ConnectionHeadingEnum.Plus : ((!FollowUp.NextControlPoint) ? ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Sharp));
				}
				switch (connectionHeadingEnum)
				{
				case ConnectionHeadingEnum.Minus:
				{
					TTransform previousTTransform = FollowUp.PreviousTTransform;
					if ((bool)previousTTransform)
					{
						return previousTTransform.localPosition;
					}
					break;
				}
				case ConnectionHeadingEnum.Plus:
				{
					TTransform nextTTransform = FollowUp.NextTTransform;
					if ((bool)nextTTransform)
					{
						return nextTTransform.localPosition;
					}
					break;
				}
				}
			}
			if ((bool)NextTTransform)
			{
				return NextTTransform.localPosition;
			}
			return TTransform.localPosition;
		}

		public Vector3 Interpolate(float localF)
		{
			return Interpolate(localF, Spline.Interpolation);
		}

		public Vector3 Interpolate(float localF, CurvyInterpolation interpolation)
		{
			switch (interpolation)
			{
			case CurvyInterpolation.Bezier:
				return interpolateBezier(localF);
			case CurvyInterpolation.CatmullRom:
				return interpolateCatmull(localF);
			case CurvyInterpolation.TCB:
				return interpolateTCB(localF);
			default:
				return interpolateLinear(localF);
			}
		}

		public Vector3 InterpolateFast(float localF)
		{
			float frag;
			int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
			int num = Mathf.Min(Approximation.Length - 1, approximationIndexINTERNAL + 1);
			return Vector3.Lerp(Approximation[approximationIndexINTERNAL], Approximation[num], frag);
		}

		public Component GetMetaData(Type type, bool autoCreate = false)
		{
			List<Component> metaData = MetaData;
			if (metaData != null && type.IsSubclassOf(typeof(Component)) && typeof(ICurvyMetadata).IsAssignableFrom(type))
			{
				for (int i = 0; i < metaData.Count; i++)
				{
					if (metaData[i] != null && metaData[i].GetType() == type)
					{
						return metaData[i];
					}
				}
			}
			Component component = null;
			if (autoCreate)
			{
				component = base.gameObject.AddComponent(type);
				MetaData.Add(component);
			}
			return component;
		}

		public T GetMetadata<T>(bool autoCreate = false) where T : Component, ICurvyMetadata
		{
			return (T)GetMetaData(typeof(T), autoCreate);
		}

		public U InterpolateMetadata<T, U>(float f) where T : Component, ICurvyInterpolatableMetadata<U>
		{
			T metadata = GetMetadata<T>();
			if (metadata != null)
			{
				CurvySplineSegment nextControlPoint = GetNextControlPoint(false, true);
				ICurvyInterpolatableMetadata<U> b = null;
				if ((bool)nextControlPoint)
				{
					b = nextControlPoint.GetMetadata<T>();
				}
				return metadata.Interpolate(b, f);
			}
			return default(U);
		}

		public object InterpolateMetadata(Type type, float f)
		{
			ICurvyInterpolatableMetadata curvyInterpolatableMetadata = GetMetaData(type) as ICurvyInterpolatableMetadata;
			if (curvyInterpolatableMetadata != null)
			{
				CurvySplineSegment nextControlPoint = GetNextControlPoint(false, true);
				ICurvyInterpolatableMetadata curvyInterpolatableMetadata2 = null;
				if ((bool)nextControlPoint)
				{
					curvyInterpolatableMetadata2 = nextControlPoint.GetMetaData(type) as ICurvyInterpolatableMetadata;
					if (curvyInterpolatableMetadata2 != null)
					{
						return curvyInterpolatableMetadata.InterpolateObject(curvyInterpolatableMetadata2, f);
					}
				}
			}
			return null;
		}

		public void DeleteMetadata()
		{
			List<Component> metaData = MetaData;
			for (int num = metaData.Count - 1; num >= 0; num--)
			{
				metaData[num].Destroy();
			}
		}

		public Vector3 InterpolateScale(float localF)
		{
			Transform nextTransform = NextTransform;
			return (!nextTransform) ? base.transform.lossyScale : Vector3.Lerp(base.transform.lossyScale, nextTransform.lossyScale, localF);
		}

		public Vector3 GetTangent(float localF)
		{
			localF = Mathf.Clamp01(localF);
			Vector3 vector = Interpolate(localF);
			return GetTangent(localF, ref vector);
		}

		public Vector3 GetTangent(float localF, ref Vector3 position)
		{
			int num = 2;
			Vector3 vector;
			do
			{
				float num2 = localF + 0.01f;
				if (num2 > 1f)
				{
					CurvySplineSegment nextSegment = NextSegment;
					if (!nextSegment)
					{
						num2 = localF - 0.01f;
						return (position - Interpolate(num2)).normalized;
					}
					vector = nextSegment.Interpolate(num2 - 1f);
				}
				else
				{
					vector = Interpolate(num2);
				}
				localF += 0.01f;
			}
			while (vector == position && --num > 0);
			return (vector - position).normalized;
		}

		public Vector3 GetTangentFast(float localF)
		{
			float frag;
			int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
			int num = Mathf.Min(ApproximationT.Length - 1, approximationIndexINTERNAL + 1);
			return Vector3.Lerp(ApproximationT[approximationIndexINTERNAL], ApproximationT[num], frag);
		}

		public Quaternion GetOrientationFast(float localF)
		{
			return GetOrientationFast(localF, false);
		}

		public Quaternion GetOrientationFast(float localF, bool inverse)
		{
			Vector3 tangentFast = GetTangentFast(localF);
			if (tangentFast != Vector3.zero)
			{
				if (inverse)
				{
					tangentFast *= -1f;
				}
				return Quaternion.LookRotation(tangentFast, GetOrientationUpFast(localF));
			}
			return Quaternion.identity;
		}

		public Vector3 GetOrientationUpFast(float localF)
		{
			float frag;
			int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
			approximationIndexINTERNAL = Mathf.Max(0, Mathf.Min(approximationIndexINTERNAL, ApproximationUp.Length - 2));
			int num = Mathf.Min(ApproximationUp.Length - 1, approximationIndexINTERNAL + 1);
			return Vector3.Lerp(ApproximationUp[approximationIndexINTERNAL], ApproximationUp[num], frag);
		}

		public float GetNearestPointF(Vector3 p)
		{
			int num = CacheSize + 1;
			float num2 = float.MaxValue;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				float sqrMagnitude = (Approximation[i] - p).sqrMagnitude;
				if (sqrMagnitude <= num2)
				{
					num2 = sqrMagnitude;
					num3 = i;
				}
			}
			int num4 = ((num3 <= 0) ? (-1) : (num3 - 1));
			int num5 = ((num3 >= CacheSize) ? (-1) : (num3 + 1));
			float frag = 0f;
			float frag2 = 0f;
			float num6 = float.MaxValue;
			float num7 = float.MaxValue;
			if (num4 > -1)
			{
				num6 = DTMath.LinePointDistanceSqr(Approximation[num4], Approximation[num3], p, out frag);
			}
			if (num5 > -1)
			{
				num7 = DTMath.LinePointDistanceSqr(Approximation[num3], Approximation[num5], p, out frag2);
			}
			if (num6 < num7)
			{
				return getApproximationLocalF(num4) + frag * mStepSize;
			}
			return getApproximationLocalF(num3) + frag2 * mStepSize;
		}

		public float DistanceToLocalF(float localDistance)
		{
			localDistance = Mathf.Clamp(localDistance, 0f, Length);
			if (ApproximationDistances.Length == 0 || localDistance == 0f)
			{
				return 0f;
			}
			if (Mathf.Approximately(localDistance, Length))
			{
				return 1f;
			}
			int num = Mathf.Min(ApproximationDistances.Length - 1, mCacheLastDistanceToLocalFIndex);
			if (ApproximationDistances[num] < localDistance)
			{
				num = ApproximationDistances.Length - 1;
			}
			while (ApproximationDistances[num] > localDistance)
			{
				num--;
			}
			mCacheLastDistanceToLocalFIndex = num + 1;
			float num2 = (localDistance - ApproximationDistances[num]) / (ApproximationDistances[num + 1] - ApproximationDistances[num]);
			float approximationLocalF = getApproximationLocalF(num);
			float approximationLocalF2 = getApproximationLocalF(num + 1);
			return approximationLocalF + (approximationLocalF2 - approximationLocalF) * num2;
		}

		public float LocalFToDistance(float localF)
		{
			localF = Mathf.Clamp01(localF);
			float frag;
			int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
			if (ApproximationDistances.Length > 0)
			{
				if (approximationIndexINTERNAL < 0 || approximationIndexINTERNAL >= ApproximationDistances.Length - 1)
				{
					Debug.Log("localF/idx/Arraysize = " + localF + "/" + approximationIndexINTERNAL + "/" + ApproximationDistances.Length);
				}
				float num = ApproximationDistances[approximationIndexINTERNAL + 1] - ApproximationDistances[approximationIndexINTERNAL];
				return ApproximationDistances[approximationIndexINTERNAL] + num * frag;
			}
			return 0f;
		}

		public float LocalFToTF(float localF)
		{
			return Spline.SegmentToTF(this, localF);
		}

		public bool SnapToFitSplineLength(float newSplineLength, float stepSize)
		{
			if (stepSize == 0f || Mathf.Approximately(newSplineLength, Spline.Length))
			{
				return true;
			}
			float length = Spline.Length;
			Vector3 vector = base.transform.position;
			Vector3 vector2 = base.transform.up * stepSize;
			base.transform.position += vector2;
			SetDirty();
			Spline.Refresh();
			bool flag = Spline.Length > length;
			int num = 30000;
			base.transform.position = vector;
			if (newSplineLength > length)
			{
				if (!flag)
				{
					vector2 *= -1f;
				}
				while (Spline.Length < newSplineLength)
				{
					num--;
					length = Spline.Length;
					base.transform.position += vector2;
					SetDirty();
					Spline.Refresh();
					if (length > Spline.Length)
					{
						return false;
					}
					if (num == 0)
					{
						Debug.LogError("CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please send a bug report!");
						return false;
					}
				}
			}
			else
			{
				if (flag)
				{
					vector2 *= -1f;
				}
				while (Spline.Length > newSplineLength)
				{
					num--;
					length = Spline.Length;
					base.transform.position += vector2;
					SetDirty();
					Spline.Refresh();
					if (length < Spline.Length)
					{
						return false;
					}
					if (num == 0)
					{
						Debug.LogError("CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please send a bug report!");
						return false;
					}
				}
			}
			return true;
		}

		public void SetDirty(bool dirtyCurve = true, bool dirtyOrientation = true)
		{
			if (!Spline || (!dirtyCurve && !dirtyOrientation))
			{
				return;
			}
			CurvySplineSegment previousControlPoint = GetPreviousControlPoint(false, true);
			switch (Spline.Interpolation)
			{
			case CurvyInterpolation.Linear:
				if ((bool)previousControlPoint)
				{
					Spline.SetDirty(previousControlPoint, dirtyCurve, dirtyOrientation);
				}
				Spline.SetDirty(this, dirtyCurve, dirtyOrientation);
				break;
			case CurvyInterpolation.Bezier:
			{
				if ((bool)previousControlPoint)
				{
					Spline.SetDirty(previousControlPoint, dirtyCurve, dirtyOrientation);
				}
				Spline.SetDirty(this, dirtyCurve, dirtyOrientation);
				CurvySplineSegment nextControlPoint2 = GetNextControlPoint(false, true);
				if ((bool)nextControlPoint2)
				{
					Spline.SetDirty(nextControlPoint2, dirtyCurve, dirtyOrientation);
				}
				break;
			}
			case CurvyInterpolation.CatmullRom:
			case CurvyInterpolation.TCB:
			{
				if ((bool)previousControlPoint)
				{
					Spline.SetDirty(previousControlPoint, dirtyCurve, dirtyOrientation);
					Spline.SetDirty(previousControlPoint.GetPreviousControlPoint(true, true), dirtyCurve, dirtyOrientation);
				}
				Spline.SetDirty(this, dirtyCurve, dirtyOrientation);
				CurvySplineSegment nextControlPoint = GetNextControlPoint(false, true);
				if ((bool)nextControlPoint)
				{
					Spline.SetDirty(nextControlPoint, dirtyCurve, dirtyOrientation);
					if ((bool)nextControlPoint.FollowUp)
					{
						Spline.SetDirty(nextControlPoint.FollowUp, dirtyCurve, dirtyOrientation);
					}
				}
				break;
			}
			}
		}

		public override string ToString()
		{
			if (Spline != null)
			{
				return Spline.name + "." + base.name;
			}
			return base.ToString();
		}

		public void BakeOrientation(bool setDirty = true)
		{
			if (!Spline || !Spline.IsInitialized)
			{
				return;
			}
			base.transform.localRotation = GetOrientationFast(0f);
			if (setDirty)
			{
				RefreshTransform();
				return;
			}
			bool flag = TTransform.localPosition != base.transform.localPosition;
			bool flag2 = (Spline.Orientation == CurvyOrientation.Static || OrientationAnchor) && (TTransform.localRotation != base.transform.localRotation || TTransform.rotation != base.transform.rotation);
			TTransform.FromTransform(base.transform);
			if (Connection != null && (flag || flag2))
			{
				Connection.SynchronizeINTERNAL(base.transform);
			}
		}

		public CurvySpline SplitSpline()
		{
			CurvySpline curvySpline = CurvySpline.Create(Spline);
			curvySpline.transform.SetParent(Spline.transform.parent, true);
			curvySpline.name = Spline.name + "_parted";
			List<CurvySplineSegment> range = Spline.ControlPoints.GetRange(ControlPointIndex, Spline.ControlPointCount - ControlPointIndex);
			for (int i = 0; i < range.Count; i++)
			{
				if (Application.isPlaying)
				{
					range[i].transform.SetParent(curvySpline.transform, true);
				}
				range[i].reSettleINTERNAL();
				curvySpline.ControlPoints.Add(range[i]);
			}
			Spline.SetDirtyAll();
			Spline.SyncHierarchyFromSpline();
			curvySpline.SetDirtyAll();
			curvySpline.SyncHierarchyFromSpline();
			Spline.Refresh();
			curvySpline.Refresh();
			return curvySpline;
		}

		private CurvySplineSegment getFollowUpCP()
		{
			ConnectionHeadingEnum connectionHeadingEnum = FollowUpHeading;
			if (connectionHeadingEnum == ConnectionHeadingEnum.Auto)
			{
				connectionHeadingEnum = (FollowUp.IsFirstVisibleControlPoint ? ConnectionHeadingEnum.Plus : (FollowUp.IsLastVisibleControlPoint ? ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Sharp));
			}
			switch (connectionHeadingEnum)
			{
			case ConnectionHeadingEnum.Minus:
				return FollowUp.GetPreviousControlPoint(false, false);
			case ConnectionHeadingEnum.Plus:
				return FollowUp.GetNextControlPoint(false, false);
			default:
				return FollowUp;
			}
		}

		private Transform getNextNextTransform(bool withFollowUp)
		{
			CurvySplineSegment nextControlPoint = GetNextControlPoint(false, withFollowUp);
			return (!nextControlPoint) ? base.transform : nextControlPoint.GetNextTransform(withFollowUp);
		}

		private TTransform getNextNextTTransform(bool withFollowUp)
		{
			CurvySplineSegment nextControlPoint = GetNextControlPoint(false, withFollowUp);
			return (!nextControlPoint) ? TTransform : nextControlPoint.GetNextTTransform(withFollowUp);
		}

		private float getApproximationLocalF(int idx)
		{
			return (float)idx * mStepSize;
		}

		public int getApproximationIndexINTERNAL(float localF, out float frag)
		{
			localF = Mathf.Clamp01(localF);
			if (localF == 1f)
			{
				frag = 1f;
				return Mathf.Max(0, Approximation.Length - 2);
			}
			float num = localF / mStepSize;
			int num2 = (int)num;
			frag = num - (float)num2;
			return num2;
		}

		private Vector3 interpolateLinear(float localF)
		{
			localF = Mathf.Clamp01(localF);
			return Vector3.Lerp(TTransform.localPosition, GetNextTTransform(true).localPosition, localF);
		}

		private Vector3 interpolateBezier(float localF)
		{
			localF = Mathf.Clamp01(localF);
			CurvySplineSegment nextControlPoint = GetNextControlPoint(false, true);
			return CurvySpline.Bezier(TTransform.localPosition + HandleOut, TTransform.localPosition, nextControlPoint.TTransform.localPosition, nextControlPoint.TTransform.localPosition + nextControlPoint.HandleIn, localF);
		}

		private Vector3 interpolateCatmull(float localF)
		{
			localF = Mathf.Clamp01(localF);
			return CurvySpline.CatmullRom(GetPreviousTTransform(true).localPosition, TTransform.localPosition, GetNextTTransform(true).localPosition, getNextNextTTransform(true).localPosition, localF);
		}

		private Vector3 interpolateTCB(float localF)
		{
			localF = Mathf.Clamp01(localF);
			float fT = StartTension;
			float fT2 = EndTension;
			float fC = StartContinuity;
			float fC2 = EndContinuity;
			float fB = StartBias;
			float fB2 = EndBias;
			if (!OverrideGlobalTension)
			{
				fT = (fT2 = Spline.Tension);
			}
			if (!OverrideGlobalContinuity)
			{
				fC = (fC2 = Spline.Continuity);
			}
			if (!OverrideGlobalBias)
			{
				fB = (fB2 = Spline.Bias);
			}
			return CurvySpline.TCB(GetPreviousTTransform(true).localPosition, TTransform.localPosition, GetNextTTransform(true).localPosition, getNextNextTTransform(true).localPosition, localF, fT, fC, fB, fT2, fC2, fB2);
		}

		internal void refreshCurveINTERNAL()
		{
			bool isValidSegment = IsValidSegment;
			TTransform nextTTransform = NextTTransform;
			if (isValidSegment)
			{
				CacheSize = CurvySpline.CalculateCacheSize(Spline.CacheDensity, (nextTTransform.position - TTransform.position).magnitude, (Spline is CurvyUISpline) ? 1 : 0);
			}
			else
			{
				CacheSize = 0;
			}
			Array.Resize(ref Approximation, CacheSize + 1);
			Array.Resize(ref ApproximationT, CacheSize + 1);
			Array.Resize(ref ApproximationDistances, CacheSize + 1);
			if (Spline.Orientation == CurvyOrientation.None)
			{
				Array.Resize(ref ApproximationUp, 2);
			}
			else
			{
				Array.Resize(ref ApproximationUp, CacheSize + 1);
			}
			mBounds = null;
			Length = 0f;
			mStepSize = 1f / (float)CacheSize;
			Approximation[0] = TTransform.localPosition;
			Approximation[CacheSize] = ((!nextTTransform) ? Approximation[0] : nextTTransform.localPosition);
			if (!isValidSegment)
			{
				return;
			}
			Vector3 vector;
			switch (Spline.Interpolation)
			{
			case CurvyInterpolation.Bezier:
			{
				for (int l = 1; l < CacheSize; l++)
				{
					Approximation[l] = interpolateBezier((float)l * mStepSize);
					vector = Approximation[l] - Approximation[l - 1];
					Length += vector.magnitude;
					ApproximationDistances[l] = Length;
					ApproximationT[l - 1] = vector.normalized;
				}
				break;
			}
			case CurvyInterpolation.CatmullRom:
			{
				for (int j = 1; j < CacheSize; j++)
				{
					Approximation[j] = interpolateCatmull((float)j * mStepSize);
					vector = Approximation[j] - Approximation[j - 1];
					Length += vector.magnitude;
					ApproximationDistances[j] = Length;
					ApproximationT[j - 1] = vector.normalized;
				}
				break;
			}
			case CurvyInterpolation.TCB:
			{
				for (int k = 1; k < CacheSize; k++)
				{
					Approximation[k] = interpolateTCB((float)k * mStepSize);
					vector = Approximation[k] - Approximation[k - 1];
					Length += vector.magnitude;
					ApproximationDistances[k] = Length;
					ApproximationT[k - 1] = vector.normalized;
				}
				break;
			}
			default:
			{
				for (int i = 1; i < CacheSize; i++)
				{
					Approximation[i] = interpolateLinear((float)i * mStepSize);
					vector = Approximation[i] - Approximation[i - 1];
					Length += vector.magnitude;
					ApproximationDistances[i] = Length;
					ApproximationT[i - 1] = vector.normalized;
				}
				break;
			}
			}
			vector = Approximation[CacheSize] - Approximation[CacheSize - 1];
			Length += vector.magnitude;
			ApproximationDistances[CacheSize] = Length;
			ApproximationT[CacheSize - 1] = vector.normalized;
		}

		internal void refreshOrientationStaticINTERNAL()
		{
			ApproximationUp[0] = getOrthoUp0INTERNAL();
			if (Approximation.Length > 1)
			{
				ApproximationUp[CacheSize] = getOrthoUp1INTERNAL();
				for (int i = 1; i < CacheSize; i++)
				{
					ApproximationUp[i] = Vector3.Lerp(ApproximationUp[0], ApproximationUp[CacheSize], (float)i / (float)CacheSize);
				}
			}
			if (AutoBakeOrientation)
			{
				BakeOrientation(false);
			}
		}

		internal void refreshOrientationPTFINTERNAL(ref Vector3 lastUpVector)
		{
			ApproximationUp[0] = lastUpVector;
			if (Approximation.Length > 1)
			{
				int num = CacheSize + 1;
				for (int i = 1; i < num; i++)
				{
					lastUpVector = DTMath.ParallelTransportFrame(ref lastUpVector, ref ApproximationT[i - 1], ref ApproximationT[i]);
					ApproximationUp[i] = lastUpVector;
				}
			}
			if (AutoBakeOrientation)
			{
				BakeOrientation(false);
			}
		}

		internal void smoothOrientationINTERNAL(ref Vector3 lastUpVector, ref float angleaccu, float angle)
		{
			ApproximationUp[0] = lastUpVector;
			int num = ApproximationUp.Length;
			for (int i = 1; i < num; i++)
			{
				ApproximationUp[i] = Quaternion.AngleAxis(angleaccu, ApproximationT[i]) * ApproximationUp[i];
				angleaccu += angle;
			}
			lastUpVector = ApproximationUp[ApproximationUp.Length - 1];
			if (AutoBakeOrientation)
			{
				BakeOrientation(false);
			}
		}

		private Bounds getBounds()
		{
			if (Approximation.Length == 0)
			{
				return new Bounds(TTransform.position, Vector3.zero);
			}
			Matrix4x4 localToWorldMatrix = Spline.transform.localToWorldMatrix;
			Bounds result = new Bounds(localToWorldMatrix.MultiplyPoint3x4(Approximation[0]), Vector3.zero);
			int num = Approximation.Length;
			for (int i = 1; i < num; i++)
			{
				result.Encapsulate(localToWorldMatrix.MultiplyPoint(Approximation[i]));
			}
			return result;
		}

		internal void ClearBoundsINTERNAL()
		{
			mBounds = null;
		}

		internal Vector3 getOrthoUp0INTERNAL()
		{
			Vector3 tangent = Quaternion.Inverse(Spline.TTransform.rotation) * TTransform.up;
			Vector3.OrthoNormalize(ref ApproximationT[0], ref tangent);
			return tangent;
		}

		internal Vector3 getOrthoUp1INTERNAL()
		{
			Vector3 tangent = Quaternion.Inverse(Spline.TTransform.rotation) * NextTTransform.up;
			Vector3.OrthoNormalize(ref ApproximationT[CacheSize], ref tangent);
			return tangent;
		}

		internal void reSettleINTERNAL(bool removeFromCollection = true)
		{
			if (removeFromCollection)
			{
				Spline.ControlPoints.Remove(this);
			}
			else
			{
				SetDirty();
			}
			mSpline = null;
			mControlPointIndex = -1;
			mSegmentIndex = -1;
		}

		private void doGizmos(bool selected)
		{
			if (CurvyGlobalManager.Gizmos == CurvySplineGizmos.None)
			{
				return;
			}
			Camera current = Camera.current;
			bool flag = current != null;
			float value = 0f;
			if (flag)
			{
				if (!current.BoundsInView(Bounds))
				{
					return;
				}
				value = (current.transform.position - Bounds.ClosestPoint(current.transform.position)).magnitude;
			}
			bool flag2 = (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Curve) == CurvySplineGizmos.Curve;
			CurvyGizmoHelper.Matrix = Spline.transform.localToWorldMatrix;
			if ((bool)Connection)
			{
				CurvyGizmoHelper.ConnectionGizmo(this);
			}
			if (flag2)
			{
				CurvyGizmoHelper.ControlPointGizmo(this, selected, (!selected) ? Spline.GizmoColor : Spline.GizmoSelectionColor);
			}
			if (!IsValidSegment)
			{
				return;
			}
			if (flag2)
			{
				float num = 20f;
				if (flag)
				{
					float num2 = Mathf.Clamp(value, 1f, 3000f) / 3000f;
					num2 = ((!(num2 < 0.01f)) ? DTTween.QuintOut(num2, 0f, 1f) : DTTween.SineOut(num2, 0f, 1f));
					num = Mathf.Clamp(Length * CurvyGlobalManager.SceneViewResolution * 0.1f / num2, 1f, 10000f);
				}
				CurvyGizmoHelper.SegmentCurveGizmo(this, (!selected) ? Spline.GizmoColor : Spline.GizmoSelectionColor, 1f / num);
			}
			if (Approximation.Length > 0 && (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Approximation) == CurvySplineGizmos.Approximation)
			{
				CurvyGizmoHelper.SegmentApproximationGizmo(this, Spline.GizmoColor * 0.8f);
			}
			if (Spline.Orientation != 0 && ApproximationUp.Length > 0 && (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Orientation) == CurvySplineGizmos.Orientation)
			{
				CurvyGizmoHelper.SegmentOrientationGizmo(this, CurvyGlobalManager.GizmoOrientationColor);
				if (OrientationAnchor && Spline.Orientation == CurvyOrientation.Dynamic)
				{
					CurvyGizmoHelper.SegmentOrientationAnchorGizmo(this, CurvyGlobalManager.GizmoOrientationColor);
				}
			}
			if (ApproximationT.Length > 0 && (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Tangents) == CurvySplineGizmos.Tangents)
			{
				CurvyGizmoHelper.SegmentTangentGizmo(this, new Color(0f, 0.7f, 0f));
			}
		}

		internal void ClearSegmentIndexINTERNAL()
		{
			mSegmentIndex = -1;
		}

		public void OnBeforePush()
		{
			Reset();
			DeleteMetadata();
			mSpline = null;
		}

		public void OnAfterPop()
		{
		}

		int IComparable.CompareTo(object obj)
		{
			return ControlPointIndex.CompareTo(((CurvySplineSegment)obj).ControlPointIndex);
		}
	}
}
