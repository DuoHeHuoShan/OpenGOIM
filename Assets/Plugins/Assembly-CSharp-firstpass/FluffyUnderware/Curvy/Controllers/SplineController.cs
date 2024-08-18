using System.Collections;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[AddComponentMenu("Curvy/Controller/Spline Controller", 5)]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/splinecontroller")]
	public class SplineController : CurvyController
	{
		[Section("General", true, false, 100, Sort = 0)]
		[FieldCondition("m_Spline", null, false, ActionAttribute.ActionEnum.ShowWarning, "Missing Source", ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private CurvySpline m_Spline;

		[SerializeField]
		private bool m_UseCache;

		[Section("Events", false, false, 1000, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/splinecontroller_events")]
		[SerializeField]
		private CurvySplineMoveEvent m_OnControlPointReached;

		[SerializeField]
		private CurvySplineMoveEvent m_OnEndReached;

		[SerializeField]
		private CurvySplineMoveEvent m_OnSwitch;

		private static CurvyController _active;

		private CurvySpline mInitialSpline;

		public float mKeepDistanceAt;

		private float mSwitchStartTime;

		private float mSwitchDuration;

		private CurvySplineMoveEventArgs mSwitchEventArgs;

		public CurvySpline Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					if (m_Spline != null)
					{
						UnbindEvents();
					}
					m_Spline = value;
					if ((bool)m_Spline)
					{
						BindEvents();
					}
				}
			}
		}

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
			}
		}

		public override bool AdaptOnChange
		{
			get
			{
				return base.AdaptOnChange;
			}
			set
			{
				if (base.AdaptOnChange != value)
				{
					base.AdaptOnChange = value;
					BindEvents();
				}
			}
		}

		public CurvySplineMoveEvent OnControlPointReached
		{
			get
			{
				return m_OnControlPointReached;
			}
			set
			{
				if (m_OnControlPointReached != value)
				{
					m_OnControlPointReached = value;
				}
				if ((bool)Spline)
				{
					BindEvents();
				}
			}
		}

		public CurvySplineMoveEvent OnEndReached
		{
			get
			{
				return m_OnEndReached;
			}
			set
			{
				if (m_OnEndReached != value)
				{
					m_OnEndReached = value;
				}
				if ((bool)Spline)
				{
					BindEvents();
				}
			}
		}

		public CurvySplineMoveEvent OnSwitch
		{
			get
			{
				return m_OnSwitch;
			}
			set
			{
				if (m_OnSwitch != value)
				{
					m_OnSwitch = value;
				}
			}
		}

		public bool IsSwitching
		{
			get
			{
				return mSwitchEventArgs != null;
			}
		}

		public override bool IsConfigured
		{
			get
			{
				return Spline != null;
			}
		}

		public override bool DependenciesInitialized
		{
			get
			{
				return Spline.IsInitialized;
			}
		}

		public override float Length
		{
			get
			{
				return (!Spline) ? 0f : Spline.Length;
			}
		}

		public override float RelativePosition
		{
			get
			{
				return base.RelativePosition;
			}
			set
			{
				base.RelativePosition = value;
				mKeepDistanceAt = 0f;
			}
		}

		public override float AbsolutePosition
		{
			get
			{
				return base.AbsolutePosition;
			}
			set
			{
				base.AbsolutePosition = value;
				mKeepDistanceAt = 0f;
			}
		}

		public override float Position
		{
			get
			{
				return base.Position;
			}
			set
			{
				base.Position = value;
				mKeepDistanceAt = 0f;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			BindEvents();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			UnbindEvents();
		}

		private IEnumerator Start()
		{
			if (IsConfigured)
			{
				while (!DependenciesInitialized)
				{
					yield return 0;
				}
				Prepare();
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Spline = null;
		}

		public override void Prepare()
		{
			BindEvents();
			base.Prepare();
			mKeepDistanceAt = 0f;
		}

		public override void Refresh()
		{
			if (IsSwitching)
			{
				mSwitchEventArgs.Delta = Mathf.Clamp01((Time.time - mSwitchStartTime) / mSwitchDuration);
				if (OnSwitch.HasListeners())
				{
					onSwitchEvent(mSwitchEventArgs);
				}
				else
				{
					mSwitchEventArgs.Data = mSwitchEventArgs.Delta;
				}
			}
			base.Refresh();
			if (IsSwitching)
			{
				Vector3 position;
				Vector3 tangent;
				Vector3 up;
				getInterpolatedSourcePosition(mSwitchEventArgs.Spline, mSwitchEventArgs.TF, out position, out tangent, out up);
				if (base.Space == Space.Self)
				{
					Transform.localPosition = Vector3.Lerp(Transform.localPosition, position, (float)mSwitchEventArgs.Data);
				}
				else
				{
					Transform.position = Vector3.Lerp(Transform.localPosition, position, (float)mSwitchEventArgs.Data);
				}
				if (base.OrientationMode != 0)
				{
				}
				if (mSwitchEventArgs.Delta == 1f)
				{
					Spline = mSwitchEventArgs.Spline;
					RelativePosition = mSwitchEventArgs.TF;
					mSwitchEventArgs = null;
				}
			}
		}

		public virtual void SwitchTo(CurvySpline target, float targetTF, float duration)
		{
			mSwitchStartTime = Time.time;
			mSwitchDuration = duration;
			mSwitchEventArgs = new CurvySplineMoveEventArgs(this, target, null, targetTF, 0f, base.Direction);
			mSwitchEventArgs.Data = 0f;
		}

		public override void BeginPreview()
		{
			mInitialSpline = Spline;
			base.BeginPreview();
		}

		public override void EndPreview()
		{
			Spline = mInitialSpline;
			base.EndPreview();
		}

		protected override float RelativeToAbsolute(float relativeDistance)
		{
			return (!(Length > 0f)) ? 0f : Spline.TFToDistance(relativeDistance, base.Clamping);
		}

		protected override float AbsoluteToRelative(float worldUnitDistance)
		{
			return (!(Length > 0f)) ? 0f : Spline.DistanceToTF(worldUnitDistance, base.Clamping);
		}

		protected override Vector3 GetInterpolatedSourcePosition(float tf)
		{
			Vector3 vector = Spline.transform.localPosition;
			if (Spline.Length > 0f)
			{
				vector = ((!UseCache) ? Spline.Interpolate(tf) : Spline.InterpolateFast(tf));
			}
			return (base.Space != 0) ? vector : Spline.transform.TransformPoint(vector);
		}

		protected override void GetInterpolatedSourcePosition(float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
		{
			getInterpolatedSourcePosition(Spline, tf, out position, out tangent, out up);
		}

		protected override Vector3 GetTangent(float tf)
		{
			if (Spline.Length == 0f)
			{
				return Vector3.forward;
			}
			Vector3 vector = ((!UseCache) ? Spline.GetTangent(tf) : Spline.GetTangentFast(tf));
			return (base.Space != 0) ? vector : Spline.transform.TransformDirection(vector);
		}

		protected override Vector3 GetOrientation(float tf)
		{
			if (Spline.Length == 0f)
			{
				return Vector3.zero;
			}
			return (base.Space != 0) ? Spline.GetOrientationUpFast(tf) : Spline.transform.TransformDirection(Spline.GetOrientationUpFast(tf));
		}

		protected override void Advance(ref float tf, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
		{
			_active = this;
			switch (mode)
			{
			case MoveModeEnum.AbsoluteExtrapolate:
				if (UseCache)
				{
					Spline.MoveByFast(ref tf, ref direction, absSpeed, base.Clamping);
					if (IsSwitching)
					{
						mSwitchEventArgs.Spline.MoveByFast(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, base.Clamping);
						onSwitchEvent(mSwitchEventArgs);
					}
				}
				else
				{
					Spline.MoveBy(ref tf, ref direction, absSpeed, base.Clamping);
					if (IsSwitching)
					{
						mSwitchEventArgs.Spline.MoveBy(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, base.Clamping);
						onSwitchEvent(mSwitchEventArgs);
					}
				}
				break;
			case MoveModeEnum.AbsolutePrecise:
				Spline.MoveByLengthFast(ref tf, ref direction, absSpeed, base.Clamping);
				if (IsSwitching)
				{
					mSwitchEventArgs.Spline.MoveByLengthFast(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, base.Clamping);
					onSwitchEvent(mSwitchEventArgs);
				}
				break;
			default:
				if (UseCache)
				{
					Spline.MoveFast(ref tf, ref direction, absSpeed, base.Clamping);
					if (IsSwitching)
					{
						mSwitchEventArgs.Spline.MoveFast(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, base.Clamping);
						onSwitchEvent(mSwitchEventArgs);
					}
				}
				else
				{
					Spline.Move(ref tf, ref direction, absSpeed, base.Clamping);
					if (IsSwitching)
					{
						mSwitchEventArgs.Spline.Move(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, base.Clamping);
						onSwitchEvent(mSwitchEventArgs);
					}
				}
				break;
			}
			_active = null;
		}

		protected override void BindEvents()
		{
			base.BindEvents();
			if ((bool)Spline)
			{
				UnbindEvents();
				m_OnControlPointReached.CheckForListeners();
				m_OnEndReached.CheckForListeners();
				if (m_OnControlPointReached != null && m_OnControlPointReached.HasListeners())
				{
					Spline.OnMoveControlPointReached.AddListenerOnce(onControlPointReachedEvent);
				}
				if (m_OnEndReached != null && m_OnEndReached.HasListeners())
				{
					Spline.OnMoveEndReached.AddListenerOnce(onEndReachedEvent);
				}
				Spline.OnRefresh.AddListener(OnRefreshSpline);
				if (AdaptOnChange)
				{
					Spline.OnBeforeControlPointAdd.AddListener(onBeforeCPChange);
					Spline.OnBeforeControlPointDelete.AddListener(onBeforeCPChange);
				}
			}
		}

		protected override void UnbindEvents()
		{
			if ((bool)Spline)
			{
				Spline.OnRefresh.RemoveListener(OnRefreshSpline);
				Spline.OnMoveControlPointReached.RemoveListener(onControlPointReachedEvent);
				Spline.OnMoveEndReached.RemoveListener(onEndReachedEvent);
				Spline.OnBeforeControlPointAdd.RemoveListener(onBeforeCPChange);
				Spline.OnBeforeControlPointDelete.RemoveListener(onBeforeCPChange);
			}
		}

		protected virtual void OnRefreshSpline(CurvySplineEventArgs e)
		{
			if (e.Sender is CurvySplineBase && e.Sender != Spline)
			{
				((CurvySplineBase)e.Sender).OnRefresh.RemoveListener(OnRefreshSpline);
			}
			else if (base.Active)
			{
				if (Application.isPlaying)
				{
					if (mKeepDistanceAt > 0f && base.IsPlaying)
					{
						AbsolutePosition = Mathf.Max(0f, mKeepDistanceAt);
					}
					if (Speed == 0f)
					{
						Refresh();
					}
				}
				else if (IsInitialized)
				{
					Prepare();
				}
			}
			else
			{
				UnbindEvents();
			}
		}

		private void getInterpolatedSourcePosition(CurvySplineBase spline, float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
		{
			if (spline.Length == 0f)
			{
				position = spline.transform.localPosition;
				tangent = Vector3.forward;
				up = Vector3.up;
			}
			else
			{
				if (UseCache)
				{
					position = spline.InterpolateFast(tf);
					tangent = spline.GetTangentFast(tf);
				}
				else
				{
					position = spline.Interpolate(tf);
					tangent = spline.GetTangent(tf, position);
				}
				up = spline.GetOrientationUpFast(tf);
			}
			if (base.Space == Space.World)
			{
				position = spline.transform.TransformPoint(position);
				tangent = spline.transform.TransformDirection(tangent);
				up = spline.transform.TransformDirection(up);
			}
		}

		private void onBeforeCPChange(CurvyControlPointEventArgs e)
		{
			if (e.Sender is CurvySplineBase && e.Sender != Spline)
			{
				((CurvySplineBase)e.Sender).OnRefresh.RemoveListener(OnRefreshSpline);
			}
			else if (base.Active)
			{
				if (mKeepDistanceAt == 0f)
				{
					switch (e.Mode)
					{
					case CurvyControlPointEventArgs.ModeEnum.Delete:
						mKeepDistanceAt = AbsolutePosition - e.ControlPoint.Length;
						break;
					case CurvyControlPointEventArgs.ModeEnum.None:
						break;
					default:
						mKeepDistanceAt = AbsolutePosition;
						break;
					}
				}
			}
			else
			{
				UnbindEvents();
			}
		}

		private void onControlPointReachedEvent(CurvySplineMoveEventArgs e)
		{
			if (_active == this)
			{
				if ((bool)e.Spline && e.Spline != Spline)
				{
					e.Spline.OnMoveControlPointReached.RemoveListener(onControlPointReachedEvent);
					return;
				}
				e.Sender = this;
				OnControlPointReached.Invoke(e);
			}
		}

		private void onEndReachedEvent(CurvySplineMoveEventArgs e)
		{
			if (_active == this)
			{
				if ((bool)e.Spline && e.Spline != Spline)
				{
					e.Spline.OnMoveEndReached.RemoveListener(onEndReachedEvent);
					return;
				}
				e.Sender = this;
				OnEndReached.Invoke(e);
			}
		}

		private void onSwitchEvent(CurvySplineMoveEventArgs e)
		{
			OnSwitch.Invoke(e);
			if (e.Cancel)
			{
				mSwitchEventArgs = null;
			}
		}
	}
}
