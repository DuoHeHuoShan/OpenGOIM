using System;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTUtility
	{
		public static bool IsEditorStateChange
		{
			get
			{
				return false;
			}
		}

		public static Material GetDefaultMaterial()
		{
			return null;
		}

		public static float GetHandleSize(Vector3 position)
		{
			Camera current = Camera.current;
			position = Gizmos.matrix.MultiplyPoint(position);
			if ((bool)current)
			{
				Transform transform = current.transform;
				Vector3 position2 = transform.position;
				float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
				Vector3 vector = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
				Vector3 vector2 = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
				float magnitude = (vector - vector2).magnitude;
				return 80f / Mathf.Max(magnitude, 0.0001f);
			}
			return 20f;
		}

		public static void SetPlayerPrefs<T>(string key, T value)
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsEnum)
			{
				PlayerPrefs.SetInt(key, Convert.ToInt32(Enum.Parse(typeof(T), value.ToString()) as Enum));
				return;
			}
			if (typeFromHandle.IsArray)
			{
				throw new NotImplementedException();
			}
			if (typeFromHandle.Matches(typeof(int), typeof(int)))
			{
				PlayerPrefs.SetInt(key, (value as int?).Value);
			}
			else if (typeFromHandle == typeof(string))
			{
				PlayerPrefs.SetString(key, value as string);
			}
			else if (typeFromHandle == typeof(float))
			{
				PlayerPrefs.SetFloat(key, (value as float?).Value);
			}
			else if (typeFromHandle == typeof(bool))
			{
				PlayerPrefs.SetInt(key, (value as bool?).Value ? 1 : 0);
			}
			else if (typeFromHandle == typeof(Color))
			{
				PlayerPrefs.SetString(key, (value as Color?).Value.ToHtml());
			}
			else
			{
				Debug.LogError("[DevTools.SetEditorPrefs] Unsupported datatype: " + typeFromHandle.Name);
			}
		}

		public static T GetPlayerPrefs<T>(string key, T defaultValue)
		{
			if (PlayerPrefs.HasKey(key))
			{
				Type typeFromHandle = typeof(T);
				try
				{
					if (typeFromHandle.IsEnum || typeFromHandle.Matches(typeof(int), typeof(int)))
					{
						return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
					}
					if (typeFromHandle.IsArray)
					{
						throw new NotImplementedException();
					}
					if (typeFromHandle == typeof(string))
					{
						return (T)(object)PlayerPrefs.GetString(key, defaultValue.ToString());
					}
					if (typeFromHandle == typeof(float))
					{
						return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
					}
					if (typeFromHandle == typeof(bool))
					{
						return (T)(object)(PlayerPrefs.GetInt(key, ((bool)(object)defaultValue) ? 1 : 0) == 1);
					}
					if (typeFromHandle == typeof(Color))
					{
						return (T)(object)PlayerPrefs.GetString(key, ((Color)(object)defaultValue).ToHtml()).ColorFromHtml();
					}
					Debug.LogError("[DevTools.SetEditorPrefs] Unsupported datatype: " + typeFromHandle.Name);
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}

		public static float RandomSign()
		{
			return UnityEngine.Random.Range(0, 2) * 2 - 1;
		}

		public static string GetHelpUrl(object forClass)
		{
			return (forClass != null) ? GetHelpUrl(forClass.GetType()) : string.Empty;
		}

		public static string GetHelpUrl(Type classType)
		{
			if (classType != null)
			{
				object[] customAttributes = classType.GetCustomAttributes(typeof(HelpURLAttribute), true);
				if (customAttributes.Length > 0)
				{
					return ((HelpURLAttribute)customAttributes[0]).URL;
				}
			}
			return string.Empty;
		}

		public static Vector3 GetCenterPosition(Vector3 fallback, params Vector3[] vectors)
		{
			if (vectors.Length == 0)
			{
				return fallback;
			}
			Vector3 vector = vectors[0];
			for (int i = 1; i < vectors.Length; i++)
			{
				vector += vectors[i];
			}
			return vector / vectors.Length;
		}

		public static T CreateGameObject<T>(Transform parent, string name) where T : MonoBehaviour
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			return gameObject.AddComponent<T>();
		}
	}
}
