using System;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class CGResourceCollectionManagerAttribute : CGResourceManagerAttribute
	{
		public bool ShowCount;

		public CGResourceCollectionManagerAttribute(string resourceName)
			: base(resourceName)
		{
			ReadOnly = true;
		}
	}
}
