using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class Pool<T> : IPool
	{
		private List<T> mObjects = new List<T>();

		private double mLastTime;

		private double mDeltaTime;

		public string Identifier { get; set; }

		public PoolSettings Settings { get; protected set; }

		public Type Type
		{
			get
			{
				return typeof(T);
			}
		}

		public int Count
		{
			get
			{
				return mObjects.Count;
			}
		}

		public Pool(PoolSettings settings = null)
		{
			Settings = settings ?? new PoolSettings();
			Identifier = typeof(T).FullName;
			mLastTime = DTTime.TimeSinceStartup + (double)UnityEngine.Random.Range(0f, Settings.Speed);
			if (Settings.Prewarm)
			{
				Reset();
			}
		}

		public void Update()
		{
			mDeltaTime += DTTime.TimeSinceStartup - mLastTime;
			mLastTime = DTTime.TimeSinceStartup;
			if (Settings.Speed > 0f)
			{
				int num = (int)(mDeltaTime / (double)Settings.Speed);
				mDeltaTime -= num;
				if (Count > Settings.Threshold)
				{
					num = Mathf.Min(num, Count - Settings.Threshold);
					while (num-- > 0)
					{
						destroy(mObjects[0]);
						mObjects.RemoveAt(0);
						log("Threshold exceeded: Deleting item");
					}
				}
				else if (Count < Settings.MinItems)
				{
					num = Mathf.Min(num, Settings.MinItems - Count);
					while (num-- > 0)
					{
						mObjects.Add(create());
						log("Below MinItems: Adding item");
					}
				}
			}
			else
			{
				mDeltaTime = 0.0;
			}
		}

		public void Reset()
		{
			if (Application.isPlaying)
			{
				while (Count < Settings.MinItems)
				{
					mObjects.Add(create());
				}
				while (Count > Settings.Threshold)
				{
					destroy(mObjects[0]);
					mObjects.RemoveAt(0);
				}
				log("Prewarm/Reset");
			}
		}

		public void Clear()
		{
			log("Clear");
			for (int i = 0; i < Count; i++)
			{
				destroy(mObjects[i]);
			}
			mObjects.Clear();
		}

		public virtual T Pop(Transform parent = null)
		{
			T val = default(T);
			if (Count > 0)
			{
				val = mObjects[0];
				mObjects.RemoveAt(0);
			}
			else if (Settings.AutoCreate || !Application.isPlaying)
			{
				log("Auto create item");
				val = create();
			}
			if (val != null)
			{
				sendAfterPop(val);
				setParent(val, parent);
				log("Pop " + val);
			}
			return val;
		}

		public virtual void Push(T item)
		{
			log("Push " + item);
			if (Application.isPlaying && item != null)
			{
				sendBeforePush(item);
				mObjects.Add(item);
			}
		}

		protected virtual void sendBeforePush(T item)
		{
			if (item is IPoolable)
			{
				((IPoolable)(object)item).OnBeforePush();
			}
		}

		protected virtual void sendAfterPop(T item)
		{
			if (item is IPoolable)
			{
				((IPoolable)(object)item).OnAfterPop();
			}
		}

		protected virtual void setParent(T item, Transform parent)
		{
		}

		protected virtual T create()
		{
			return Activator.CreateInstance<T>();
		}

		protected virtual void destroy(T item)
		{
		}

		private void log(string msg)
		{
			if (Settings.Debug)
			{
				Debug.Log(string.Format("[{0}] ({1} items) {2}", Identifier, Count, msg));
			}
		}
	}
}
