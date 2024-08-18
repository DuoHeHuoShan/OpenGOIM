using System;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class CGDataReferenceSelectorAttribute : DTPropertyAttribute
	{
		public readonly Type DataType;

		public CGDataReferenceSelectorAttribute(Type dataType)
			: base(string.Empty, string.Empty)
		{
			DataType = dataType;
		}
	}
}
