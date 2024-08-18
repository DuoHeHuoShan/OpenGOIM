using System;
using System.Reflection;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class FieldInfoExt
	{
		public static T GetCustomAttribute<T>(this FieldInfo field) where T : Attribute
		{
			object[] customAttributes = field.GetCustomAttributes(typeof(T), true);
			return (customAttributes.Length <= 0) ? ((T)null) : ((T)customAttributes[0]);
		}
	}
}
