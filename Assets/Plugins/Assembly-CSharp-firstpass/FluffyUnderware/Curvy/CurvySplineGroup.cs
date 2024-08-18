using System;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	public class CurvySplineGroup : CurvySplineBase
	{
		[SerializeField]
		[ArrayEx(ShowAdd = false)]
		private List<CurvySpline> m_Splines = new List<CurvySpline>();

		private bool mIsContinuous;

		private bool mIsClosed;

		private bool mDirty;

		private Bounds? mBounds;

		public List<CurvySpline> Splines
		{
			get
			{
				return m_Splines;
			}
			set
			{
				m_Splines = Splines;
				SetDirtyAll();
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

		public float[] Distances { get; private set; }

		public override int Count
		{
			get
			{
				return Splines.Count;
			}
		}

		public CurvySpline this[int idx]
		{
			get
			{
				return (idx <= -1 || idx >= Splines.Count) ? null : Splines[idx];
			}
		}

		public override bool IsContinuous
		{
			get
			{
				return mIsContinuous;
			}
		}

		public override bool IsClosed
		{
			get
			{
				return mIsContinuous && mIsClosed;
			}
		}

		public override bool Dirty
		{
			get
			{
				return mDirty;
			}
		}

		private IEnumerator Start()
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i] != null)
				{
					while (!this[i].IsInitialized)
					{
						yield return new WaitForEndOfFrame();
					}
				}
			}
			Refresh();
		}

		protected override void OnEnable()
		{
			if (!Application.isPlaying)
			{
				SetDirtyAll();
			}
		}

		private void OnDisable()
		{
			mIsInitialized = false;
			for (int i = 0; i < Count; i++)
			{
				if (this[i] != null)
				{
					this[i].OnRefresh.RemoveListener(OnSplineRefresh);
				}
			}
		}

		private void Update()
		{
			if (Dirty)
			{
				Refresh();
			}
		}

		public void Reset()
		{
			Clear();
		}

		public static CurvySplineGroup Create(params CurvySpline[] splines)
		{
			CurvySplineGroup component = new GameObject("Curvy Spline Group", typeof(CurvySplineGroup)).GetComponent<CurvySplineGroup>();
			component.Add(splines);
			return component;
		}

		public override void SetDirtyAll()
		{
			mDirty = true;
		}

		public override Vector3 Interpolate(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.Interpolate(localTF);
		}

		public override Vector3 InterpolateFast(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.InterpolateFast(localTF);
		}

		public override Vector3 InterpolateUserValue(float tf, int index)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.InterpolateUserValue(localTF, index);
		}

		public override Vector3 InterpolateScale(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.InterpolateScale(localTF);
		}

		public override Vector3 GetOrientationUpFast(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.GetOrientationUpFast(localTF);
		}

		public override Quaternion GetOrientationFast(float tf, bool inverse)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Quaternion.identity : curvySpline.GetOrientationFast(localTF, inverse);
		}

		public override Vector3 GetTangent(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.GetTangent(localTF);
		}

		public override Vector3 GetTangent(float tf, Vector3 position)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.GetTangent(localTF, position);
		}

		public override Vector3 GetTangentFast(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? Vector3.zero : curvySpline.GetTangentFast(localTF);
		}

		public override Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
		{
			return MoveByLengthFast(ref tf, ref direction, distance, clamping);
		}

		public override Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
		{
			return MoveByLengthFast(ref tf, ref direction, distance, clamping);
		}

		public override float TFToDistance(float tf, CurvyClamping clamping)
		{
			if (Count == 0)
			{
				return 0f;
			}
			float localTF;
			int num = TFToSplineIndex(tf, out localTF);
			return Distances[num] + this[num].TFToDistance(localTF);
		}

		public CurvySpline TFToSpline(float tf)
		{
			float localTF;
			int num = TFToSplineIndex(tf, out localTF);
			return (num != -1) ? this[num] : null;
		}

		public CurvySpline TFToSpline(float tf, out float localTF)
		{
			int num = TFToSplineIndex(tf, out localTF);
			return (num != -1) ? this[num] : null;
		}

		public float SplineToTF(CurvySpline spline, float splineTF)
		{
			if (Count == 0)
			{
				return 0f;
			}
			return (float)Splines.IndexOf(spline) / (float)Count + 1f / (float)Count * splineTF;
		}

		public override CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
		{
			localF = 0f;
			float localTF;
			int num = TFToSplineIndex(tf, out localTF, clamping);
			return (num != -1) ? this[num].TFToSegment(localTF, out localF) : null;
		}

		public override float SegmentToTF(CurvySplineSegment segment)
		{
			return SplineToTF(segment.Spline, 0f);
		}

		public override float SegmentToTF(CurvySplineSegment segment, float localF)
		{
			float splineTF = segment.LocalFToTF(localF);
			return SplineToTF(segment.Spline, splineTF);
		}

		public override float DistanceToTF(float distance, CurvyClamping clamping)
		{
			float localDistance;
			CurvySpline curvySpline = DistanceToSpline(distance, out localDistance, clamping);
			return (!curvySpline) ? 0f : SplineToTF(curvySpline, curvySpline.DistanceToTF(localDistance));
		}

		public CurvySpline DistanceToSpline(float distance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			float localDistance;
			return DistanceToSpline(distance, out localDistance, clamping);
		}

		public CurvySpline DistanceToSpline(float distance, out float localDistance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			distance = CurvyUtility.ClampDistance(distance, clamping, base.Length);
			localDistance = 0f;
			for (int i = 1; i < Count; i++)
			{
				if (Distances[i] >= distance)
				{
					localDistance = distance - Distances[i - 1];
					return this[i - 1];
				}
			}
			localDistance = distance - Distances[Count - 1];
			return this[Count - 1];
		}

		public override Vector3[] GetApproximation(Space space = Space.Self)
		{
			List<Vector3[]> list = new List<Vector3[]>(Count);
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				Vector3[] array = this[i].GetApproximation(space);
				if (NextSplineConnected(i))
				{
					Array.Resize(ref array, array.Length - 1);
				}
				list.Add(array);
				num += list[i].Length;
			}
			Vector3[] array2 = new Vector3[num];
			num = 0;
			for (int j = 0; j < Count; j++)
			{
				list[j].CopyTo(array2, num);
				num += list[j].Length;
			}
			return array2;
		}

		public override Vector3[] GetApproximationT()
		{
			List<Vector3[]> list = new List<Vector3[]>(Count);
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				Vector3[] array = this[i].GetApproximationT();
				if (NextSplineConnected(i))
				{
					Array.Resize(ref array, array.Length - 1);
				}
				list.Add(array);
				num += list[i].Length;
			}
			Vector3[] array2 = new Vector3[num];
			num = 0;
			for (int j = 0; j < Count; j++)
			{
				list[j].CopyTo(array2, num);
				num += list[j].Length;
			}
			return array2;
		}

		public override Vector3[] GetApproximationUpVectors()
		{
			List<Vector3[]> list = new List<Vector3[]>(Count);
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				Vector3[] array = this[i].GetApproximationUpVectors();
				if (NextSplineConnected(i))
				{
					Array.Resize(ref array, array.Length - 1);
				}
				list.Add(array);
				num += list[i].Length;
			}
			Vector3[] array2 = new Vector3[num];
			num = 0;
			for (int j = 0; j < Count; j++)
			{
				list[j].CopyTo(array2, num);
				num += list[j].Length;
			}
			return array2;
		}

		public override float GetNearestPointTF(Vector3 p)
		{
			Vector3 nearest;
			return GetNearestPointTF(p, out nearest);
		}

		public override float GetNearestPointTF(Vector3 p, out Vector3 nearest)
		{
			nearest = Vector3.zero;
			float splineTF = -1f;
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < Count; i++)
			{
				Vector3 nearest2;
				float nearestPointTF = this[i].GetNearestPointTF(p, out nearest2);
				if (nearestPointTF > -1f)
				{
					float sqrMagnitude = (p - this[i].Interpolate(nearestPointTF)).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num2 = i;
						splineTF = nearestPointTF;
						nearest = nearest2;
						num = sqrMagnitude;
					}
				}
			}
			return (num2 <= -1) ? (-1f) : SplineToTF(this[num2], splineTF);
		}

		public override Component GetMetadata(Type type, float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? null : curvySpline.GetMetadata(type, localTF);
		}

		public override U InterpolateMetadata<T, U>(float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? default(U) : curvySpline.InterpolateMetadata<T, U>(localTF);
		}

		public override object InterpolateMetadata(Type type, float tf)
		{
			float localTF;
			CurvySpline curvySpline = TFToSpline(tf, out localTF);
			return (!curvySpline) ? null : curvySpline.InterpolateMetadata(type, localTF);
		}

		public void Add(params CurvySpline[] splines)
		{
			Splines.AddRange(splines);
			SetDirtyAll();
		}

		public void Delete(CurvySpline spline)
		{
			Splines.Remove(spline);
			SetDirtyAll();
		}

		public override void Clear()
		{
			Splines.Clear();
			SetDirtyAll();
		}

		public override void Refresh()
		{
			RemoveEmptySplines();
			for (int i = 0; i < Count; i++)
			{
				if (!this[i].IsInitialized)
				{
					return;
				}
			}
			mIsInitialized = Count > 0;
			if (mIsInitialized)
			{
				for (int j = 0; j < Count; j++)
				{
					this[j].OnRefresh.AddListenerOnce(OnSplineRefresh);
				}
				doRefreshLength();
				doGetProperties();
				OnRefreshEvent(new CurvySplineEventArgs(this));
			}
		}

		public void RemoveEmptySplines()
		{
			if (Splines.Count <= 0)
			{
				return;
			}
			for (int num = Splines.Count - 1; num > -1; num--)
			{
				if (Splines[num] == null)
				{
					Splines.RemoveAt(num);
				}
			}
		}

		private Bounds getBounds()
		{
			Bounds result = default(Bounds);
			for (int i = 0; i < Count; i++)
			{
				result.Encapsulate(this[i].Bounds);
			}
			return result;
		}

		private void doRefreshLength()
		{
			mLength = 0f;
			Distances = new float[Count];
			for (int i = 0; i < Count; i++)
			{
				Distances[i] = mLength;
				mLength += this[i].Length;
			}
		}

		private bool NextSplineConnected(int idx)
		{
			idx = Mathf.Clamp(idx, 0, Count - 1);
			int idx2 = ((idx != Count - 1) ? (idx + 1) : 0);
			CurvySpline nextSpline = this[idx].NextSpline;
			return (bool)nextSpline && nextSpline == this[idx2];
		}

		private void OnSplineRefresh(CurvySplineEventArgs e)
		{
			if (!Splines.Contains(e.Spline))
			{
				e.Spline.OnRefresh.RemoveListener(OnSplineRefresh);
			}
			else if (mIsInitialized)
			{
				doRefreshLength();
				doGetProperties();
				OnRefreshEvent(new CurvySplineEventArgs(this));
			}
		}

		private void doGetProperties()
		{
			mIsContinuous = true;
			for (int i = 0; i < Count - 2; i++)
			{
				if (!NextSplineConnected(i))
				{
					mIsContinuous = false;
					break;
				}
			}
			mIsClosed = Count > 1 && NextSplineConnected(Count - 1);
		}

		private int TFToSplineIndex(float tf, out float localTF, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			tf = CurvyUtility.ClampTF(tf, clamping);
			localTF = 0f;
			if (Count == 0)
			{
				return -1;
			}
			float num = tf * (float)Count;
			int num2 = (int)num;
			localTF = num - (float)num2;
			if (num2 == Count)
			{
				num2--;
				localTF = 1f;
			}
			return num2;
		}
	}
}
