using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[ResourceLoader("GameObject")]
	public class CGGameObjectResourceLoader : ICGResourceLoader
	{
		public Component Create(CGModule module, string context)
		{
			GameObject gameObject = module.Generator.PoolManager.GetPrefabPool(context).Pop();
			return gameObject.transform;
		}

		public void Destroy(CGModule module, Component obj, string context, bool kill)
		{
			if (!(obj != null))
			{
				return;
			}
			if (kill)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(obj.gameObject);
				}
				else
				{
					Object.DestroyImmediate(obj.gameObject);
				}
			}
			else
			{
				module.Generator.PoolManager.GetPrefabPool(context).Push(obj.gameObject);
			}
		}
	}
}
