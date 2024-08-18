using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGDataReference
	{
		[SerializeField]
		private CGModule m_Module;

		[SerializeField]
		private string m_SlotName;

		private CGModuleOutputSlot mSlot;

		public CGData[] Data
		{
			get
			{
				return (Slot == null) ? new CGData[0] : Slot.Data;
			}
		}

		public CGModuleOutputSlot Slot
		{
			get
			{
				if ((mSlot == null || mSlot.Module != m_Module || mSlot.Info == null || mSlot.Info.Name != m_SlotName) && m_Module != null && m_Module.Generator != null && m_Module.Generator.IsInitialized && !string.IsNullOrEmpty(m_SlotName))
				{
					mSlot = m_Module.GetOutputSlot(m_SlotName);
				}
				return mSlot;
			}
		}

		public bool HasValue
		{
			get
			{
				return Data.Length > 0;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return string.IsNullOrEmpty(SlotName);
			}
		}

		public CGModule Module
		{
			get
			{
				return m_Module;
			}
		}

		public string SlotName
		{
			get
			{
				return m_SlotName;
			}
		}

		public CGDataReference()
		{
		}

		public CGDataReference(CGModule module, string slotName)
		{
			setINTERNAL(module, slotName);
		}

		public CGDataReference(CurvyGenerator generator, string moduleName, string slotName)
		{
			setINTERNAL(generator, moduleName, slotName);
		}

		public void Clear()
		{
			setINTERNAL(null, string.Empty);
		}

		public T GetData<T>() where T : CGData
		{
			return (Data.Length != 0) ? (Data[0] as T) : ((T)null);
		}

		public T[] GetAllData<T>() where T : CGData
		{
			return Data as T[];
		}

		public void setINTERNAL(CGModule module, string slotName)
		{
			m_Module = module;
			m_SlotName = slotName;
			mSlot = null;
		}

		public void setINTERNAL(CurvyGenerator generator, string moduleName, string slotName)
		{
			m_Module = generator.GetModule(moduleName);
			m_SlotName = slotName;
			mSlot = null;
		}
	}
}
