using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGModuleSlot
	{
		protected List<CGModuleSlot> mLinkedSlots;

		public CGModule Module { get; internal set; }

		public SlotInfo Info { get; internal set; }

		public Vector2 Origin { get; set; }

		public Rect DropZone { get; set; }

		public bool IsLinked
		{
			get
			{
				return LinkedSlots != null && LinkedSlots.Count > 0;
			}
		}

		public bool IsLinkedAndConfigured
		{
			get
			{
				if (!IsLinked)
				{
					return false;
				}
				for (int i = 0; i < LinkedSlots.Count; i++)
				{
					if (!LinkedSlots[i].Module.IsConfigured)
					{
						return false;
					}
				}
				return true;
			}
		}

		public IOnRequestProcessing OnRequestModule
		{
			get
			{
				return Module as IOnRequestProcessing;
			}
		}

		public IOnRequestPath OnRequestPathModule
		{
			get
			{
				return Module as IOnRequestPath;
			}
		}

		public IExternalInput ExternalInput
		{
			get
			{
				return Module as IExternalInput;
			}
		}

		public List<CGModuleSlot> LinkedSlots
		{
			get
			{
				if (mLinkedSlots == null)
				{
					LoadLinkedSlots();
				}
				return mLinkedSlots ?? new List<CGModuleSlot>();
			}
		}

		public int Count
		{
			get
			{
				return LinkedSlots.Count;
			}
		}

		public string Name
		{
			get
			{
				return (Info == null) ? string.Empty : Info.Name;
			}
		}

		public bool HasLinkTo(CGModuleSlot other)
		{
			for (int i = 0; i < LinkedSlots.Count; i++)
			{
				if (LinkedSlots[i] == other)
				{
					return true;
				}
			}
			return false;
		}

		public List<CGModule> GetLinkedModules()
		{
			List<CGModule> list = new List<CGModule>();
			for (int i = 0; i < LinkedSlots.Count; i++)
			{
				list.Add(LinkedSlots[i].Module);
			}
			return list;
		}

		public virtual void LinkTo(CGModuleSlot other)
		{
			if ((bool)Module)
			{
				Module.Generator.sortModulesINTERNAL();
				Module.Dirty = true;
			}
			if ((bool)other.Module)
			{
				other.Module.Dirty = true;
			}
		}

		public virtual void UnlinkFrom(CGModuleSlot other)
		{
			if ((bool)Module)
			{
				Module.Generator.sortModulesINTERNAL();
				Module.Dirty = true;
			}
			if ((bool)other.Module)
			{
				other.Module.Dirty = true;
			}
		}

		public virtual void UnlinkAll()
		{
		}

		public void ReInitializeLinkedSlots()
		{
			mLinkedSlots = null;
		}

		public void ReInitializeLinkedTargetModules()
		{
			List<CGModule> linkedModules = GetLinkedModules();
			foreach (CGModule item in linkedModules)
			{
				if (item != null)
				{
					item.ReInitializeLinkedSlots();
				}
			}
		}

		protected virtual void LoadLinkedSlots()
		{
		}

		public static implicit operator bool(CGModuleSlot a)
		{
			return !object.ReferenceEquals(a, null);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}.{2}", GetType().Name, Module.name, Name);
		}
	}
}
