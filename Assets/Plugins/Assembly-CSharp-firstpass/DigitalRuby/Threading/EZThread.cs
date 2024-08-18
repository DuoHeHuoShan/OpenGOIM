using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitalRuby.Threading
{
	public class EZThread : MonoBehaviour
	{
		public class EZThreadRunner
		{
			private bool running = true;

			private Action action;

			public AutoResetEvent SyncEvent { get; private set; }

			internal EZThreadRunner(Action action, bool synchronizeWithUpdate)
			{
				this.action = action;
				running = true;
				if (synchronizeWithUpdate)
				{
					SyncEvent = new AutoResetEvent(true);
				}
				ThreadPool.QueueUserWorkItem((!synchronizeWithUpdate) ? ((WaitCallback)delegate
				{
					ThreadFunction();
				}) : ((WaitCallback)delegate
				{
					ThreadFunctionSync();
				}));
			}

			private void ThreadFunction()
			{
				while (running)
				{
					action();
				}
			}

			private void ThreadFunctionSync()
			{
				while (running)
				{
					action();
					SyncEvent.WaitOne(100);
				}
			}

			public void Stop()
			{
				running = false;
			}
		}

		private static GameObject singletonObject;

		private static EZThread singleton;

		private readonly List<KeyValuePair<Action, float>> mainThreadActions = new List<KeyValuePair<Action, float>>();

		private readonly Queue<KeyValuePair<object, Action<object>>> mainThreadCompletions = new Queue<KeyValuePair<object, Action<object>>>();

		private readonly List<EZThreadRunner> threads = new List<EZThreadRunner>();

		public EZThread Instance
		{
			get
			{
				return singleton;
			}
		}

		private static void EnsureCreated()
		{
			if (!(singleton != null))
			{
				singletonObject = new GameObject("EZTHREAD");
				singletonObject.hideFlags = HideFlags.HideAndDontSave;
				singleton = singletonObject.AddComponent<EZThread>();
				UnityEngine.Object.DontDestroyOnLoad(singleton);
				ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount);
				ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
			}
		}

		private static void InternalExecute(Func<object> a, Action<object> completion)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				object result = a();
				if (completion != null)
				{
					ExecuteOnMainThread(delegate
					{
						completion(result);
					});
				}
			});
		}

		private void RunMainThreadActions()
		{
			int num = 0;
			while (true)
			{
				KeyValuePair<Action, float> keyValuePair;
				lock (mainThreadActions)
				{
					if (num >= mainThreadActions.Count)
					{
						break;
					}
					keyValuePair = mainThreadActions[num];
					float num2 = keyValuePair.Value - Time.deltaTime;
					if (num2 > 0f)
					{
						mainThreadActions[num++] = new KeyValuePair<Action, float>(keyValuePair.Key, num2);
						continue;
					}
					mainThreadActions.RemoveAt(num);
					goto IL_0088;
				}
				IL_0088:
				keyValuePair.Key();
			}
			while (true)
			{
				KeyValuePair<object, Action<object>> keyValuePair2;
				lock (mainThreadCompletions)
				{
					if (mainThreadCompletions.Count == 0)
					{
						break;
					}
					keyValuePair2 = mainThreadCompletions.Dequeue();
				}
				keyValuePair2.Value(keyValuePair2.Key);
			}
		}

		private void NotifyThreads()
		{
			lock (threads)
			{
				foreach (EZThreadRunner thread in threads)
				{
					if (thread.SyncEvent != null)
					{
						thread.SyncEvent.Set();
					}
				}
			}
		}

		private void Start()
		{
			SceneManager.sceneUnloaded += SceneManagerSceneUnloaded;
		}

		private void SceneManagerSceneUnloaded(Scene arg0)
		{
			Reset();
		}

		private void Update()
		{
			RunMainThreadActions();
			NotifyThreads();
		}

		private void OnDestroy()
		{
			Reset();
		}

		private void OnApplicationQuit()
		{
			Reset();
		}

		public void Reset()
		{
			lock (threads)
			{
				foreach (EZThreadRunner thread in threads)
				{
					thread.Stop();
				}
				threads.Clear();
			}
			lock (mainThreadActions)
			{
				mainThreadActions.Clear();
			}
			lock (mainThreadCompletions)
			{
				mainThreadCompletions.Clear();
			}
		}

		public static void ExecuteInBackground(Action action)
		{
			EnsureCreated();
			InternalExecute(delegate
			{
				action();
				return (object)null;
			}, null);
		}

		public static void ExecuteInBackground(Func<object> action, Action<object> completion)
		{
			EnsureCreated();
			InternalExecute(action, completion);
		}

		public static void ExecuteOnMainThread(Action action)
		{
			EnsureCreated();
			lock (singleton.mainThreadActions)
			{
				singleton.mainThreadActions.Add(new KeyValuePair<Action, float>(action, 0f));
			}
		}

		public static void ExecuteOnMainThread(Action action, float delaySeconds)
		{
			EnsureCreated();
			lock (singleton.mainThreadActions)
			{
				singleton.mainThreadActions.Add(new KeyValuePair<Action, float>(action, delaySeconds));
			}
		}

		public static EZThreadRunner BeginThread(Action action)
		{
			return BeginThread(action, true);
		}

		public static EZThreadRunner BeginThread(Action action, bool synchronizeWithUpdate)
		{
			EnsureCreated();
			EZThreadRunner eZThreadRunner = new EZThreadRunner(action, synchronizeWithUpdate);
			lock (singleton.threads)
			{
				singleton.threads.Add(eZThreadRunner);
				return eZThreadRunner;
			}
		}

		public static void EndThread(EZThreadRunner thread)
		{
			thread.Stop();
			lock (singleton.threads)
			{
				singleton.threads.Remove(thread);
			}
		}
	}
}
