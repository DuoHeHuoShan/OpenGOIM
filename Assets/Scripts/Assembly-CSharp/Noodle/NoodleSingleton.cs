using UnityEngine;

namespace Noodle
{
	public class NoodleSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Object.FindObjectOfType<T>();
				}
				if (_instance == null)
				{
					string text = typeof(T).ToString();
					Debug.LogWarningFormat("[NoodleSingleton<{0}>] Call To Instance() creating a new GameObject", text);
					GameObject gameObject = new GameObject(text);
					_instance = gameObject.AddComponent<T>();
				}
				return _instance;
			}
		}

		protected bool IsValidSingleton
		{
			get
			{
				return _instance == this;
			}
		}

		protected virtual void Awake()
		{
			if (_instance != null)
			{
				Debug.LogWarning("[NoodleSingleton] " + typeof(T).ToString() + " was a duplicate singleton!");
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DontDestroyOnLoad(base.gameObject);
				_instance = GetComponent<T>();
			}
		}
	}
}
