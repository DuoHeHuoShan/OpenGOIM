using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[DTVersion("2.1.2")]
	public class CurvySplineBase : DTVersionedMonoBehaviour
	{
		[Group("Events", Expanded = false, Sort = 1000, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/curvyspline_events")]
		[SortOrder(0)]
		[SerializeField]
		private CurvySplineEvent m_OnRefresh = new CurvySplineEvent();

		[Group("Events", Sort = 1000)]
		[SortOrder(1)]
		[SerializeField]
		private CurvySplineEvent m_OnAfterControlPointChanges = new CurvySplineEvent();

		[Group("Events", Sort = 1000)]
		[SortOrder(2)]
		[SerializeField]
		private CurvyControlPointEvent m_OnBeforeControlPointAdd = new CurvyControlPointEvent();

		[Group("Events", Sort = 1000)]
		[SortOrder(3)]
		[SerializeField]
		private CurvyControlPointEvent m_OnAfterControlPointAdd = new CurvyControlPointEvent();

		[Group("Events", Sort = 1000)]
		[SortOrder(4)]
		[SerializeField]
		private CurvyControlPointEvent m_OnBeforeControlPointDelete = new CurvyControlPointEvent();

		[SerializeField]
		[HideInInspector]
		private CurvySplineMoveEvent m_OnMoveControlPointReached = new CurvySplineMoveEvent();

		[SerializeField]
		[HideInInspector]
		private CurvySplineMoveEvent m_OnMoveEndReached = new CurvySplineMoveEvent();

		[HideInInspector]
		public bool ShowGizmos = true;

		protected float mLength;

		private TTransform mTTransform;

		private Bounds mBounds;

		protected bool mIsInitialized;

		public CurvySplineEvent OnRefresh
		{
			get
			{
				return m_OnRefresh;
			}
			set
			{
				if (m_OnRefresh != value)
				{
					m_OnRefresh = value;
				}
			}
		}

		public CurvySplineEvent OnAfterControlPointChanges
		{
			get
			{
				return m_OnAfterControlPointChanges;
			}
			set
			{
				if (m_OnAfterControlPointChanges != value)
				{
					m_OnAfterControlPointChanges = value;
				}
			}
		}

		public CurvyControlPointEvent OnBeforeControlPointAdd
		{
			get
			{
				return m_OnBeforeControlPointAdd;
			}
			set
			{
				if (m_OnBeforeControlPointAdd != value)
				{
					m_OnBeforeControlPointAdd = value;
				}
			}
		}

		public CurvyControlPointEvent OnAfterControlPointAdd
		{
			get
			{
				return m_OnAfterControlPointAdd;
			}
			set
			{
				if (m_OnAfterControlPointAdd != value)
				{
					m_OnAfterControlPointAdd = value;
				}
			}
		}

		public CurvyControlPointEvent OnBeforeControlPointDelete
		{
			get
			{
				return m_OnBeforeControlPointDelete;
			}
			set
			{
				if (m_OnBeforeControlPointDelete != value)
				{
					m_OnBeforeControlPointDelete = value;
				}
			}
		}

		public CurvySplineMoveEvent OnMoveControlPointReached
		{
			get
			{
				return m_OnMoveControlPointReached;
			}
			set
			{
				if (m_OnMoveControlPointReached != value)
				{
					m_OnMoveControlPointReached = value;
				}
			}
		}

		public CurvySplineMoveEvent OnMoveEndReached
		{
			get
			{
				return m_OnMoveEndReached;
			}
			set
			{
				if (m_OnMoveEndReached != value)
				{
					m_OnMoveEndReached = value;
				}
			}
		}

		public TTransform TTransform
		{
			get
			{
				return mTTransform;
			}
		}

		public bool IsInitialized
		{
			get
			{
				return mIsInitialized;
			}
		}

		public float Length
		{
			get
			{
				return mLength;
			}
		}

		public virtual bool IsContinuous
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsClosed
		{
			get
			{
				return false;
			}
		}

		public virtual bool Dirty
		{
			get
			{
				return false;
			}
		}

		public virtual int Count
		{
			get
			{
				return 0;
			}
		}

		public virtual Bounds Bounds
		{
			get
			{
				return default(Bounds);
			}
		}

		protected CurvySplineEventArgs OnRefreshEvent(CurvySplineEventArgs e)
		{
			if (OnRefresh != null)
			{
				OnRefresh.Invoke(e);
			}
			return e;
		}

		protected CurvyControlPointEventArgs OnBeforeControlPointAddEvent(CurvyControlPointEventArgs e)
		{
			if (OnBeforeControlPointAdd != null)
			{
				OnBeforeControlPointAdd.Invoke(e);
			}
			return e;
		}

		protected CurvyControlPointEventArgs OnAfterControlPointAddEvent(CurvyControlPointEventArgs e)
		{
			if (OnAfterControlPointAdd != null)
			{
				OnAfterControlPointAdd.Invoke(e);
			}
			return e;
		}

		protected CurvyControlPointEventArgs OnBeforeControlPointDeleteEvent(CurvyControlPointEventArgs e)
		{
			if (OnBeforeControlPointDelete != null)
			{
				OnBeforeControlPointDelete.Invoke(e);
			}
			return e;
		}

		protected CurvySplineEventArgs OnAfterControlPointChangesEvent(CurvySplineEventArgs e)
		{
			if (OnAfterControlPointChanges != null)
			{
				OnAfterControlPointChanges.Invoke(e);
			}
			return e;
		}

		protected CurvySplineMoveEventArgs OnMoveControlPointReachedEvent(CurvySplineMoveEventArgs e)
		{
			if (OnMoveControlPointReached != null)
			{
				OnMoveControlPointReached.Invoke(e);
			}
			if (OnMoveEndReached != null && (e.ControlPoint.IsFirstVisibleControlPoint || e.ControlPoint.IsLastVisibleControlPoint))
			{
				OnMoveEndReached.Invoke(e);
			}
			return e;
		}

		protected virtual void Awake()
		{
			mTTransform = new TTransform(base.transform);
		}

		protected virtual void OnEnable()
		{
		}

		public virtual Vector3 Interpolate(float tf)
		{
			return Vector3.zero;
		}

		public virtual Vector3 Interpolate(float tf, CurvyInterpolation interpolation)
		{
			return Vector3.zero;
		}

		public virtual Vector3 InterpolateFast(float tf)
		{
			return Vector3.zero;
		}

		public virtual Vector3 InterpolateUserValue(float tf, int index)
		{
			return Vector3.zero;
		}

		public T GetMetadata<T>(float tf) where T : Component, ICurvyMetadata
		{
			return (T)GetMetadata(typeof(T), tf);
		}

		public virtual Component GetMetadata(Type type, float tf)
		{
			return null;
		}

		public virtual U InterpolateMetadata<T, U>(float tf) where T : Component, ICurvyInterpolatableMetadata<U>
		{
			return default(U);
		}

		public virtual object InterpolateMetadata(Type type, float tf)
		{
			return null;
		}

		public virtual Vector3 InterpolateScale(float tf)
		{
			return Vector3.zero;
		}

		public virtual Vector3 GetOrientationUpFast(float tf)
		{
			return Vector3.zero;
		}

		public virtual Quaternion GetOrientationFast(float tf)
		{
			return GetOrientationFast(tf, false);
		}

		public virtual Quaternion GetOrientationFast(float tf, bool inverse)
		{
			return Quaternion.identity;
		}

		public virtual Vector3 GetTangent(float tf)
		{
			return Vector3.zero;
		}

		public virtual Vector3 GetTangent(float tf, Vector3 position)
		{
			return Vector3.zero;
		}

		public virtual Vector3 GetTangentFast(float tf)
		{
			return Vector3.zero;
		}

		public virtual float TFToDistance(float tf, CurvyClamping clamping)
		{
			return 0f;
		}

		public float TFToDistance(float tf)
		{
			return TFToDistance(tf, CurvyClamping.Clamp);
		}

		public virtual float DistanceToTF(float distance, CurvyClamping clamping)
		{
			return 0f;
		}

		public float DistanceToTF(float distance)
		{
			return DistanceToTF(distance, CurvyClamping.Clamp);
		}

		public virtual CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
		{
			localF = 0f;
			return null;
		}

		public CurvySplineSegment TFToSegment(float tf, CurvyClamping clamping)
		{
			float localF;
			return TFToSegment(tf, out localF, clamping);
		}

		public CurvySplineSegment TFToSegment(float tf)
		{
			float localF;
			return TFToSegment(tf, out localF, CurvyClamping.Clamp);
		}

		public CurvySplineSegment TFToSegment(float tf, out float localF)
		{
			return TFToSegment(tf, out localF, CurvyClamping.Clamp);
		}

		public virtual float SegmentToTF(CurvySplineSegment segment)
		{
			return SegmentToTF(segment, 0f);
		}

		public virtual float SegmentToTF(CurvySplineSegment segment, float localF)
		{
			return 0f;
		}

		public virtual float GetNearestPointTF(Vector3 p)
		{
			return 0f;
		}

		public virtual float GetNearestPointTF(Vector3 p, out Vector3 nearest)
		{
			nearest = Vector3.zero;
			return 0f;
		}

		public virtual void Clear()
		{
		}

		public virtual Vector3[] GetApproximation(Space space = Space.Self)
		{
			return new Vector3[0];
		}

		public virtual Vector3[] GetApproximationT()
		{
			return new Vector3[0];
		}

		public virtual Vector3[] GetApproximationUpVectors()
		{
			return new Vector3[0];
		}

		public Vector3[] GetPolygonByAngle(float angle, float minDistance)
		{
			if (Mathf.Approximately(angle, 0f))
			{
				Debug.LogError("CurvySplineBase.GetPolygonByAngle: angle must be greater than 0!");
				return new Vector3[0];
			}
			List<Vector3> list = new List<Vector3>();
			float tf = 0f;
			int direction = 1;
			float num = minDistance * minDistance;
			list.Add(Interpolate(0f));
			while (tf < 1f)
			{
				Vector3 vector = MoveByAngle(ref tf, ref direction, angle, CurvyClamping.Clamp);
				if ((vector - list[list.Count - 1]).sqrMagnitude >= num)
				{
					list.Add(vector);
				}
			}
			return list.ToArray();
		}

		public Vector3[] GetPolygon(float fromTF, float toTF, float maxAngle, float minDistance, float maxDistance, out List<float> vertexTF, out List<Vector3> vertexTangents, bool includeEndPoint = true, float stepSize = 0.01f)
		{
			stepSize = Mathf.Clamp(stepSize, 0.002f, 1f);
			maxDistance = ((maxDistance != -1f) ? Mathf.Clamp(maxDistance, 0f, Length) : Length);
			minDistance = Mathf.Clamp(minDistance, 0f, maxDistance);
			if (!IsClosed)
			{
				toTF = Mathf.Clamp01(toTF);
				fromTF = Mathf.Clamp(fromTF, 0f, toTF);
			}
			List<Vector3> vPos = new List<Vector3>();
			List<Vector3> vTan = new List<Vector3>();
			List<float> vTF = new List<float>();
			int linearSteps = 0;
			float angleFromLast = 0f;
			float distAccu = 0f;
			Vector3 curPos = Interpolate(fromTF);
			Vector3 curTangent = GetTangent(fromTF);
			Vector3 vector = curPos;
			Vector3 vector2 = curTangent;
			Action<float> action = delegate(float f)
			{
				vPos.Add(curPos);
				vTan.Add(curTangent);
				vTF.Add(f);
				angleFromLast = 0f;
				distAccu = 0f;
				linearSteps = 0;
			};
			action(fromTF);
			float num = fromTF + stepSize;
			while (num < toTF)
			{
				float num2 = num % 1f;
				curPos = Interpolate(num2);
				curTangent = GetTangent(num2);
				if (curTangent == Vector3.zero)
				{
					Debug.Log("zero Tangent! Oh no!");
				}
				distAccu += (curPos - vector).magnitude;
				if (curTangent == vector2)
				{
					linearSteps++;
				}
				if (distAccu >= minDistance)
				{
					if (distAccu >= maxDistance)
					{
						action(num2);
					}
					else
					{
						angleFromLast += Vector3.Angle(vector2, curTangent);
						if (angleFromLast >= maxAngle || (linearSteps > 0 && angleFromLast > 0f))
						{
							action(num2);
						}
					}
				}
				num += stepSize;
				vector = curPos;
				vector2 = curTangent;
			}
			if (includeEndPoint)
			{
				vTF.Add(toTF % 1f);
				curPos = Interpolate(toTF % 1f);
				vPos.Add(curPos);
				vTan.Add(GetTangent(toTF % 1f, curPos));
			}
			vertexTF = vTF;
			vertexTangents = vTan;
			return vPos.ToArray();
		}

		public Vector3[] GetApproximationPoints(float fromTF, float toTF, bool includeEndPoint = true)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(fromTF, out localF);
			float frag;
			int num = curvySplineSegment.getApproximationIndexINTERNAL(localF, out frag);
			float localF2;
			CurvySplineSegment curvySplineSegment2 = TFToSegment(toTF, out localF2);
			float frag2;
			int approximationIndexINTERNAL = curvySplineSegment2.getApproximationIndexINTERNAL(localF2, out frag2);
			CurvySplineSegment curvySplineSegment3 = curvySplineSegment;
			Vector3[] array = new Vector3[1] { Vector3.Lerp(curvySplineSegment3.Approximation[num], curvySplineSegment3.Approximation[num + 1], frag) };
			while ((bool)curvySplineSegment3 && curvySplineSegment3 != curvySplineSegment2)
			{
				array = array.AddRange(curvySplineSegment3.Approximation.SubArray(num + 1, curvySplineSegment3.Approximation.Length - 1));
				num = 1;
				curvySplineSegment3 = curvySplineSegment3.NextSegment;
			}
			if ((bool)curvySplineSegment3)
			{
				int num2 = ((!(curvySplineSegment == curvySplineSegment3)) ? 1 : (num + 1));
				array = array.AddRange(curvySplineSegment3.Approximation.SubArray(num2, approximationIndexINTERNAL - num2));
				if (includeEndPoint && (frag2 > 0f || frag2 < 1f))
				{
					return array.Add(Vector3.Lerp(curvySplineSegment3.Approximation[approximationIndexINTERNAL], curvySplineSegment3.Approximation[approximationIndexINTERNAL + 1], frag2));
				}
			}
			return array;
		}

		public virtual Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
		{
			tf = CurvyUtility.ClampTF(tf + fDistance * (float)direction, ref direction, clamping);
			return Interpolate(tf);
		}

		public virtual Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
		{
			tf = CurvyUtility.ClampTF(tf + fDistance * (float)direction, ref direction, clamping);
			return InterpolateFast(tf);
		}

		public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping)
		{
			return MoveBy(ref tf, ref direction, distance, clamping, 0.002f);
		}

		public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
		{
			return Move(ref tf, ref direction, ExtrapolateDistanceToTF(tf, distance, stepSize), clamping);
		}

		public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
		{
			return MoveByFast(ref tf, ref direction, distance, clamping, 0.002f);
		}

		public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
		{
			return MoveFast(ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping);
		}

		public virtual Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
		{
			float distance2 = ClampDistance(TFToDistance(tf) + distance * (float)direction, ref direction, clamping);
			tf = DistanceToTF(distance2);
			return InterpolateFast(tf);
		}

		public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping)
		{
			return MoveByAngle(ref tf, ref direction, angle, clamping, 0.002f);
		}

		public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
		{
			if (clamping == CurvyClamping.PingPong)
			{
				Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
				return Vector3.zero;
			}
			stepSize = Mathf.Max(0.0001f, stepSize);
			float num = tf;
			Vector3 vector = Interpolate(tf);
			Vector3 tangent = GetTangent(tf, vector);
			Vector3 vector2 = Vector3.zero;
			int num2 = 10000;
			while (num2-- > 0)
			{
				tf += stepSize * (float)direction;
				if (tf > 1f)
				{
					if (clamping != CurvyClamping.Loop)
					{
						tf = 1f;
						return Interpolate(1f);
					}
					tf -= 1f;
				}
				else if (tf < 0f)
				{
					if (clamping != CurvyClamping.Loop)
					{
						tf = 0f;
						return Interpolate(0f);
					}
					tf += 1f;
				}
				vector2 = Interpolate(tf);
				Vector3 to = vector2 - vector;
				float num3 = Vector3.Angle(tangent, to);
				if (num3 >= angle)
				{
					tf = num + (tf - num) * angle / num3;
					return Interpolate(tf);
				}
			}
			return vector2;
		}

		public virtual Vector3 MoveByAngleFast(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
		{
			if (clamping == CurvyClamping.PingPong)
			{
				Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
				return Vector3.zero;
			}
			stepSize = Mathf.Max(0.0001f, stepSize);
			float num = tf;
			Vector3 vector = InterpolateFast(tf);
			Vector3 tangentFast = GetTangentFast(tf);
			Vector3 vector2 = Vector3.zero;
			int num2 = 10000;
			while (num2-- > 0)
			{
				tf += stepSize * (float)direction;
				if (tf > 1f)
				{
					if (clamping != CurvyClamping.Loop)
					{
						tf = 1f;
						return InterpolateFast(1f);
					}
					tf -= 1f;
				}
				else if (tf < 0f)
				{
					if (clamping != CurvyClamping.Loop)
					{
						tf = 0f;
						return InterpolateFast(0f);
					}
					tf += 1f;
				}
				vector2 = InterpolateFast(tf);
				Vector3 to = vector2 - vector;
				float num3 = Vector3.Angle(tangentFast, to);
				if (num3 >= angle)
				{
					tf = num + (tf - num) * angle / num3;
					return InterpolateFast(tf);
				}
			}
			return vector2;
		}

		public virtual Vector3 GetExtrusionPoint(float tf, float radius, float angle)
		{
			Vector3 vector = Interpolate(tf);
			Vector3 tangent = GetTangent(tf, vector);
			Quaternion quaternion = Quaternion.AngleAxis(angle, tangent);
			return vector + quaternion * GetOrientationUpFast(tf) * radius;
		}

		public virtual Vector3 GetExtrusionPointFast(float tf, float radius, float angle)
		{
			Vector3 vector = InterpolateFast(tf);
			Vector3 tangentFast = GetTangentFast(tf);
			Quaternion quaternion = Quaternion.AngleAxis(angle, tangentFast);
			return vector + quaternion * GetOrientationUpFast(tf) * radius;
		}

		public Vector3 GetRotatedUp(float tf, float angle)
		{
			Vector3 tangent = GetTangent(tf);
			Quaternion quaternion = Quaternion.AngleAxis(angle, tangent);
			return quaternion * GetOrientationUpFast(tf);
		}

		public Vector3 GetRotatedUpFast(float tf, float angle)
		{
			Vector3 tangentFast = GetTangentFast(tf);
			Quaternion quaternion = Quaternion.AngleAxis(angle, tangentFast);
			return quaternion * GetOrientationUpFast(tf);
		}

		public virtual Vector3 GetTangentByDistance(float distance)
		{
			return GetTangent(DistanceToTF(distance));
		}

		public virtual Vector3 GetTangentByDistanceFast(float distance)
		{
			return GetTangentFast(DistanceToTF(distance));
		}

		public virtual Vector3 InterpolateByDistance(float distance)
		{
			return Interpolate(DistanceToTF(distance));
		}

		public virtual Vector3 InterpolateByDistanceFast(float distance)
		{
			return InterpolateFast(DistanceToTF(distance));
		}

		public float ExtrapolateDistanceToTF(float tf, float distance, float stepSize)
		{
			stepSize = Mathf.Max(0.0001f, stepSize);
			Vector3 vector = Interpolate(tf);
			float num = ((tf != 1f) ? Mathf.Min(1f, tf + stepSize) : Mathf.Min(1f, tf - stepSize));
			stepSize = Mathf.Abs(num - tf);
			Vector3 vector2 = Interpolate(num);
			float magnitude = (vector2 - vector).magnitude;
			if (magnitude != 0f)
			{
				return 1f / magnitude * stepSize * distance;
			}
			return 0f;
		}

		public float ExtrapolateDistanceToTFFast(float tf, float distance, float stepSize)
		{
			stepSize = Mathf.Max(0.0001f, stepSize);
			Vector3 vector = InterpolateFast(tf);
			float num = ((tf != 1f) ? Mathf.Min(1f, tf + stepSize) : Mathf.Min(1f, tf - stepSize));
			stepSize = Mathf.Abs(num - tf);
			Vector3 vector2 = InterpolateFast(num);
			float magnitude = (vector2 - vector).magnitude;
			if (magnitude != 0f)
			{
				return 1f / magnitude * stepSize * distance;
			}
			return 0f;
		}

		public void Destroy()
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
			}
		}

		public virtual void Refresh()
		{
		}

		public virtual void SetDirtyAll()
		{
		}

		public float ClampDistance(float distance, CurvyClamping clamping)
		{
			return CurvyUtility.ClampDistance(distance, clamping, Length);
		}

		public float ClampDistance(float distance, CurvyClamping clamping, float min, float max)
		{
			return CurvyUtility.ClampDistance(distance, clamping, Length, min, max);
		}

		public float ClampDistance(float distance, ref int dir, CurvyClamping clamping)
		{
			return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length);
		}

		public float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float min, float max)
		{
			return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length, min, max);
		}
	}
}
