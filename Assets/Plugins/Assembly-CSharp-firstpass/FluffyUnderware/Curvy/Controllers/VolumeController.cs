using System.Collections;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[AddComponentMenu("Curvy/Controller/CG Volume Controller", 8)]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/volumecontroller")]
	public class VolumeController : CurvyController
	{
		[Section("General", true, false, 100)]
		[CGDataReferenceSelector(typeof(CGVolume), Label = "Volume/Slot")]
		[SerializeField]
		private CGDataReference m_Volume = new CGDataReference();

		[Section("Cross Position", true, false, 100, Sort = 1, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/volumecontroller_crossposition")]
		[SerializeField]
		[FloatRegion(UseSlider = true, Precision = 4, RegionOptionsPropertyName = "CrossRangeOptions", Options = AttributeOptionsFlags.Full)]
		private FloatRegion m_CrossRange = new FloatRegion(-0.5f, 0.5f);

		[SerializeField]
		[RangeEx(-0.5f, 0.5f, "", "")]
		private float m_CrossInitialPosition;

		[SerializeField]
		private CurvyClamping m_CrossClamping;

		private float mKeepDistanceAt;

		private float mCrossTF;

		public CGDataReference Volume
		{
			get
			{
				return m_Volume;
			}
			set
			{
				if (m_Volume == value)
				{
					return;
				}
				if (m_Volume != null)
				{
					UnbindEvents();
				}
				m_Volume = value;
				if (m_Volume != null)
				{
					if (!mIsPrepared)
					{
						Prepare();
					}
					else
					{
						BindEvents();
					}
				}
			}
		}

		public CGVolume VolumeData
		{
			get
			{
				return (!Volume.HasValue) ? null : Volume.GetData<CGVolume>();
			}
		}

		public float CrossFrom
		{
			get
			{
				return m_CrossRange.From;
			}
			set
			{
				float num = Mathf.Max(-0.5f, value);
				if (m_CrossRange.From != num)
				{
					m_CrossRange.From = num;
				}
				if (!Application.isPlaying)
				{
					Prepare();
				}
			}
		}

		public float CrossTo
		{
			get
			{
				return m_CrossRange.To;
			}
			set
			{
				float num = Mathf.Clamp(value, CrossFrom, 0.5f);
				if (m_CrossRange.To != num)
				{
					m_CrossRange.To = num;
				}
				if (!Application.isPlaying)
				{
					Prepare();
				}
			}
		}

		public float CrossLength
		{
			get
			{
				return m_CrossRange.Length;
			}
		}

		public float CrossInitialPosition
		{
			get
			{
				return m_CrossInitialPosition;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				if (m_CrossInitialPosition != num)
				{
					m_CrossInitialPosition = num;
					if (!Application.isPlaying)
					{
						Prepare();
					}
				}
			}
		}

		public CurvyClamping CrossClamping
		{
			get
			{
				return m_CrossClamping;
			}
			set
			{
				if (m_CrossClamping != value)
				{
					m_CrossClamping = value;
				}
			}
		}

		public float CrossPosition
		{
			get
			{
				return getUnrangedCross(mCrossTF);
			}
			set
			{
				float rangedCross = getRangedCross(CurvyUtility.ClampValue(value, CrossClamping, -0.5f, 0.5f));
				if (mCrossTF != rangedCross)
				{
					mCrossTF = rangedCross;
					if (Speed == 0f)
					{
						Refresh();
					}
				}
			}
		}

		public override bool IsConfigured
		{
			get
			{
				return Volume != null && !Volume.IsEmpty;
			}
		}

		public override bool DependenciesInitialized
		{
			get
			{
				return IsConfigured && Volume.HasValue;
			}
		}

		public override float Length
		{
			get
			{
				return (VolumeData == null) ? 0f : VolumeData.Length;
			}
		}

		private RegionOptions<float> CrossRangeOptions
		{
			get
			{
				return RegionOptions<float>.MinMax(-0.5f, 0.5f);
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
			Volume = null;
		}

		public override void Prepare()
		{
			BindEvents();
			if (IsInitialized)
			{
				mCrossTF = getRangedCross(CrossInitialPosition);
			}
			base.Prepare();
		}

		public float CrossRelativeToAbsolute(float relativeDistance)
		{
			return (VolumeData == null) ? 0f : VolumeData.CrossFToDistance(GetTF(Position), relativeDistance, CrossClamping);
		}

		public float CrossAbsoluteToRelative(float worldUnitDistance)
		{
			return (VolumeData == null) ? 0f : VolumeData.CrossDistanceToF(GetTF(Position), worldUnitDistance, CrossClamping);
		}

		protected override float RelativeToAbsolute(float relativeDistance)
		{
			return (VolumeData == null) ? 0f : VolumeData.FToDistance(relativeDistance);
		}

		protected override float AbsoluteToRelative(float worldUnitDistance)
		{
			return (VolumeData == null) ? 0f : VolumeData.DistanceToF(worldUnitDistance);
		}

		protected override Vector3 GetInterpolatedSourcePosition(float tf)
		{
			return (base.Space != 0) ? VolumeData.InterpolateVolumePosition(tf, mCrossTF) : Volume.Module.Generator.transform.TransformPoint(VolumeData.InterpolateVolumePosition(tf, mCrossTF));
		}

		protected override void GetInterpolatedSourcePosition(float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
		{
			VolumeData.InterpolateVolume(tf, mCrossTF, out position, out tangent, out up);
			if (base.Space == Space.World)
			{
				position = Volume.Module.Generator.transform.TransformPoint(position);
				tangent = Volume.Module.Generator.transform.TransformDirection(tangent);
				up = Volume.Module.Generator.transform.TransformDirection(up);
			}
		}

		protected override Vector3 GetTangent(float tf)
		{
			return (base.Space != 0) ? VolumeData.InterpolateVolumeDirection(tf, mCrossTF) : Volume.Module.Generator.transform.TransformDirection(VolumeData.InterpolateVolumeDirection(tf, mCrossTF));
		}

		protected override Vector3 GetOrientation(float tf)
		{
			return (base.Space != 0) ? VolumeData.InterpolateVolumeUp(tf, mCrossTF) : Volume.Module.Generator.transform.TransformDirection(VolumeData.InterpolateVolumeUp(tf, mCrossTF));
		}

		protected override void Advance(ref float virtualPosition, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
		{
			if (mode == MoveModeEnum.Relative)
			{
				VolumeData.Move(ref virtualPosition, ref direction, absSpeed, clamping);
			}
			else
			{
				VolumeData.MoveBy(ref virtualPosition, ref direction, absSpeed, clamping);
			}
		}

		protected override void BindEvents()
		{
			base.BindEvents();
			if (Volume != null && Volume.Module != null)
			{
				UnbindEvents();
				Volume.Module.OnRefresh.AddListener(OnRefreshPath);
			}
		}

		protected override void UnbindEvents()
		{
			if (Volume != null && Volume.Module != null)
			{
				Volume.Module.OnRefresh.RemoveListener(OnRefreshPath);
			}
		}

		protected virtual void OnRefreshPath(CurvyCGEventArgs e)
		{
			if (Volume == null || e.Module != Volume.Module)
			{
				e.Module.OnRefresh.RemoveListener(OnRefreshPath);
			}
			else if (base.Active)
			{
				if (Application.isPlaying)
				{
					if (mKeepDistanceAt > 0f && IsInitialized && base.IsPlaying)
					{
						AbsolutePosition = mKeepDistanceAt;
						mKeepDistanceAt = 0f;
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

		private float getRangedCross(float f)
		{
			return DTMath.MapValue(CrossFrom, CrossTo, f, -0.5f, 0.5f);
		}

		private float getUnrangedCross(float f)
		{
			return DTMath.MapValue(-0.5f, 0.5f, f, CrossFrom, CrossTo);
		}
	}
}
