using System;
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[ExecuteInEditMode]
	public class CGModule : DTVersionedMonoBehaviour
	{
		[Group("Events", Expanded = false, Sort = 1000)]
		[SerializeField]
		private CurvyCGEvent m_OnBeforeRefresh = new CurvyCGEvent();

		[Group("Events")]
		[SerializeField]
		private CurvyCGEvent m_OnRefresh = new CurvyCGEvent();

		[SerializeField]
		[HideInInspector]
		private string m_ModuleName;

		[SerializeField]
		[HideInInspector]
		private bool m_Active = true;

		[Group("Seed Options", Expanded = false, Sort = 1001)]
		[GroupCondition("usesRandom")]
		[FieldAction("CBSeedOptions", ActionAttribute.ActionEnum.Callback, ShowBelowProperty = true)]
		[SerializeField]
		private bool m_RandomizeSeed;

		[SerializeField]
		[HideInInspector]
		private int m_Seed = (int)DateTime.Now.Ticks;

		[NonSerialized]
		public List<string> UIMessages = new List<string>();

		private CurvyGenerator mGenerator;

		[SerializeField]
		[HideInInspector]
		private int m_UniqueID;

		internal int SortAncestors;

		[HideInInspector]
		public CGModuleProperties Properties = new CGModuleProperties();

		[HideInInspector]
		public List<CGModuleLink> InputLinks = new List<CGModuleLink>();

		[HideInInspector]
		public List<CGModuleLink> OutputLinks = new List<CGModuleLink>();

		private ModuleInfoAttribute mInfo;

		private bool mDirty = true;

		private bool mInitialized;

		private bool mIsConfiguredInternal;

		private bool mStateChangeDirty;

		private bool mLastIsConfiguredState;

		public CurvyCGEvent OnBeforeRefresh
		{
			get
			{
				return m_OnBeforeRefresh;
			}
			set
			{
				if (m_OnBeforeRefresh != value)
				{
					m_OnBeforeRefresh = value;
				}
			}
		}

		public CurvyCGEvent OnRefresh
		{
			get
			{
				return m_OnRefresh;
			}
			set
			{
				if (m_OnRefresh != value)
				{
					m_OnRefresh = value;
				}
			}
		}

		public string ModuleName
		{
			get
			{
				return base.name;
			}
			set
			{
				if (base.name != value)
				{
					base.name = value;
					renameManagedResourcesINTERNAL();
				}
			}
		}

		public bool Active
		{
			get
			{
				return m_Active;
			}
			set
			{
				if (m_Active != value)
				{
					m_Active = value;
					Dirty = true;
					Generator.sortModulesINTERNAL();
				}
			}
		}

		public int Seed
		{
			get
			{
				return m_Seed;
			}
			set
			{
				if (m_Seed != value)
				{
					m_Seed = value;
				}
				Dirty = true;
			}
		}

		public bool RandomizeSeed
		{
			get
			{
				return m_RandomizeSeed;
			}
			set
			{
				if (m_RandomizeSeed != value)
				{
					m_RandomizeSeed = value;
				}
			}
		}

		public CurvyGenerator Generator
		{
			get
			{
				return mGenerator;
			}
		}

		public int UniqueID
		{
			get
			{
				return m_UniqueID;
			}
		}

		public bool CircularReferenceError { get; set; }

		public Dictionary<string, CGModuleInputSlot> InputByName { get; private set; }

		public Dictionary<string, CGModuleOutputSlot> OutputByName { get; private set; }

		public List<CGModuleInputSlot> Input { get; private set; }

		public List<CGModuleOutputSlot> Output { get; private set; }

		public ModuleInfoAttribute Info
		{
			get
			{
				if (mInfo == null)
				{
					mInfo = getInfo();
				}
				return mInfo;
			}
		}

		public bool Dirty
		{
			get
			{
				return mDirty;
			}
			set
			{
				if (mDirty != value)
				{
					mDirty = value;
				}
				if (mDirty)
				{
					setTreeDirtyState();
				}
				if (!(this is IOnRequestProcessing) && !(this is INoProcessing))
				{
					return;
				}
				mDirty = false;
				if (Output != null)
				{
					for (int i = 0; i < Output.Count; i++)
					{
						Output[i].LastRequestParameters = null;
					}
				}
			}
		}

		public virtual bool IsConfigured
		{
			get
			{
				if (!IsInitialized || CircularReferenceError || !Active)
				{
					mIsConfiguredInternal = false;
					return false;
				}
				int num = 0;
				for (int i = 0; i < Input.Count; i++)
				{
					InputSlotInfo inputInfo = Input[i].InputInfo;
					if (Input[i].IsLinked)
					{
						for (int j = 0; j < Input[i].Count; j++)
						{
							if (Input[i].SourceSlot(j) != null)
							{
								if (Input[i].SourceSlot(j).Module.IsConfigured)
								{
									num++;
								}
								else if (!inputInfo.Optional)
								{
									mIsConfiguredInternal = false;
									return false;
								}
							}
						}
					}
					else if (inputInfo == null || !inputInfo.Optional)
					{
						mIsConfiguredInternal = false;
						return false;
					}
				}
				mIsConfiguredInternal = num > 0 || Input.Count == 0;
				return mIsConfiguredInternal;
			}
		}

		public virtual bool IsInitialized
		{
			get
			{
				return mInitialized;
			}
		}

		protected CurvyCGEventArgs OnBeforeRefreshEvent(CurvyCGEventArgs e)
		{
			if (OnBeforeRefresh != null)
			{
				OnBeforeRefresh.Invoke(e);
			}
			return e;
		}

		protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
		{
			if (OnRefresh != null)
			{
				OnRefresh.Invoke(e);
			}
			return e;
		}

		protected virtual void Awake()
		{
			mGenerator = GetComponentInParent<CurvyGenerator>();
		}

		protected virtual void OnEnable()
		{
			if ((bool)mGenerator)
			{
				Initialize();
				Generator.sortModulesINTERNAL();
			}
		}

		public void Initialize()
		{
			if (!mGenerator)
			{
				mGenerator = GetComponentInParent<CurvyGenerator>();
			}
			if (!mGenerator)
			{
				Invoke("Delete", 0f);
				return;
			}
			mInfo = getInfo();
			CheckForVersionUpgrade();
			if (string.IsNullOrEmpty(ModuleName))
			{
				if (string.IsNullOrEmpty(Info.ModuleName))
				{
					ModuleName = Generator.getUniqueModuleNameINTERNAL(Info.MenuName.Substring(Info.MenuName.LastIndexOf("/") + 1));
				}
				else
				{
					ModuleName = Generator.getUniqueModuleNameINTERNAL(Info.ModuleName);
				}
			}
			loadSlots();
			mInitialized = true;
		}

		protected virtual void OnDisable()
		{
		}

		protected virtual void OnDestroy()
		{
			bool flag = true;
			setTreeDirtyStateChange();
			List<Component> components;
			List<string> componentNames;
			if (flag && GetManagedResources(out components, out componentNames))
			{
				for (int num = components.Count - 1; num >= 0; num--)
				{
					DeleteManagedResource(componentNames[num], components[num], string.Empty, true);
				}
			}
			List<CGModuleInputSlot> inputSlots = GetInputSlots();
			List<CGModuleOutputSlot> outputSlots = GetOutputSlots();
			foreach (CGModuleInputSlot item in inputSlots)
			{
				item.ReInitializeLinkedTargetModules();
			}
			foreach (CGModuleOutputSlot item2 in outputSlots)
			{
				item2.ReInitializeLinkedTargetModules();
			}
			if ((bool)Generator)
			{
				Generator.ModulesByID.Remove(UniqueID);
				Generator.Modules.Remove(this);
				Generator.sortModulesINTERNAL();
			}
			mInitialized = false;
		}

		private void OnDidApplyAnimationProperties()
		{
			Dirty = true;
		}

		public virtual void Refresh()
		{
			UIMessages.Clear();
		}

		public virtual void Reset()
		{
			ModuleName = ((!string.IsNullOrEmpty(Info.ModuleName)) ? Info.ModuleName : GetType().Name);
		}

		public void ReInitializeLinkedSlots()
		{
			List<CGModuleInputSlot> inputSlots = GetInputSlots();
			List<CGModuleOutputSlot> outputSlots = GetOutputSlots();
			for (int i = 0; i < inputSlots.Count; i++)
			{
				inputSlots[i].ReInitializeLinkedSlots();
			}
			for (int j = 0; j < outputSlots.Count; j++)
			{
				outputSlots[j].ReInitializeLinkedSlots();
			}
		}

		public virtual void OnStateChange()
		{
			Dirty = true;
			if (Output != null)
			{
				for (int i = 0; i < Output.Count; i++)
				{
					Output[i].ClearData();
				}
			}
		}

		public virtual void OnTemplateCreated()
		{
		}

		protected T GetRequestParameter<T>(ref CGDataRequestParameter[] requests) where T : CGDataRequestParameter
		{
			for (int i = 0; i < requests.Length; i++)
			{
				if (requests[i] is T)
				{
					return (T)requests[i];
				}
			}
			return (T)null;
		}

		protected void RemoveRequestParameter(ref CGDataRequestParameter[] requests, CGDataRequestParameter request)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				if (requests[i] == request)
				{
					requests = requests.RemoveAt(i);
					break;
				}
			}
		}

		public CGModuleLink GetOutputLink(CGModuleOutputSlot outSlot, CGModuleInputSlot inSlot)
		{
			return GetLink(OutputLinks, outSlot, inSlot);
		}

		public List<CGModuleLink> GetOutputLinks(CGModuleOutputSlot outSlot)
		{
			return GetLinks(OutputLinks, outSlot);
		}

		public CGModuleLink GetInputLink(CGModuleInputSlot inSlot, CGModuleOutputSlot outSlot)
		{
			return GetLink(InputLinks, inSlot, outSlot);
		}

		public List<CGModuleLink> GetInputLinks(CGModuleInputSlot inSlot)
		{
			return GetLinks(InputLinks, inSlot);
		}

		private CGModuleLink GetLink(List<CGModuleLink> lst, CGModuleSlot source, CGModuleSlot target)
		{
			for (int i = 0; i < lst.Count; i++)
			{
				if (lst[i].IsSame(source, target))
				{
					return lst[i];
				}
			}
			return null;
		}

		private List<CGModuleLink> GetLinks(List<CGModuleLink> lst, CGModuleSlot source)
		{
			List<CGModuleLink> list = new List<CGModuleLink>();
			for (int i = 0; i < lst.Count; i++)
			{
				if (lst[i].IsFrom(source))
				{
					list.Add(lst[i]);
				}
			}
			return list;
		}

		public CGModule CopyTo(CurvyGenerator targetGenerator)
		{
			CGModule cGModule = this.DuplicateGameObject<CGModule>(targetGenerator.transform);
			cGModule.mGenerator = targetGenerator;
			cGModule.Initialize();
			cGModule.ModuleName = ModuleName;
			cGModule.ModuleName = targetGenerator.getUniqueModuleNameINTERNAL(cGModule.ModuleName);
			cGModule.SetUniqueIdINTERNAL();
			cGModule.renameManagedResourcesINTERNAL();
			return cGModule;
		}

		public Component AddManagedResource(string resourceName, string context = "", int index = -1)
		{
			Component component = CGResourceHandler.CreateResource(this, resourceName, context);
			RenameResource(resourceName + context, component, index);
			component.transform.SetParent(base.transform);
			return component;
		}

		public void DeleteManagedResource(string resourceName, Component res, string context = "", bool dontUsePool = false)
		{
			if ((bool)res)
			{
				CGResourceHandler.DestroyResource(this, resourceName, res, context, dontUsePool);
			}
		}

		public bool IsManagedResource(Component res)
		{
			return (bool)res && res.transform.parent == base.transform;
		}

		protected void RenameResource(string resourceName, Component resource, int index = -1)
		{
			resource.name = string.Format("{0}_{1}_{2}", ModuleName, UniqueID, resourceName);
			if (index > -1)
			{
				resource.name += string.Format("{0:000}", index);
			}
		}

		protected PrefabPool GetPrefabPool(GameObject prefab)
		{
			return Generator.PoolManager.GetPrefabPool(UniqueID + "_" + prefab.name, prefab);
		}

		public List<IPool> GetAllPrefabPools()
		{
			return Generator.PoolManager.FindPools(UniqueID + "_");
		}

		public void DeleteAllPrefabPools()
		{
			Generator.PoolManager.DeletePools(UniqueID + "_");
		}

		public void Delete()
		{
			OnStateChange();
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public CGModuleInputSlot GetInputSlot(string name)
		{
			return (InputByName == null || !InputByName.ContainsKey(name)) ? null : InputByName[name];
		}

		public List<CGModuleInputSlot> GetInputSlots(Type filterType = null)
		{
			if (filterType == null)
			{
				return new List<CGModuleInputSlot>(Input);
			}
			List<CGModuleInputSlot> list = new List<CGModuleInputSlot>();
			for (int i = 0; i < Output.Count; i++)
			{
				if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
				{
					list.Add(Input[i]);
				}
			}
			return list;
		}

		public CGModuleOutputSlot GetOutputSlot(string name)
		{
			return (OutputByName == null || !OutputByName.ContainsKey(name)) ? null : OutputByName[name];
		}

		public List<CGModuleOutputSlot> GetOutputSlots(Type filterType = null)
		{
			if (filterType == null)
			{
				return new List<CGModuleOutputSlot>(Output);
			}
			List<CGModuleOutputSlot> list = new List<CGModuleOutputSlot>();
			for (int i = 0; i < Output.Count; i++)
			{
				if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
				{
					list.Add(Output[i]);
				}
			}
			return list;
		}

		public bool GetManagedResources(out List<Component> components, out List<string> componentNames)
		{
			components = new List<Component>();
			componentNames = new List<string>();
			FieldInfo[] allFields = GetType().GetAllFields(false, true);
			FieldInfo[] array = allFields;
			foreach (FieldInfo fieldInfo in array)
			{
				CGResourceManagerAttribute customAttribute = fieldInfo.GetCustomAttribute<CGResourceManagerAttribute>();
				if (customAttribute == null)
				{
					continue;
				}
				if (typeof(ICGResourceCollection).IsAssignableFrom(fieldInfo.FieldType))
				{
					ICGResourceCollection iCGResourceCollection = fieldInfo.GetValue(this) as ICGResourceCollection;
					if (iCGResourceCollection == null)
					{
						continue;
					}
					Component[] itemsArray = iCGResourceCollection.ItemsArray;
					Component[] array2 = itemsArray;
					foreach (Component component in array2)
					{
						if (component.transform.parent == base.transform)
						{
							components.Add(component);
							componentNames.Add(customAttribute.ResourceName);
						}
					}
				}
				else
				{
					Component component2 = fieldInfo.GetValue(this) as Component;
					if ((bool)component2 && component2.transform.parent == base.transform)
					{
						components.Add(component2);
						componentNames.Add(customAttribute.ResourceName);
					}
				}
			}
			return components.Count > 0;
		}

		public int SetUniqueIdINTERNAL()
		{
			m_UniqueID = ++Generator.m_LastModuleID;
			return m_UniqueID;
		}

		internal void initializeSort()
		{
			SortAncestors = 0;
			CircularReferenceError = false;
			for (int i = 0; i < Input.Count; i++)
			{
				if (Input[i].IsLinked)
				{
					SortAncestors++;
				}
			}
		}

		internal List<CGModule> decrementChilds()
		{
			List<CGModule> list = new List<CGModule>();
			for (int i = 0; i < Output.Count; i++)
			{
				for (int j = 0; j < Output[i].LinkedSlots.Count; j++)
				{
					if (--Output[i].LinkedSlots[j].Module.SortAncestors == 0)
					{
						list.Add(Output[i].LinkedSlots[j].Module);
					}
				}
			}
			return list;
		}

		internal void doRefresh()
		{
			if (RandomizeSeed)
			{
				setSeed((int)DateTime.Now.Ticks);
			}
			else
			{
				setSeed(Seed);
			}
			OnBeforeRefreshEvent(new CurvyCGEventArgs(this));
			Refresh();
			setSeed((int)DateTime.Now.Ticks);
			OnRefreshEvent(new CurvyCGEventArgs(this));
			mDirty = false;
		}

		private void setSeed(int seed)
		{
			UnityEngine.Random.InitState(seed);
		}

		internal ModuleInfoAttribute getInfo()
		{
			object[] customAttributes = GetType().GetCustomAttributes(typeof(ModuleInfoAttribute), true);
			return (customAttributes.Length <= 0) ? null : ((ModuleInfoAttribute)customAttributes[0]);
		}

		private bool usesRandom()
		{
			return (Info != null) & Info.UsesRandom;
		}

		private void loadSlots()
		{
			InputByName = new Dictionary<string, CGModuleInputSlot>();
			OutputByName = new Dictionary<string, CGModuleOutputSlot>();
			Input = new List<CGModuleInputSlot>();
			Output = new List<CGModuleOutputSlot>();
			FieldInfo[] allFields = GetType().GetAllFields();
			FieldInfo[] array = allFields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.FieldType == typeof(CGModuleInputSlot))
				{
					CGModuleInputSlot cGModuleInputSlot = (CGModuleInputSlot)fieldInfo.GetValue(this);
					cGModuleInputSlot.Module = this;
					cGModuleInputSlot.Info = getSlotInfo(fieldInfo);
					cGModuleInputSlot.ReInitializeLinkedSlots();
					InputByName.Add(cGModuleInputSlot.Info.Name, cGModuleInputSlot);
					Input.Add(cGModuleInputSlot);
				}
				else if (fieldInfo.FieldType == typeof(CGModuleOutputSlot))
				{
					CGModuleOutputSlot cGModuleOutputSlot = (CGModuleOutputSlot)fieldInfo.GetValue(this);
					cGModuleOutputSlot.Module = this;
					cGModuleOutputSlot.Info = getSlotInfo(fieldInfo);
					cGModuleOutputSlot.ReInitializeLinkedSlots();
					OutputByName.Add(cGModuleOutputSlot.Info.Name, cGModuleOutputSlot);
					Output.Add(cGModuleOutputSlot);
				}
			}
		}

		private SlotInfo getSlotInfo(FieldInfo f)
		{
			SlotInfo customAttribute = f.GetCustomAttribute<SlotInfo>();
			if (customAttribute != null)
			{
				if (string.IsNullOrEmpty(customAttribute.Name))
				{
					customAttribute.Name = f.Name.TrimStart("In").TrimStart("Out");
				}
				for (int i = 0; i < customAttribute.DataTypes.Length; i++)
				{
					if (!customAttribute.DataTypes[i].IsSubclassOf(typeof(CGData)))
					{
						Debug.LogError(string.Format("{0}, Slot '{1}': Data type needs to be subclass of CGData!", GetType().Name, customAttribute.Name));
					}
				}
				return customAttribute;
			}
			Debug.LogError("The Slot '" + f.Name + "' of type '" + f.DeclaringType.Name + "' needs a SlotInfo attribute!");
			return null;
		}

		private void setTreeDirtyStateChange()
		{
			mStateChangeDirty = true;
			if (Output == null)
			{
				return;
			}
			for (int i = 0; i < Output.Count; i++)
			{
				if (!Output[i].IsLinked)
				{
					continue;
				}
				List<CGModule> linkedModules = Output[i].GetLinkedModules();
				for (int j = 0; j < linkedModules.Count; j++)
				{
					if (linkedModules[j] != this || linkedModules[j].CircularReferenceError)
					{
						linkedModules[j].setTreeDirtyStateChange();
					}
				}
			}
		}

		private void setTreeDirtyState()
		{
			bool isConfigured = IsConfigured;
			if (mLastIsConfiguredState != isConfigured)
			{
				mStateChangeDirty = true;
			}
			mLastIsConfiguredState = isConfigured;
			if (Output == null)
			{
				return;
			}
			for (int i = 0; i < Output.Count; i++)
			{
				if (!Output[i].IsLinked)
				{
					continue;
				}
				List<CGModule> linkedModules = Output[i].GetLinkedModules();
				for (int j = 0; j < linkedModules.Count; j++)
				{
					if (linkedModules[j] != this || linkedModules[j].CircularReferenceError)
					{
						linkedModules[j].Dirty = true;
					}
				}
			}
		}

		public void checkOnStateChangedINTERNAL()
		{
			if (mStateChangeDirty)
			{
				OnStateChange();
			}
			mStateChangeDirty = false;
		}

		public void renameManagedResourcesINTERNAL()
		{
			FieldInfo[] allFields = GetType().GetAllFields(false, true);
			FieldInfo[] array = allFields;
			foreach (FieldInfo fieldInfo in array)
			{
				CGResourceManagerAttribute customAttribute = fieldInfo.GetCustomAttribute<CGResourceManagerAttribute>();
				if (customAttribute != null)
				{
					Component component = fieldInfo.GetValue(this) as Component;
					if ((bool)component && component.transform.parent == base.transform)
					{
						RenameResource(customAttribute.ResourceName, component);
					}
				}
			}
		}
	}
}
