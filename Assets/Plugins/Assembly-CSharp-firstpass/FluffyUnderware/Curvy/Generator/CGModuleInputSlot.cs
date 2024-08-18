using System;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGModuleInputSlot : CGModuleSlot
	{
		public InputSlotInfo InputInfo
		{
			get
			{
				return base.Info as InputSlotInfo;
			}
		}

		protected override void LoadLinkedSlots()
		{
			if (!base.Module.Generator.IsInitialized)
			{
				return;
			}
			base.LoadLinkedSlots();
			mLinkedSlots = new List<CGModuleSlot>();
			List<CGModuleLink> inputLinks = base.Module.GetInputLinks(this);
			foreach (CGModuleLink item in inputLinks)
			{
				CGModule module = base.Module.Generator.GetModule(item.TargetModuleID, true);
				if ((bool)module)
				{
					CGModuleOutputSlot cGModuleOutputSlot = module.OutputByName[item.TargetSlotName];
					if (!cGModuleOutputSlot.Module.GetOutputLink(cGModuleOutputSlot, this))
					{
						cGModuleOutputSlot.Module.OutputLinks.Add(new CGModuleLink(cGModuleOutputSlot, this));
						cGModuleOutputSlot.ReInitializeLinkedSlots();
					}
					if (!mLinkedSlots.Contains(cGModuleOutputSlot))
					{
						mLinkedSlots.Add(cGModuleOutputSlot);
					}
				}
				else
				{
					base.Module.InputLinks.Remove(item);
				}
			}
		}

		public override void UnlinkAll()
		{
			List<CGModuleSlot> list = new List<CGModuleSlot>(base.LinkedSlots);
			foreach (CGModuleSlot item in list)
			{
				UnlinkFrom(item);
			}
		}

		public override void LinkTo(CGModuleSlot outputSlot)
		{
			if (!HasLinkTo(outputSlot))
			{
				base.Module.InputLinks.Add(new CGModuleLink(this, outputSlot));
				outputSlot.Module.OutputLinks.Add(new CGModuleLink(outputSlot, this));
				if (!base.LinkedSlots.Contains(outputSlot))
				{
					base.LinkedSlots.Add(outputSlot);
				}
				if (!outputSlot.LinkedSlots.Contains(this))
				{
					outputSlot.LinkedSlots.Add(this);
				}
				base.LinkTo(outputSlot);
			}
		}

		public override void UnlinkFrom(CGModuleSlot outputSlot)
		{
			if (HasLinkTo(outputSlot))
			{
				CGModuleLink inputLink = base.Module.GetInputLink(this, (CGModuleOutputSlot)outputSlot);
				base.Module.InputLinks.Remove(inputLink);
				CGModuleLink outputLink = outputSlot.Module.GetOutputLink((CGModuleOutputSlot)outputSlot, this);
				outputSlot.Module.OutputLinks.Remove(outputLink);
				base.LinkedSlots.Remove(outputSlot);
				outputSlot.LinkedSlots.Remove(this);
				base.UnlinkFrom(outputSlot);
			}
		}

		public CGModuleOutputSlot SourceSlot(int index = 0)
		{
			return (index >= base.Count || index < 0) ? null : ((CGModuleOutputSlot)base.LinkedSlots[index]);
		}

		public virtual bool IsValidTarget(CGModuleOutputSlot source)
		{
			InputSlotInfo inputInfo = InputInfo;
			return source.Module != base.Module && inputInfo.IsValidFrom(source.OutputInfo.DataType) && ((source.OnRequestModule != null && (inputInfo.RequestDataOnly || base.OnRequestModule != null)) || (source.OnRequestModule == null && !inputInfo.RequestDataOnly));
		}

		private CGModule SourceModule(int index)
		{
			return (index >= base.Count || index < 0) ? null : base.LinkedSlots[index].Module;
		}

		public T GetData<T>(params CGDataRequestParameter[] requests) where T : CGData
		{
			CGData[] data = GetData<T>(0, requests);
			return (data != null && data.Length != 0) ? (data[0] as T) : ((T)null);
		}

		public List<T> GetAllData<T>(params CGDataRequestParameter[] requests) where T : CGData
		{
			List<T> list = new List<T>();
			for (int i = 0; i < base.Count; i++)
			{
				CGData[] data = GetData<T>(i, requests);
				if (data != null)
				{
					if (!base.Info.Array)
					{
						list.Add(data[0] as T);
						break;
					}
					list.Capacity += data.Length;
					for (int j = 0; j < data.Length; j++)
					{
						list.Add(data[j] as T);
					}
				}
			}
			return list;
		}

		private CGData[] GetData<T>(int slotIndex, params CGDataRequestParameter[] requests) where T : CGData
		{
			CGModuleOutputSlot cGModuleOutputSlot = SourceSlot(slotIndex);
			if ((bool)cGModuleOutputSlot)
			{
				if (!cGModuleOutputSlot.Module.Active)
				{
					return new T[0];
				}
				bool flag = InputInfo.ModifiesData && (cGModuleOutputSlot.Module is IOnRequestProcessing || cGModuleOutputSlot.Count > 1);
				if (cGModuleOutputSlot.Module is IOnRequestProcessing)
				{
					bool flag2 = cGModuleOutputSlot.Data == null || cGModuleOutputSlot.Data.Length == 0;
					if (!flag2 && cGModuleOutputSlot.LastRequestParameters != null && cGModuleOutputSlot.LastRequestParameters.Length == requests.Length)
					{
						for (int i = 0; i < requests.Length; i++)
						{
							if (!requests[i].Equals(cGModuleOutputSlot.LastRequestParameters[i]))
							{
								flag2 = true;
								break;
							}
						}
					}
					else
					{
						flag2 = true;
					}
					if (flag2)
					{
						cGModuleOutputSlot.LastRequestParameters = requests;
						cGModuleOutputSlot.SetData(((IOnRequestProcessing)cGModuleOutputSlot.Module).OnSlotDataRequest(this, cGModuleOutputSlot, requests));
					}
					if (flag)
					{
						return cloneData<T>(ref cGModuleOutputSlot.Data);
					}
					return cGModuleOutputSlot.Data;
				}
				if (flag)
				{
					return cloneData<T>(ref cGModuleOutputSlot.Data);
				}
				return cGModuleOutputSlot.Data;
			}
			return new CGData[0];
		}

		private CGData[] cloneData<T>(ref CGData[] source) where T : CGData
		{
			T[] array = new T[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				array[i] = source[i].Clone<T>();
			}
			return array;
		}
	}
}
