using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGResourceHandler
	{
		private static Dictionary<string, ICGResourceLoader> Loader = new Dictionary<string, ICGResourceLoader>();

		public static Component CreateResource(CGModule module, string resName, string context)
		{
			if (Loader.Count == 0)
			{
				getLoaders();
			}
			if (Loader.ContainsKey(resName))
			{
				ICGResourceLoader iCGResourceLoader = Loader[resName];
				return iCGResourceLoader.Create(module, context);
			}
			Debug.LogError("CGResourceHandler: Missing Loader for resource '" + resName + "'");
			return null;
		}

		public static void DestroyResource(CGModule module, string resName, Component obj, string context, bool kill)
		{
			if (Loader.Count == 0)
			{
				getLoaders();
			}
			if (Loader.ContainsKey(resName))
			{
				ICGResourceLoader iCGResourceLoader = Loader[resName];
				iCGResourceLoader.Destroy(module, obj, context, kill);
			}
			else
			{
				Debug.LogError("CGResourceHandler: Missing Loader for resource '" + resName + "'");
			}
		}

		private static void getLoaders()
		{
			Type[] allTypes = typeof(CGModule).GetAllTypes();
			Type[] array = allTypes;
			foreach (Type type in array)
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(ResourceLoaderAttribute), true);
				if (customAttributes.Length > 0)
				{
					ICGResourceLoader iCGResourceLoader = (ICGResourceLoader)Activator.CreateInstance(type);
					if (iCGResourceLoader != null)
					{
						Loader.Add(((ResourceLoaderAttribute)customAttributes[0]).ResourceName, iCGResourceLoader);
					}
				}
			}
		}
	}
}
