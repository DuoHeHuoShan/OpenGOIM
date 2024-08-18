using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ComponentExt
	{
		public static void StripComponents(this Component c, params Type[] toKeep)
		{
			if (toKeep.Length == 0)
			{
				c.gameObject.StripComponents(c.GetType());
			}
			else
			{
				c.gameObject.StripComponents(toKeep);
			}
		}

		public static GameObject AddChildGameObject(this Component c, string name)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(c.transform);
			return gameObject;
		}

		public static T AddChildGameObject<T>(this Component c, string name) where T : Component
		{
			GameObject gameObject = new GameObject(name);
			if ((bool)gameObject)
			{
				gameObject.transform.SetParent(c.transform);
				return gameObject.AddComponent<T>();
			}
			return (T)null;
		}

		public static T DuplicateGameObject<T>(this Component source, Transform newParent, bool keepPrefabConnection = false) where T : Component
		{
			if (!source || !source.gameObject)
			{
				return (T)null;
			}
			List<Component> list = new List<Component>(source.gameObject.GetComponents<Component>());
			int num = list.IndexOf(source);
			GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject);
			if ((bool)gameObject)
			{
				gameObject.transform.SetParent(newParent, false);
				Component[] components = gameObject.GetComponents<Component>();
				return components[num] as T;
			}
			return (T)null;
		}

		public static Component DuplicateGameObject(this Component source, Transform newParent, bool keepPrefabConnection = false)
		{
			if (!source || !source.gameObject || !newParent)
			{
				return null;
			}
			List<Component> list = new List<Component>(source.gameObject.GetComponents<Component>());
			int num = list.IndexOf(source);
			GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject);
			if ((bool)gameObject)
			{
				gameObject.transform.SetParent(newParent, false);
				Component[] components = gameObject.GetComponents<Component>();
				return components[num];
			}
			return null;
		}
	}
}
