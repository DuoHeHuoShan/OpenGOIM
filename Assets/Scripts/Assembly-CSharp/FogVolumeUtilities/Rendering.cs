using UnityEngine;

namespace FogVolumeUtilities
{
	public class Rendering
	{
		public static void EnsureKeyword(Material material, string name, bool enabled)
		{
			if (enabled != material.IsKeywordEnabled(name))
			{
				if (enabled)
				{
					material.EnableKeyword(name);
				}
				else
				{
					material.DisableKeyword(name);
				}
			}
		}
	}
}
