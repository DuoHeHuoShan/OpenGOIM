using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeshCombineStudio
{
	public static class Methods
	{
		public static Vector3 Clamp(Vector3 v, float min, float max)
		{
			if (v.x < min)
			{
				v.x = min;
			}
			else if (v.x > max)
			{
				v.x = max;
			}
			if (v.y < min)
			{
				v.y = min;
			}
			else if (v.y > max)
			{
				v.y = max;
			}
			if (v.z < min)
			{
				v.z = min;
			}
			else if (v.z > max)
			{
				v.z = max;
			}
			return v;
		}

		public static Vector3 FloatToVector3(float v)
		{
			return new Vector3(v, v, v);
		}

		public static float SinDeg(float angle)
		{
			return Mathf.Sin(angle * ((float)Math.PI / 180f));
		}

		public static void SetTag(GameObject go, string tag)
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].tag = tag;
			}
		}

		public static void SetTagWhenCollider(GameObject go, string tag)
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].GetComponent<Collider>() != null)
				{
					componentsInChildren[i].tag = tag;
				}
			}
		}

		public static void SetTagAndLayer(GameObject go, string tag, int layer)
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].tag = tag;
				componentsInChildren[i].gameObject.layer = layer;
			}
		}

		public static void SetLayer(GameObject go, int layer)
		{
			go.layer = layer;
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = layer;
			}
		}

		public static bool Contains(string compare, string name)
		{
			List<string> list = new List<string>();
			int num;
			do
			{
				num = name.IndexOf("*");
				if (num != -1)
				{
					if (num != 0)
					{
						list.Add(name.Substring(0, num));
					}
					if (num == name.Length - 1)
					{
						break;
					}
					name = name.Substring(num + 1);
				}
			}
			while (num != -1);
			list.Add(name);
			for (int i = 0; i < list.Count; i++)
			{
				if (!compare.Contains(list[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static T[] Search<T>(GameObject parentGO = null)
		{
			GameObject[] array = null;
			array = ((!(parentGO == null)) ? new GameObject[1] { parentGO } : SceneManager.GetActiveScene().GetRootGameObjects());
			if (array == null)
			{
				return null;
			}
			if (typeof(T) == typeof(GameObject))
			{
				List<GameObject> list = new List<GameObject>();
				for (int i = 0; i < array.Length; i++)
				{
					Transform[] componentsInChildren = array[i].GetComponentsInChildren<Transform>(true);
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						list.Add(componentsInChildren[j].gameObject);
					}
				}
				return list.ToArray() as T[];
			}
			if (parentGO == null)
			{
				List<T> list2 = new List<T>();
				for (int k = 0; k < array.Length; k++)
				{
					list2.AddRange(array[k].GetComponentsInChildren<T>(true));
				}
				return list2.ToArray();
			}
			return parentGO.GetComponentsInChildren<T>(true);
		}

		public static T Find<T>(GameObject parentGO, string name) where T : UnityEngine.Object
		{
			T[] array = Search<T>(parentGO);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == name)
				{
					return array[i];
				}
			}
			return (T)null;
		}

		public static void SetCollidersActive(Collider[] colliders, bool active, string[] nameList)
		{
			for (int i = 0; i < colliders.Length; i++)
			{
				for (int j = 0; j < nameList.Length; j++)
				{
					if (colliders[i].name.Contains(nameList[j]))
					{
						colliders[i].enabled = active;
					}
				}
			}
		}

		public static void SelectChildrenWithMeshRenderer(Transform t)
		{
		}

		public static void DestroyChildren(Transform t)
		{
			while (t.childCount > 0)
			{
				Transform child = t.GetChild(0);
				child.parent = null;
				UnityEngine.Object.DestroyImmediate(child.gameObject);
			}
		}

		public static void Destroy(GameObject go)
		{
			if (!(go == null))
			{
				UnityEngine.Object.Destroy(go);
			}
		}

		public static void SetChildrenActive(Transform t, bool active)
		{
			for (int i = 0; i < t.childCount; i++)
			{
				Transform child = t.GetChild(i);
				child.gameObject.SetActive(active);
			}
		}

		public static float GetMax(Vector3 v)
		{
			float num = v.x;
			if (v.y > num)
			{
				num = v.y;
			}
			if (v.z > num)
			{
				num = v.z;
			}
			return num;
		}

		public static Vector3 SetMin(Vector3 v, float min)
		{
			if (v.x < min)
			{
				v.x = min;
			}
			if (v.y < min)
			{
				v.y = min;
			}
			if (v.z < min)
			{
				v.z = min;
			}
			return v;
		}

		public static Vector3 Snap(Vector3 v, float snapSize)
		{
			v.x = Mathf.Floor(v.x / snapSize) * snapSize;
			v.y = Mathf.Floor(v.y / snapSize) * snapSize;
			v.z = Mathf.Floor(v.z / snapSize) * snapSize;
			return v;
		}

		public static Vector3 Abs(Vector3 v)
		{
			return new Vector3((!(v.x < 0f)) ? v.x : (0f - v.x), (!(v.y < 0f)) ? v.y : (0f - v.y), (!(v.z < 0f)) ? v.z : (0f - v.z));
		}

		public static void SnapBoundsAndPreserveArea(ref Bounds bounds, float snapSize, Vector3 offset)
		{
			Vector3 vector = Snap(bounds.center, snapSize) + offset;
			bounds.size += Abs(vector - bounds.center) * 2f;
			bounds.center = vector;
		}

		public static void ListRemoveAt<T>(List<T> list, int index)
		{
			list[index] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
		}

		public static void CopyComponent(Component component, GameObject target)
		{
			Type type = component.GetType();
			target.AddComponent(type);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				propertyInfo.SetValue(target.GetComponent(type), propertyInfo.GetValue(component, null), null);
			}
		}
	}
}
