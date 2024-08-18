using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGModuleOutputSlot : CGModuleSlot
	{
		public CGData[] Data = new CGData[0];

		public CGDataRequestParameter[] LastRequestParameters;

		public OutputSlotInfo OutputInfo
		{
			get
			{
				return base.Info as OutputSlotInfo;
			}
		}

		public bool HasData
		{
			get
			{
				return Data != null && Data.Length > 0 && Data[0] != null;
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
			List<CGModuleLink> outputLinks = base.Module.GetOutputLinks(this);
			foreach (CGModuleLink item in outputLinks)
			{
				CGModule module = base.Module.Generator.GetModule(item.TargetModuleID, true);
				if ((bool)module)
				{
					CGModuleInputSlot cGModuleInputSlot = module.InputByName[item.TargetSlotName];
					if (!cGModuleInputSlot.Module.GetInputLink(cGModuleInputSlot, this))
					{
						cGModuleInputSlot.Module.InputLinks.Add(new CGModuleLink(cGModuleInputSlot, this));
						cGModuleInputSlot.ReInitializeLinkedSlots();
					}
					if (!mLinkedSlots.Contains(cGModuleInputSlot))
					{
						mLinkedSlots.Add(cGModuleInputSlot);
					}
				}
				else
				{
					base.Module.OutputLinks.Remove(item);
				}
			}
		}

		public override void LinkTo(CGModuleSlot inputSlot)
		{
			if (!HasLinkTo(inputSlot))
			{
				if (!inputSlot.Info.Array && inputSlot.IsLinked)
				{
					inputSlot.UnlinkAll();
				}
				base.Module.OutputLinks.Add(new CGModuleLink(this, inputSlot));
				inputSlot.Module.InputLinks.Add(new CGModuleLink(inputSlot, this));
				if (!base.LinkedSlots.Contains(inputSlot))
				{
					base.LinkedSlots.Add(inputSlot);
				}
				if (!inputSlot.LinkedSlots.Contains(this))
				{
					inputSlot.LinkedSlots.Add(this);
				}
				base.LinkTo(inputSlot);
			}
		}

		public override void UnlinkFrom(CGModuleSlot inputSlot)
		{
			if (HasLinkTo(inputSlot))
			{
				CGModuleLink outputLink = base.Module.GetOutputLink(this, (CGModuleInputSlot)inputSlot);
				base.Module.OutputLinks.Remove(outputLink);
				CGModuleLink inputLink = inputSlot.Module.GetInputLink((CGModuleInputSlot)inputSlot, this);
				inputSlot.Module.InputLinks.Remove(inputLink);
				base.LinkedSlots.Remove(inputSlot);
				inputSlot.LinkedSlots.Remove(this);
				base.UnlinkFrom(inputSlot);
			}
		}

		public void ClearData()
		{
			Data = new CGData[0];
		}

		public void SetData<T>(List<T> data) where T : CGData
		{
			if (data == null)
			{
				Data = new CGData[0];
				return;
			}
			if (!base.Info.Array && data.Count > 1)
			{
				Debug.LogWarning("[Curvy] " + base.Module.GetType().Name + " (" + base.Info.Name + ") only supports a single data item! Either avoid calculating unneccessary data or define the slot as an array!");
			}
			Data = data.ToArray();
		}

		public void SetData(params CGData[] data)
		{
			Data = ((data != null) ? data : new CGData[0]);
		}

		public T GetData<T>() where T : CGData
		{
			return (Data.Length != 0) ? (Data[0] as T) : ((T)null);
		}

		public T[] GetAllData<T>() where T : CGData
		{
			return Data as T[];
		}
	}
}
