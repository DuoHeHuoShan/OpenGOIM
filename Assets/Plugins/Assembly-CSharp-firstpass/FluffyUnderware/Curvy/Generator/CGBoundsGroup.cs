using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGBoundsGroup
	{
		public enum DistributionModeEnum
		{
			Parent = 0,
			Self = 1
		}

		public enum RotationModeEnum
		{
			Full = 0,
			Direction = 1,
			Horizontal = 2,
			Independent = 3
		}

		[SerializeField]
		private string m_Name;

		[SerializeField]
		private bool m_KeepTogether;

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_SpaceBefore = new FloatRegion
		{
			SimpleValue = true
		};

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_SpaceAfter = new FloatRegion
		{
			SimpleValue = true
		};

		[RangeEx(0f, 1f, "", "", Slider = true, Precision = 1)]
		[SerializeField]
		private float m_Weight = 0.5f;

		[SerializeField]
		private CurvyRepeatingOrderEnum m_RepeatingOrder = CurvyRepeatingOrderEnum.Row;

		[IntRegion(UseSlider = false, RegionOptionsPropertyName = "RepeatingGroupsOptions", Options = AttributeOptionsFlags.Compact)]
		[SerializeField]
		private IntRegion m_RepeatingItems;

		[SerializeField]
		[Header("Lateral Placement")]
		private DistributionModeEnum m_DistributionMode;

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, RegionOptionsPropertyName = "PositionRangeOptions", UseSlider = true, Precision = 3)]
		private FloatRegion m_PositionOffset = new FloatRegion(0f);

		[SerializeField]
		[FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
		private FloatRegion m_Height = new FloatRegion(0f);

		[Header("Rotation")]
		[Label("Mode", "")]
		[SerializeField]
		private RotationModeEnum m_RotationMode;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_RotationOffset;

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_RotationScatter;

		[SerializeField]
		private List<CGBoundsGroupItem> m_Items = new List<CGBoundsGroupItem>();

		private WeightedRandom<int> mItemBag;

		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				if (m_Name != value)
				{
					m_Name = value;
				}
			}
		}

		public bool KeepTogether
		{
			get
			{
				return m_KeepTogether;
			}
			set
			{
				if (m_KeepTogether != value)
				{
					m_KeepTogether = value;
				}
			}
		}

		public FloatRegion SpaceBefore
		{
			get
			{
				return m_SpaceBefore;
			}
			set
			{
				if (m_SpaceBefore != value)
				{
					m_SpaceBefore = value;
				}
			}
		}

		public FloatRegion SpaceAfter
		{
			get
			{
				return m_SpaceAfter;
			}
			set
			{
				if (m_SpaceAfter != value)
				{
					m_SpaceAfter = value;
				}
			}
		}

		public float Weight
		{
			get
			{
				return m_Weight;
			}
			set
			{
				float num = Mathf.Clamp01(value);
				if (m_Weight != num)
				{
					m_Weight = num;
				}
			}
		}

		public CurvyRepeatingOrderEnum RepeatingOrder
		{
			get
			{
				return m_RepeatingOrder;
			}
			set
			{
				if (m_RepeatingOrder != value)
				{
					m_RepeatingOrder = value;
				}
			}
		}

		public IntRegion RepeatingItems
		{
			get
			{
				return m_RepeatingItems;
			}
			set
			{
				if (m_RepeatingItems != value)
				{
					m_RepeatingItems = value;
				}
			}
		}

		public DistributionModeEnum DistributionMode
		{
			get
			{
				return m_DistributionMode;
			}
			set
			{
				if (m_DistributionMode != value)
				{
					m_DistributionMode = value;
				}
			}
		}

		public FloatRegion PositionOffset
		{
			get
			{
				return m_PositionOffset;
			}
			set
			{
				if (m_PositionOffset != value)
				{
					m_PositionOffset = value;
				}
			}
		}

		public FloatRegion Height
		{
			get
			{
				return m_Height;
			}
			set
			{
				if (m_Height != value)
				{
					m_Height = value;
				}
			}
		}

		public RotationModeEnum RotationMode
		{
			get
			{
				return m_RotationMode;
			}
			set
			{
				if (m_RotationMode != value)
				{
					m_RotationMode = value;
				}
			}
		}

		public Vector3 RotationOffset
		{
			get
			{
				return m_RotationOffset;
			}
			set
			{
				if (m_RotationOffset != value)
				{
					m_RotationOffset = value;
				}
			}
		}

		public Vector3 RotationScatter
		{
			get
			{
				return m_RotationScatter;
			}
			set
			{
				if (m_RotationScatter != value)
				{
					m_RotationScatter = value;
				}
			}
		}

		public List<CGBoundsGroupItem> Items
		{
			get
			{
				return m_Items;
			}
		}

		public int FirstRepeating
		{
			get
			{
				return m_RepeatingItems.From;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, Mathf.Max(0, ItemCount - 1));
				if (m_RepeatingItems.From != num)
				{
					m_RepeatingItems.From = num;
				}
			}
		}

		public int LastRepeating
		{
			get
			{
				return m_RepeatingItems.To;
			}
			set
			{
				int num = Mathf.Clamp(value, FirstRepeating, Mathf.Max(0, ItemCount - 1));
				if (m_RepeatingItems.To != num)
				{
					m_RepeatingItems.To = num;
				}
			}
		}

		public int ItemCount
		{
			get
			{
				return Items.Count;
			}
		}

		private RegionOptions<int> RepeatingGroupsOptions
		{
			get
			{
				return RegionOptions<int>.MinMax(0, Mathf.Max(0, ItemCount - 1));
			}
		}

		private RegionOptions<float> PositionRangeOptions
		{
			get
			{
				return RegionOptions<float>.MinMax(-0.5f, 0.5f);
			}
		}

		private int lastItemIndex
		{
			get
			{
				return Mathf.Max(0, ItemCount - 1);
			}
		}

		public CGBoundsGroup(string name)
		{
			Name = name;
		}

		internal void PrepareINTERNAL()
		{
			m_RepeatingItems.MakePositive();
			m_RepeatingItems.Clamp(0, ItemCount - 1);
			if (mItemBag == null)
			{
				mItemBag = new WeightedRandom<int>();
			}
			else
			{
				mItemBag.Clear();
			}
			if (Items.Count != 0 && RepeatingOrder == CurvyRepeatingOrderEnum.Random)
			{
				for (int i = FirstRepeating; i <= LastRepeating; i++)
				{
					mItemBag.Add(i, (int)(Items[i].Weight * 10f));
				}
			}
		}

		internal int getRandomItemINTERNAL()
		{
			return mItemBag.Next();
		}
	}
}
