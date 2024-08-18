using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class PoolManager : MonoBehaviour
	{
		[Section("General", true, false, 100)]
		[SerializeField]
		private bool m_AutoCreatePools = true;

		[AsGroup(null, Expanded = false)]
		[SerializeField]
		private PoolSettings m_DefaultSettings = new PoolSettings();

		public Dictionary<string, IPool> Pools = new Dictionary<string, IPool>();

		public Dictionary<Type, IPool> TypePools = new Dictionary<Type, IPool>();

		private IPool[] mPools = new IPool[0];

		public bool AutoCreatePools
		{
			get
			{
				return m_AutoCreatePools;
			}
			set
			{
				if (m_AutoCreatePools != value)
				{
					m_AutoCreatePools = value;
				}
			}
		}

		public PoolSettings DefaultSettings
		{
			get
			{
				return m_DefaultSettings;
			}
			set
			{
				if (m_DefaultSettings != value)
				{
					m_DefaultSettings = value;
				}
				m_DefaultSettings.OnValidate();
			}
		}

		public bool IsInitialized { get; private set; }

		public int Count
		{
			get
			{
				return Pools.Count + TypePools.Count;
			}
		}

		private void OnDisable()
		{
			IsInitialized = false;
		}

		private void Update()
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			if (mPools.Length != TypePools.Count)
			{
				Array.Resize(ref mPools, TypePools.Count);
				TypePools.Values.CopyTo(mPools, 0);
			}
			for (int i = 0; i < mPools.Length; i++)
			{
				mPools[i].Update();
			}
		}

		private void Initialize()
		{
			Pools.Clear();
			IPool[] components = GetComponents<IPool>();
			IPool[] array = components;
			foreach (IPool pool in array)
			{
				pool.Identifier = GetUniqueIdentifier(pool.Identifier);
				Pools.Add(pool.Identifier, pool);
			}
			IsInitialized = true;
		}

		public string GetUniqueIdentifier(string ident)
		{
			int num = 0;
			string text = ident;
			while (Pools.ContainsKey(text))
			{
				int num2 = ++num;
				text = ident + num2;
			}
			return text;
		}

		public Pool<T> GetTypePool<T>()
		{
			IPool value = null;
			if (!TypePools.TryGetValue(typeof(T), out value) && AutoCreatePools)
			{
				value = CreateTypePool<T>();
			}
			return (Pool<T>)value;
		}

		public ComponentPool GetComponentPool<T>() where T : Component
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			IPool value = null;
			if (!Pools.TryGetValue(typeof(T).AssemblyQualifiedName, out value) && AutoCreatePools)
			{
				value = CreateComponentPool<T>();
			}
			return (ComponentPool)value;
		}

		public PrefabPool GetPrefabPool(string identifier, params GameObject[] prefabs)
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			IPool value;
			if (!Pools.TryGetValue(identifier, out value) && AutoCreatePools)
			{
				value = CreatePrefabPool(identifier, null, prefabs);
			}
			return (PrefabPool)value;
		}

		public Pool<T> CreateTypePool<T>(PoolSettings settings = null)
		{
			PoolSettings settings2 = settings ?? new PoolSettings(DefaultSettings);
			IPool value = null;
			if (!TypePools.TryGetValue(typeof(T), out value))
			{
				value = new Pool<T>(settings2);
				TypePools.Add(typeof(T), value);
			}
			return (Pool<T>)value;
		}

		public ComponentPool CreateComponentPool<T>(PoolSettings settings = null) where T : Component
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			PoolSettings settings2 = settings ?? new PoolSettings(DefaultSettings);
			IPool value = null;
			if (!Pools.TryGetValue(typeof(T).AssemblyQualifiedName, out value))
			{
				value = base.gameObject.AddComponent<ComponentPool>();
				((ComponentPool)value).Initialize(typeof(T), settings2);
				Pools.Add(value.Identifier, value);
			}
			return (ComponentPool)value;
		}

		public PrefabPool CreatePrefabPool(string name, PoolSettings settings = null, params GameObject[] prefabs)
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			PoolSettings settings2 = settings ?? new PoolSettings(DefaultSettings);
			IPool value = null;
			if (!Pools.TryGetValue(name, out value))
			{
				PrefabPool prefabPool = base.gameObject.AddComponent<PrefabPool>();
				prefabPool.Initialize(name, settings2, prefabs);
				Pools.Add(name, prefabPool);
				return prefabPool;
			}
			return (PrefabPool)value;
		}

		public List<IPool> FindPools(string identifierStartsWith)
		{
			List<IPool> list = new List<IPool>();
			foreach (KeyValuePair<string, IPool> pool in Pools)
			{
				if (pool.Key.StartsWith(identifierStartsWith))
				{
					list.Add(pool.Value);
				}
			}
			return list;
		}

		public void DeletePools(string startsWith)
		{
			List<IPool> list = FindPools(startsWith);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				DeletePool(list[num]);
			}
		}

		public void DeletePool(IPool pool)
		{
			if (pool is PrefabPool || pool is ComponentPool)
			{
				UnityEngine.Object.Destroy((MonoBehaviour)pool);
				Pools.Remove(pool.Identifier);
			}
		}

		public void DeletePool<T>()
		{
			TypePools.Remove(typeof(T));
		}
	}
}
