using System;

namespace FluffyUnderware.Curvy.Generator
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ResourceLoaderAttribute : Attribute
	{
		public readonly string ResourceName;

		public ResourceLoaderAttribute(string resName)
		{
			ResourceName = resName;
		}
	}
}
