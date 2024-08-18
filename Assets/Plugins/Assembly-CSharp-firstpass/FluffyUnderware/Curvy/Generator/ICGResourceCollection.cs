using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public interface ICGResourceCollection
	{
		int Count { get; }

		Component[] ItemsArray { get; }
	}
}
