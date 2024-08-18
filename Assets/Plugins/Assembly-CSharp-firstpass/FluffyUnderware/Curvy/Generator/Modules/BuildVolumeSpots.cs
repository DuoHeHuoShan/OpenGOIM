using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Volume Spots", ModuleName = "Volume Spots", Description = "Generate spots along a path/volume", UsesRandom = true)]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgvolumespots")]
	public class BuildVolumeSpots : CGModule
	{
		private class GroupSet
		{
			public CGBoundsGroup Group;

			public float Length;

			public List<int> Items = new List<int>();

			public List<float> Distances = new List<float>();
		}

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path/Volume")]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGBounds) }, Array = true)]
		public CGModuleInputSlot InBounds = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGSpots))]
		public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

		[Tab("General")]
		[FloatRegion(RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
		[SerializeField]
		private FloatRegion m_Range = FloatRegion.ZeroOne;

		[SerializeField]
		private bool m_UseVolume;

		[Tooltip("Dry run without actually creating spots?")]
		[SerializeField]
		private bool m_Simulate;

		[Section("Default/General/Cross", true, false, 100)]
		[SerializeField]
		[RangeEx(-1f, 1f, "", "")]
		private float m_CrossBase;

		[SerializeField]
		private AnimationCurve m_CrossCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

		[Tab("Groups")]
		[ArrayEx(Space = 10)]
		[SerializeField]
		private List<CGBoundsGroup> m_Groups = new List<CGBoundsGroup>();

		[IntRegion(UseSlider = false, RegionOptionsPropertyName = "RepeatingGroupsOptions", Options = AttributeOptionsFlags.Compact)]
		[SerializeField]
		private IntRegion m_RepeatingGroups;

		[SerializeField]
		private CurvyRepeatingOrderEnum m_RepeatingOrder = CurvyRepeatingOrderEnum.Row;

		[SerializeField]
		private bool m_FitEnd;

		public CGSpots SimulatedSpots;

		private WeightedRandom<int> mGroupBag;

		private List<CGBounds> mBounds;

		private bool mGroupsHaveDepth;

		public FloatRegion Range
		{
			get
			{
				return m_Range;
			}
			set
			{
				if (m_Range != value)
				{
					m_Range = value;
				}
				base.Dirty = true;
			}
		}

		public bool UseVolume
		{
			get
			{
				return m_UseVolume;
			}
			set
			{
				if (m_UseVolume != value)
				{
					m_UseVolume = value;
				}
				base.Dirty = true;
			}
		}

		public bool Simulate
		{
			get
			{
				return m_Simulate;
			}
			set
			{
				if (m_Simulate != value)
				{
					m_Simulate = value;
				}
				base.Dirty = true;
			}
		}

		public float CrossBase
		{
			get
			{
				return m_CrossBase;
			}
			set
			{
				float num = Mathf.Repeat(value, 1f);
				if (m_CrossBase != num)
				{
					m_CrossBase = num;
				}
				base.Dirty = true;
			}
		}

		public AnimationCurve CrossCurve
		{
			get
			{
				return m_CrossCurve;
			}
			set
			{
				if (m_CrossCurve != value)
				{
					m_CrossCurve = value;
				}
				base.Dirty = true;
			}
		}

		public List<CGBoundsGroup> Groups
		{
			get
			{
				return m_Groups;
			}
			set
			{
				if (m_Groups != value)
				{
					m_Groups = value;
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
				base.Dirty = true;
			}
		}

		public int FirstRepeating
		{
			get
			{
				return m_RepeatingGroups.From;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, Mathf.Max(0, GroupCount - 1));
				if (m_RepeatingGroups.From != num)
				{
					m_RepeatingGroups.From = num;
				}
				base.Dirty = true;
			}
		}

		public int LastRepeating
		{
			get
			{
				return m_RepeatingGroups.To;
			}
			set
			{
				int num = Mathf.Clamp(value, FirstRepeating, Mathf.Max(0, GroupCount - 1));
				if (m_RepeatingGroups.To != num)
				{
					m_RepeatingGroups.To = num;
				}
				base.Dirty = true;
			}
		}

		public bool FitEnd
		{
			get
			{
				return m_FitEnd;
			}
			set
			{
				if (m_FitEnd != value)
				{
					m_FitEnd = value;
				}
				base.Dirty = true;
			}
		}

		public int GroupCount
		{
			get
			{
				return Groups.Count;
			}
		}

		public GUIContent[] BoundsNames
		{
			get
			{
				if (mBounds == null)
				{
					return new GUIContent[0];
				}
				GUIContent[] array = new GUIContent[mBounds.Count];
				for (int i = 0; i < mBounds.Count; i++)
				{
					array[i] = new GUIContent(i + ":" + mBounds[i].Name);
				}
				return array;
			}
		}

		public int[] BoundsIndices
		{
			get
			{
				if (mBounds == null)
				{
					return new int[0];
				}
				int[] array = new int[mBounds.Count];
				for (int i = 0; i < mBounds.Count; i++)
				{
					array[i] = i;
				}
				return array;
			}
		}

		public int Count { get; private set; }

		private int lastGroupIndex
		{
			get
			{
				return Mathf.Max(0, GroupCount - 1);
			}
		}

		private RegionOptions<float> RangeOptions
		{
			get
			{
				return RegionOptions<float>.MinMax(0f, 1f);
			}
		}

		private RegionOptions<int> RepeatingGroupsOptions
		{
			get
			{
				return RegionOptions<int>.MinMax(0, Mathf.Max(0, GroupCount - 1));
			}
		}

		private CGPath Path { get; set; }

		private CGVolume Volume
		{
			get
			{
				return Path as CGVolume;
			}
		}

		private bool UsePath
		{
			get
			{
				return Volume == null || UseVolume;
			}
		}

		private float Length
		{
			get
			{
				return (Path == null) ? 0f : (Path.Length * m_Range.Length);
			}
		}

		private float StartDistance { get; set; }

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 350f;
		}

		public override void Reset()
		{
			base.Reset();
			m_Range = FloatRegion.ZeroOne;
			UseVolume = false;
			Simulate = false;
			CrossBase = 0f;
			CrossCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
			RepeatingOrder = CurvyRepeatingOrderEnum.Row;
			FirstRepeating = 0;
			LastRepeating = 0;
			FitEnd = false;
			Groups.Clear();
			AddGroup("Group");
		}

		public override void OnStateChange()
		{
			base.OnStateChange();
			if (!IsConfigured)
			{
				Clear();
			}
		}

		public void Clear()
		{
			Count = 0;
			SimulatedSpots = new CGSpots();
			OutSpots.SetData(SimulatedSpots);
		}

		public override void Refresh()
		{
			base.Refresh();
			mBounds = InBounds.GetAllData<CGBounds>(new CGDataRequestParameter[0]);
			Path = InPath.GetData<CGPath>(new CGDataRequestParameter[0]);
			if (Path != null && Volume == null && UseVolume)
			{
				m_UseVolume = false;
			}
			List<CGSpot> spots = new List<CGSpot>();
			List<GroupSet> sets = null;
			prepare();
			if ((bool)Path && mBounds.Count > 0 && mGroupsHaveDepth)
			{
				float remainingLength = Length;
				StartDistance = Path.FToDistance(m_Range.Low);
				float currentDistance = StartDistance;
				for (int i = 0; i < FirstRepeating; i++)
				{
					addGroupItems(Groups[i], ref spots, ref remainingLength, ref currentDistance);
					if (remainingLength <= 0f)
					{
						break;
					}
				}
				if (GroupCount - LastRepeating - 1 > 0)
				{
					sets = new List<GroupSet>();
					float currentDistance2 = 0f;
					for (int j = LastRepeating + 1; j < GroupCount; j++)
					{
						sets.Add(addGroupItems(Groups[j], ref spots, ref remainingLength, ref currentDistance2, true));
					}
				}
				if (RepeatingOrder == CurvyRepeatingOrderEnum.Row)
				{
					int firstRepeating = FirstRepeating;
					while (remainingLength > 0f)
					{
						addGroupItems(Groups[firstRepeating++], ref spots, ref remainingLength, ref currentDistance);
						if (firstRepeating > LastRepeating)
						{
							firstRepeating = FirstRepeating;
						}
					}
				}
				else
				{
					while (remainingLength > 0f)
					{
						addGroupItems(Groups[mGroupBag.Next()], ref spots, ref remainingLength, ref currentDistance);
					}
				}
				if (sets != null)
				{
					rebase(ref spots, ref sets, currentDistance, remainingLength);
				}
			}
			Count = spots.Count;
			SimulatedSpots = new CGSpots(spots);
			if (Simulate)
			{
				OutSpots.SetData(new CGSpots());
			}
			else
			{
				OutSpots.SetData(SimulatedSpots);
			}
		}

		public CGBoundsGroup AddGroup(string name)
		{
			CGBoundsGroup cGBoundsGroup = new CGBoundsGroup(name);
			Groups.Add(cGBoundsGroup);
			base.Dirty = true;
			return cGBoundsGroup;
		}

		public void RemoveGroup(CGBoundsGroup group)
		{
			Groups.Remove(group);
			base.Dirty = true;
		}

		private GroupSet addGroupItems(CGBoundsGroup group, ref List<CGSpot> spots, ref float remainingLength, ref float currentDistance, bool calcLengthOnly = false)
		{
			if (group.ItemCount == 0)
			{
				return null;
			}
			int num = 0;
			float next = group.SpaceBefore.Next;
			float next2 = group.SpaceAfter.Next;
			float num2 = remainingLength - next;
			GroupSet groupSet = null;
			GroupSet groupSet2 = new GroupSet();
			float num3 = currentDistance + next;
			if (calcLengthOnly)
			{
				groupSet = new GroupSet();
				groupSet.Group = group;
				groupSet.Length = next + next2;
			}
			for (int i = 0; i < group.FirstRepeating; i++)
			{
				int index = group.Items[i].Index;
				CGBounds bounds = getItemBounds(index);
				if (!bounds)
				{
					continue;
				}
				num2 -= bounds.Depth;
				if (num2 > 0f)
				{
					if (calcLengthOnly)
					{
						groupSet.Length += bounds.Depth;
						groupSet.Items.Add(index);
						groupSet.Distances.Add(num3);
					}
					else
					{
						spots.Add(getSpot(index, ref group, ref bounds, num3, remainingLength));
					}
					num3 += bounds.Depth;
					num++;
					continue;
				}
				if (group.KeepTogether && num > 0)
				{
					spots.RemoveRange(spots.Count - num, num);
				}
				break;
			}
			if (num2 > 0f)
			{
				float num4 = 0f;
				for (int j = group.LastRepeating + 1; j < group.ItemCount; j++)
				{
					int index = group.Items[j].Index;
					CGBounds bounds = getItemBounds(index);
					if ((bool)bounds)
					{
						num2 -= bounds.Depth;
						if (!(num2 > 0f))
						{
							break;
						}
						groupSet2.Length += bounds.Depth;
						groupSet2.Items.Add(index);
						groupSet2.Distances.Add(num4);
						num4 += bounds.Depth;
					}
				}
				if (num2 > 0f)
				{
					for (int k = group.FirstRepeating; k <= group.LastRepeating; k++)
					{
						int index2 = ((group.RepeatingOrder != CurvyRepeatingOrderEnum.Row) ? group.getRandomItemINTERNAL() : k);
						int index = group.Items[index2].Index;
						CGBounds bounds = getItemBounds(index);
						if (!bounds)
						{
							continue;
						}
						num2 -= bounds.Depth;
						if (num2 > 0f)
						{
							if (calcLengthOnly)
							{
								groupSet.Length += bounds.Depth;
								groupSet.Items.Add(index);
								groupSet.Distances.Add(num3);
							}
							else
							{
								spots.Add(getSpot(index, ref group, ref bounds, num3, remainingLength));
							}
							num3 += bounds.Depth;
							num++;
							continue;
						}
						if (group.KeepTogether && num > 0)
						{
							spots.RemoveRange(spots.Count - num, num);
						}
						break;
					}
				}
				if (num2 > 0f || !group.KeepTogether)
				{
					for (int l = 0; l < groupSet2.Items.Count; l++)
					{
						CGBounds bounds2 = getItemBounds(groupSet2.Items[l]);
						spots.Add(getSpot(groupSet2.Items[l], ref group, ref bounds2, num3 + groupSet2.Distances[l], remainingLength));
						num3 += bounds2.Depth;
					}
				}
			}
			remainingLength = num2 - next2;
			currentDistance = num3 + next2;
			return groupSet;
		}

		private void rebase(ref List<CGSpot> spots, ref List<GroupSet> sets, float currentDistance, float remainingLength)
		{
			if (FitEnd)
			{
				currentDistance = Path.FToDistance(m_Range.To);
				for (int i = 0; i < sets.Count; i++)
				{
					currentDistance -= sets[i].Length;
				}
			}
			for (int j = 0; j < sets.Count; j++)
			{
				GroupSet groupSet = sets[j];
				for (int k = 0; k < groupSet.Items.Count; k++)
				{
					CGBounds bounds = getItemBounds(groupSet.Items[k]);
					spots.Add(getSpot(groupSet.Items[k], ref groupSet.Group, ref bounds, currentDistance + groupSet.Distances[k], remainingLength));
				}
			}
		}

		private CGSpot getSpot(int itemID, ref CGBoundsGroup group, ref CGBounds bounds, float startDist, float remainingDistance)
		{
			CGSpot result = new CGSpot(itemID);
			float f = Path.DistanceToF(startDist + bounds.Depth / 2f);
			Vector3 pos = Vector3.zero;
			Vector3 dir = Vector3.forward;
			Vector3 up = Vector3.up;
			float crossValue = getCrossValue((startDist - StartDistance) / Length, group);
			if (group.RotationMode != CGBoundsGroup.RotationModeEnum.Independent)
			{
				if (UseVolume)
				{
					Volume.InterpolateVolume(f, crossValue, out pos, out dir, out up);
				}
				else
				{
					Path.Interpolate(f, crossValue, out pos, out dir, out up);
				}
				switch (group.RotationMode)
				{
				case CGBoundsGroup.RotationModeEnum.Direction:
					up = Vector3.up;
					break;
				case CGBoundsGroup.RotationModeEnum.Horizontal:
					up = Vector3.up;
					dir.y = 0f;
					break;
				}
			}
			else
			{
				pos = ((!UseVolume) ? Path.InterpolatePosition(f) : Volume.InterpolateVolumePosition(f, crossValue));
			}
			if (Path.SourceIsManaged)
			{
				result.Rotation = Quaternion.LookRotation(dir, up) * Quaternion.Euler(group.RotationOffset.x + group.RotationScatter.x * (float)UnityEngine.Random.Range(-1, 1), group.RotationOffset.y + group.RotationScatter.y * (float)UnityEngine.Random.Range(-1, 1), group.RotationOffset.z + group.RotationScatter.z * (float)UnityEngine.Random.Range(-1, 1));
				result.Position = pos + result.Rotation * new Vector3(0f, group.Height.Next, 0f);
			}
			else
			{
				result.Rotation = Quaternion.LookRotation(dir, up) * Quaternion.Euler(group.RotationOffset.x + group.RotationScatter.x * (float)UnityEngine.Random.Range(-1, 1), group.RotationOffset.y + group.RotationScatter.y * (float)UnityEngine.Random.Range(-1, 1), group.RotationOffset.z + group.RotationScatter.z * (float)UnityEngine.Random.Range(-1, 1));
				result.Position = pos + result.Rotation * new Vector3(0f, group.Height.Next, 0f);
			}
			return result;
		}

		private void prepare()
		{
			mGroupsHaveDepth = false;
			m_RepeatingGroups.MakePositive();
			m_RepeatingGroups.Clamp(0, GroupCount - 1);
			if (mGroupBag == null)
			{
				mGroupBag = new WeightedRandom<int>();
			}
			else
			{
				mGroupBag.Clear();
			}
			if (RepeatingOrder == CurvyRepeatingOrderEnum.Random)
			{
				for (int i = FirstRepeating; i <= LastRepeating; i++)
				{
					mGroupBag.Add(i, (int)(Groups[i].Weight * 10f));
				}
			}
			for (int j = 0; j < Groups.Count; j++)
			{
				Groups[j].PrepareINTERNAL();
				mGroupsHaveDepth = mGroupsHaveDepth || getMinGroupDepth(Groups[j]) > 0f;
			}
		}

		private float getMinGroupDepth(CGBoundsGroup group)
		{
			float num = group.SpaceBefore.Low + group.SpaceAfter.Low;
			for (int i = 0; i < group.ItemCount; i++)
			{
				CGBounds itemBounds = getItemBounds(group.Items[i].Index);
				if ((bool)itemBounds)
				{
					num += itemBounds.Depth;
				}
			}
			return num;
		}

		private CGBounds getItemBounds(int itemIndex)
		{
			return (itemIndex < 0 || itemIndex >= mBounds.Count) ? null : mBounds[itemIndex];
		}

		private float getCrossValue(float globalF, CGBoundsGroup group)
		{
			switch (group.DistributionMode)
			{
			case CGBoundsGroup.DistributionModeEnum.Parent:
				return DTMath.MapValue(-0.5f, 0.5f, CrossBase + m_CrossCurve.Evaluate(globalF) + group.PositionOffset.Next);
			case CGBoundsGroup.DistributionModeEnum.Self:
				return DTMath.MapValue(-0.5f, 0.5f, group.PositionOffset.Next);
			default:
				return 0f;
			}
		}
	}
}
