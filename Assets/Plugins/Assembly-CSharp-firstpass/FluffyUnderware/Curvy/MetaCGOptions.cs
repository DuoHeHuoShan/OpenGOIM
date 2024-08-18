using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/metacgoptions")]
	public class MetaCGOptions : CurvyMetadataBase, ICurvyMetadata
	{
		[Positive]
		[SerializeField]
		private int m_MaterialID;

		[SerializeField]
		private bool m_HardEdge;

		[Positive(Tooltip = "Max step distance when using optimization")]
		[SerializeField]
		private float m_MaxStepDistance;

		[Section("Extended UV", true, false, 100, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/metacgoptions_extendeduv")]
		[FieldCondition("showUVEdge", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_UVEdge;

		[Positive]
		[FieldCondition("showExplicitU", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_ExplicitU;

		[FieldCondition("showFirstU", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[FieldAction("CBSetFirstU", ActionAttribute.ActionEnum.Callback)]
		[Positive]
		[SerializeField]
		private float m_FirstU;

		[FieldCondition("showSecondU", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Positive]
		[SerializeField]
		private float m_SecondU;

		public int MaterialID
		{
			get
			{
				return m_MaterialID;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_MaterialID != num)
				{
					m_MaterialID = num;
					SetDirty();
				}
			}
		}

		public bool HardEdge
		{
			get
			{
				return m_HardEdge;
			}
			set
			{
				if (m_HardEdge != value)
				{
					m_HardEdge = value;
					SetDirty();
				}
			}
		}

		public bool UVEdge
		{
			get
			{
				return m_UVEdge;
			}
			set
			{
				if (m_UVEdge != value)
				{
					m_UVEdge = value;
					SetDirty();
				}
			}
		}

		public bool ExplicitU
		{
			get
			{
				return m_ExplicitU;
			}
			set
			{
				if (m_ExplicitU != value)
				{
					m_ExplicitU = value;
					SetDirty();
				}
			}
		}

		public float FirstU
		{
			get
			{
				return m_FirstU;
			}
			set
			{
				if (m_FirstU != value)
				{
					m_FirstU = value;
					SetDirty();
				}
			}
		}

		public float SecondU
		{
			get
			{
				return m_SecondU;
			}
			set
			{
				if (m_SecondU != value)
				{
					m_SecondU = value;
					SetDirty();
				}
			}
		}

		public float MaxStepDistance
		{
			get
			{
				return m_MaxStepDistance;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_MaxStepDistance != num)
				{
					m_MaxStepDistance = num;
					SetDirty();
				}
			}
		}

		public bool HasDifferentMaterial
		{
			get
			{
				return (bool)base.ControlPoint.PreviousControlPoint && GetPreviousData<MetaCGOptions>().MaterialID != MaterialID;
			}
		}

		private bool showUVEdge
		{
			get
			{
				return (bool)base.ControlPoint && (base.Spline.Closed || (!base.ControlPoint.IsFirstVisibleControlPoint && !base.ControlPoint.IsLastVisibleControlPoint)) && !HasDifferentMaterial;
			}
		}

		private bool showExplicitU
		{
			get
			{
				return (bool)base.ControlPoint && !UVEdge && !HasDifferentMaterial;
			}
		}

		private bool showFirstU
		{
			get
			{
				bool result = false;
				if ((bool)base.ControlPoint)
				{
					result = UVEdge || ExplicitU || HasDifferentMaterial;
				}
				return result;
			}
		}

		private bool showSecondU
		{
			get
			{
				return UVEdge || HasDifferentMaterial;
			}
		}

		public void Reset()
		{
			MaterialID = 0;
			HardEdge = false;
			MaxStepDistance = 0f;
			UVEdge = false;
			ExplicitU = false;
			FirstU = 0f;
			SecondU = 0f;
		}

		public float GetDefinedFirstU(float defaultValue)
		{
			return (!UVEdge && !ExplicitU && !HasDifferentMaterial) ? defaultValue : FirstU;
		}

		public float GetDefinedSecondU(float defaultValue)
		{
			return (!UVEdge && !HasDifferentMaterial) ? GetDefinedFirstU(defaultValue) : SecondU;
		}

		public MetaCGOptions GetPreviousDefined(out CurvySplineSegment cp)
		{
			if (base.Spline.Closed && base.ControlPoint.IsFirstVisibleControlPoint)
			{
				cp = base.ControlPoint;
				return this;
			}
			cp = base.ControlPoint.PreviousControlPoint;
			while ((bool)cp && !cp.IsLastVisibleControlPoint)
			{
				MetaCGOptions metadata = cp.GetMetadata<MetaCGOptions>(true);
				if (metadata.UVEdge || metadata.ExplicitU || metadata.HasDifferentMaterial)
				{
					return metadata;
				}
				cp = cp.PreviousControlPoint;
			}
			return null;
		}

		public MetaCGOptions GetNextDefined(out CurvySplineSegment cp)
		{
			cp = base.ControlPoint.NextControlPoint;
			while ((bool)cp)
			{
				MetaCGOptions metadata = cp.GetMetadata<MetaCGOptions>(true);
				if (metadata.UVEdge || metadata.ExplicitU || metadata.HasDifferentMaterial)
				{
					return metadata;
				}
				if (base.Spline.Closed && cp.IsFirstVisibleControlPoint)
				{
					return null;
				}
				cp = cp.NextControlPoint;
			}
			return null;
		}
	}
}
