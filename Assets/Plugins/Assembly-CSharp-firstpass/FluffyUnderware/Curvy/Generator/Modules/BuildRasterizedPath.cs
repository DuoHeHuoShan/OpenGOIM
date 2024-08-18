using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Rasterized Path", ModuleName = "Rasterize Path", Description = "Rasterizes a virtual path")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgbuildrasterizedpath")]
	public class BuildRasterizedPath : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path", RequestDataOnly = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath), Name = "Path")]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		[FloatRegion(UseSlider = true, RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
		[SerializeField]
		private FloatRegion m_Range = FloatRegion.ZeroOne;

		[SerializeField]
		[RangeEx(0f, 100f, "", "")]
		private int m_Resolution = 50;

		[SerializeField]
		private bool m_Optimize;

		[FieldCondition("m_Optimize", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[RangeEx(0.1f, 120f, "", "")]
		private float m_AngleTreshold = 10f;

		public float From
		{
			get
			{
				return m_Range.From;
			}
			set
			{
				float num = Mathf.Repeat(value, 1f);
				if (m_Range.From != num)
				{
					m_Range.From = num;
				}
				base.Dirty = true;
			}
		}

		public float To
		{
			get
			{
				return m_Range.To;
			}
			set
			{
				float num = Mathf.Max(From, value);
				if (PathIsClosed)
				{
					num = Mathf.Repeat(value, 1f);
				}
				if (m_Range.To != num)
				{
					m_Range.To = num;
				}
				base.Dirty = true;
			}
		}

		public float Length
		{
			get
			{
				return (!PathIsClosed) ? m_Range.To : (m_Range.To - m_Range.From);
			}
			set
			{
				float num = ((!PathIsClosed) ? value : (value - m_Range.To));
				if (m_Range.To != num)
				{
					m_Range.To = num;
				}
				base.Dirty = true;
			}
		}

		public int Resolution
		{
			get
			{
				return m_Resolution;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, 100);
				if (m_Resolution != num)
				{
					m_Resolution = num;
				}
				base.Dirty = true;
			}
		}

		public bool Optimize
		{
			get
			{
				return m_Optimize;
			}
			set
			{
				if (m_Optimize != value)
				{
					m_Optimize = value;
				}
				base.Dirty = true;
			}
		}

		public float AngleThreshold
		{
			get
			{
				return m_AngleTreshold;
			}
			set
			{
				float num = Mathf.Clamp(value, 0.1f, 120f);
				if (m_AngleTreshold != num)
				{
					m_AngleTreshold = num;
				}
				base.Dirty = true;
			}
		}

		public CGPath Path
		{
			get
			{
				return OutPath.GetData<CGPath>();
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return !IsConfigured || InPath.SourceSlot().OnRequestPathModule.PathIsClosed;
			}
		}

		private RegionOptions<float> RangeOptions
		{
			get
			{
				if (!PathIsClosed)
				{
					return RegionOptions<float>.MinMax(0f, 1f);
				}
				RegionOptions<float> result = default(RegionOptions<float>);
				result.LabelFrom = "Start";
				result.ClampFrom = DTValueClamping.Min;
				result.FromMin = 0f;
				result.LabelTo = "Length";
				result.ClampTo = DTValueClamping.Range;
				result.ToMin = 0f;
				result.ToMax = 1f;
				return result;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 100f;
		}

		public override void Reset()
		{
			base.Reset();
			m_Range = FloatRegion.ZeroOne;
			Resolution = 50;
			AngleThreshold = 10f;
			OutPath.ClearData();
		}

		public override void Refresh()
		{
			base.Refresh();
			if (Length == 0f)
			{
				Reset();
				return;
			}
			List<CGDataRequestParameter> list = new List<CGDataRequestParameter>();
			list.Add(new CGDataRequestRasterization(From, Length, CGUtility.CalculateSamplePointsCacheSize(Resolution, InPath.SourceSlot().OnRequestPathModule.PathLength), AngleThreshold, Optimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
			CGPath data = InPath.GetData<CGPath>(list.ToArray());
			OutPath.SetData(data);
		}
	}
}
