using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[ExecuteInEditMode]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/generator")]
	[AddComponentMenu("Curvy/Generator", 3)]
	[RequireComponent(typeof(PoolManager))]
	public class CurvyGenerator : DTVersionedMonoBehaviour
	{
		[SerializeField]
		[RangeEx(0f, 1f, "Min Distance", "Minimum distance between rasterized sample points")]
		private float m_MinDistance = 0.1f;

		[Tooltip("Show Debug Output?")]
		[SerializeField]
		private bool m_ShowDebug;

		[SerializeField]
		private bool m_AutoRefresh = true;

		[FieldCondition("m_AutoRefresh", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Positive(Tooltip = "Refresh delay (ms)")]
		[SerializeField]
		private int m_RefreshDelay;

		[FieldCondition("m_AutoRefresh", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Positive(Tooltip = "Refresh delay (ms)")]
		[SerializeField]
		private int m_RefreshDelayEditor = 10;

		[Section("Events", false, false, 1000, HelpURL = "http://www.fluffyunderware.com/curvy/doclink/210/generator_events")]
		[SerializeField]
		private CurvyCGEvent m_OnRefresh = new CurvyCGEvent();

		[HideInInspector]
		public List<CGModule> Modules = new List<CGModule>();

		[SerializeField]
		[HideInInspector]
		internal int m_LastModuleID;

		public Dictionary<int, CGModule> ModulesByID = new Dictionary<int, CGModule>();

		private bool mInitialized;

		private bool mInitializedPhaseOne;

		private bool mNeedSort = true;

		private double mLastUpdateTime;

		private PoolManager mPoolManager;

		public float MinDistance
		{
			get
			{
				return m_MinDistance;
			}
			set
			{
				if (m_MinDistance != value)
				{
					m_MinDistance = Mathf.Max(0.0001f, value);
				}
				if (IsInitialized)
				{
					Refresh(true);
				}
			}
		}

		public bool ShowDebug
		{
			get
			{
				return m_ShowDebug;
			}
			set
			{
				if (m_ShowDebug != value)
				{
					m_ShowDebug = value;
				}
			}
		}

		public bool AutoRefresh
		{
			get
			{
				return m_AutoRefresh;
			}
			set
			{
				if (m_AutoRefresh != value)
				{
					m_AutoRefresh = value;
				}
			}
		}

		public int RefreshDelay
		{
			get
			{
				return m_RefreshDelay;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_RefreshDelay != num)
				{
					m_RefreshDelay = num;
				}
			}
		}

		public int RefreshDelayEditor
		{
			get
			{
				return m_RefreshDelayEditor;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_RefreshDelayEditor != num)
				{
					m_RefreshDelayEditor = num;
				}
			}
		}

		public PoolManager PoolManager
		{
			get
			{
				if (mPoolManager == null)
				{
					mPoolManager = GetComponent<PoolManager>();
				}
				return mPoolManager;
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

		public bool IsInitialized
		{
			get
			{
				return mInitialized;
			}
		}

		public bool Destroying { get; private set; }

		private void Awake()
		{
		}

		private void OnEnable()
		{
			PoolManager.AutoCreatePools = true;
		}

		private void OnDisable()
		{
			mInitialized = false;
			mInitializedPhaseOne = false;
			mNeedSort = true;
		}

		private void OnDestroy()
		{
			Destroying = true;
		}

		private void Update()
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			else if (Application.isPlaying && AutoRefresh && DTTime.TimeSinceStartup - mLastUpdateTime > (double)((float)RefreshDelay * 0.001f))
			{
				mLastUpdateTime = DTTime.TimeSinceStartup;
				Refresh();
			}
		}

		public static CurvyGenerator Create()
		{
			GameObject gameObject = new GameObject("Curvy Generator", typeof(CurvyGenerator));
			return gameObject.GetComponent<CurvyGenerator>();
		}

		public T AddModule<T>() where T : CGModule
		{
			return (T)AddModule(typeof(T));
		}

		public CGModule AddModule(Type type)
		{
			GameObject gameObject = new GameObject(string.Empty);
			gameObject.transform.SetParent(base.transform, false);
			CGModule cGModule = (CGModule)gameObject.AddComponent(type);
			cGModule.SetUniqueIdINTERNAL();
			Modules.Add(cGModule);
			ModulesByID.Add(cGModule.UniqueID, cGModule);
			return cGModule;
		}

		public void ArrangeModules()
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			foreach (CGModule module in Modules)
			{
				vector.x = Mathf.Min(module.Properties.Dimensions.x, vector.x);
				vector.y = Mathf.Min(module.Properties.Dimensions.y, vector.y);
			}
			vector -= new Vector2(10f, 10f);
			foreach (CGModule module2 in Modules)
			{
				module2.Properties.Dimensions.x -= vector.x;
				module2.Properties.Dimensions.y -= vector.y;
			}
		}

		public void Clear()
		{
			clearModules();
		}

		public void DeleteModule(CGModule module)
		{
			if ((bool)module)
			{
				module.Delete();
			}
		}

		public List<T> FindModules<T>(bool includeOnRequestProcessing = false) where T : CGModule
		{
			List<T> list = new List<T>();
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i] is T && (includeOnRequestProcessing || !(Modules[i] is IOnRequestProcessing)))
				{
					list.Add((T)Modules[i]);
				}
			}
			return list;
		}

		public List<CGModule> GetModules(bool includeOnRequestProcessing = false)
		{
			if (includeOnRequestProcessing)
			{
				return new List<CGModule>(Modules);
			}
			List<CGModule> list = new List<CGModule>();
			for (int i = 0; i < Modules.Count; i++)
			{
				if (!(Modules[i] is IOnRequestProcessing))
				{
					list.Add(Modules[i]);
				}
			}
			return list;
		}

		public CGModule GetModule(int moduleID, bool includeOnRequestProcessing = false)
		{
			CGModule value;
			if (ModulesByID.TryGetValue(moduleID, out value) && (includeOnRequestProcessing || !(value is IOnRequestProcessing)))
			{
				return value;
			}
			return null;
		}

		public T GetModule<T>(int moduleID, bool includeOnRequestProcessing = false) where T : CGModule
		{
			return GetModule(moduleID, includeOnRequestProcessing) as T;
		}

		public CGModule GetModule(string moduleName, bool includeOnRequestProcessing = false)
		{
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i].ModuleName.Equals(moduleName, StringComparison.CurrentCultureIgnoreCase) && (includeOnRequestProcessing || !(Modules[i] is IOnRequestProcessing)))
				{
					return Modules[i];
				}
			}
			return null;
		}

		public T GetModule<T>(string moduleName, bool includeOnRequestProcessing = false) where T : CGModule
		{
			return GetModule(moduleName, includeOnRequestProcessing) as T;
		}

		public CGModuleOutputSlot GetModuleOutputSlot(int moduleId, string slotName)
		{
			CGModule module = GetModule(moduleId);
			if ((bool)module)
			{
				return module.GetOutputSlot(slotName);
			}
			return null;
		}

		public CGModuleOutputSlot GetModuleOutputSlot(string moduleName, string slotName)
		{
			CGModule module = GetModule(moduleName);
			if ((bool)module)
			{
				return module.GetOutputSlot(slotName);
			}
			return null;
		}

		public void Initialize(bool force = false)
		{
			if (!mInitializedPhaseOne || force)
			{
				Modules = new List<CGModule>(GetComponentsInChildren<CGModule>());
				ModulesByID.Clear();
				for (int i = 0; i < Modules.Count; i++)
				{
					if (!Modules[i].IsInitialized || force)
					{
						Modules[i].Initialize();
					}
					if (ModulesByID.ContainsKey(Modules[i].UniqueID))
					{
						Debug.LogError("ID of '" + Modules[i].ModuleName + "' isn't unique!");
						return;
					}
					ModulesByID.Add(Modules[i].UniqueID, Modules[i]);
				}
				if (Modules.Count > 0)
				{
					sortModulesINTERNAL();
				}
				mInitializedPhaseOne = true;
			}
			for (int j = 0; j < Modules.Count; j++)
			{
				if (Modules[j] is IExternalInput && !Modules[j].IsInitialized)
				{
					return;
				}
			}
			mInitialized = true;
			mInitializedPhaseOne = false;
			mNeedSort = mNeedSort || force;
			Refresh(true);
		}

		public void Refresh(bool forceUpdate = false)
		{
			if (!IsInitialized)
			{
				return;
			}
			if (mNeedSort)
			{
				doSortModules();
			}
			CGModule cGModule = null;
			for (int i = 0; i < Modules.Count; i++)
			{
				if (forceUpdate && Modules[i] is IOnRequestProcessing)
				{
					Modules[i].Dirty = true;
				}
				if (Modules[i] is INoProcessing || (!Modules[i].Dirty && (!forceUpdate || Modules[i] is IOnRequestProcessing)))
				{
					continue;
				}
				Modules[i].checkOnStateChangedINTERNAL();
				if (Modules[i].IsInitialized && Modules[i].IsConfigured)
				{
					if (cGModule == null)
					{
						cGModule = Modules[i];
					}
					Modules[i].doRefresh();
				}
			}
			if (cGModule != null)
			{
				OnRefreshEvent(new CurvyCGEventArgs(this, cGModule));
			}
		}

		protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
		{
			if (OnRefresh != null)
			{
				OnRefresh.Invoke(e);
			}
			return e;
		}

		private void clearModules()
		{
			for (int num = Modules.Count - 1; num >= 0; num--)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(Modules[num].gameObject);
				}
			}
			Modules.Clear();
			ModulesByID.Clear();
			m_LastModuleID = 0;
		}

		public string getUniqueModuleNameINTERNAL(string name)
		{
			string text = name;
			int num = 1;
			bool flag;
			do
			{
				flag = true;
				foreach (CGModule module in Modules)
				{
					if (module.ModuleName.Equals(text, StringComparison.CurrentCultureIgnoreCase))
					{
						text = name + num++;
						flag = false;
						break;
					}
				}
			}
			while (!flag);
			return text;
		}

		internal void sortModulesINTERNAL()
		{
			mNeedSort = true;
		}

		private bool doSortModules()
		{
			List<CGModule> list = new List<CGModule>(Modules);
			List<CGModule> list2 = new List<CGModule>();
			List<CGModule> list3 = new List<CGModule>();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].initializeSort();
				if (list[num] is INoProcessing)
				{
					list3.Add(list[num]);
					list.RemoveAt(num);
				}
				else if (list[num].SortAncestors == 0)
				{
					list2.Add(list[num]);
					list.RemoveAt(num);
				}
			}
			Modules.Clear();
			int num2 = 0;
			while (list2.Count > 0)
			{
				CGModule cGModule = list2[0];
				list2.RemoveAt(0);
				List<CGModule> list4 = cGModule.decrementChilds();
				list2.AddRange(list4);
				for (int i = 0; i < list4.Count; i++)
				{
					list.Remove(list4[i]);
				}
				Modules.Add(cGModule);
				cGModule.transform.SetSiblingIndex(num2++);
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].CircularReferenceError = true;
			}
			Modules.AddRange(list);
			Modules.AddRange(list3);
			mNeedSort = false;
			return list.Count > 0;
		}
	}
}
