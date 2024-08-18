using System;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class CGResourceManagerAttribute : DTPropertyAttribute
	{
		public readonly string ResourceName;

		public bool ReadOnly;

		public CGResourceManagerAttribute(string resourceName)
			: base(string.Empty, string.Empty)
		{
			ResourceName = resourceName;
		}
	}
}
