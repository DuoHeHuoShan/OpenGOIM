using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public interface ICGResourceLoader
	{
		Component Create(CGModule module, string context);

		void Destroy(CGModule module, Component obj, string context, bool kill);
	}
}
