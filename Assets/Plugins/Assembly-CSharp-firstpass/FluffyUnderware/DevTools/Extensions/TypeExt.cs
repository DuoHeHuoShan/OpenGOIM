using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class TypeExt
	{
		public static Type[] GetAllTypes(this Type typeFromAssembly)
		{
			return typeFromAssembly.Assembly.GetTypes();
		}

		public static Dictionary<U, Type> GetAllTypesWithAttribute<U>(this Type type)
		{
			Dictionary<U, Type> dictionary = new Dictionary<U, Type>();
			Type[] allTypes = type.GetAllTypes();
			Type[] array = allTypes;
			foreach (Type type2 in array)
			{
				if (type2.IsSubclassOf(type))
				{
					object[] customAttributes = type2.GetCustomAttributes(typeof(U), false);
					if (customAttributes.Length > 0)
					{
						dictionary.Add((U)customAttributes[0], type2);
					}
				}
			}
			return dictionary;
		}

		public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type, bool includeInherited = false, bool includePrivate = false) where T : Attribute
		{
			FieldInfo[] allFields = type.GetAllFields(includeInherited, includePrivate);
			List<FieldInfo> list = new List<FieldInfo>();
			FieldInfo[] array = allFields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.GetCustomAttribute<T>() != null)
				{
					list.Add(fieldInfo);
				}
			}
			return list;
		}

		public static T GetCustomAttribute<T>(this Type type) where T : Attribute
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(T), true);
			return (customAttributes.Length <= 0) ? ((T)null) : ((T)customAttributes[0]);
		}

		public static MethodInfo MethodByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				return type.GetMethodIncludingBaseClasses(name, bindingFlags);
			}
			return type.GetMethod(name, bindingFlags);
		}

		public static FieldInfo FieldByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				return type.GetFieldIncludingBaseClasses(name, bindingFlags);
			}
			return type.GetField(name, bindingFlags);
		}

		public static PropertyInfo PropertyByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				return type.GetPropertyIncludingBaseClasses(name, bindingFlags);
			}
			return type.GetProperty(name, bindingFlags);
		}

		public static FieldInfo[] GetAllFields(this Type type, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				Type type2 = type;
				List<FieldInfo> list = new List<FieldInfo>();
				while (type2 != typeof(object))
				{
					list.AddRange(type2.GetFields(bindingFlags));
					type2 = type2.BaseType;
				}
				return list.ToArray();
			}
			return type.GetFields(bindingFlags);
		}

		public static PropertyInfo[] GetAllProperties(this Type type, bool includeInherited = false, bool includePrivate = false)
		{
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
			if (includePrivate)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (includeInherited)
			{
				Type type2 = type;
				List<PropertyInfo> list = new List<PropertyInfo>();
				while (type2 != typeof(object))
				{
					list.AddRange(type2.GetProperties(bindingFlags));
					type2 = type2.BaseType;
				}
				return list.ToArray();
			}
			return type.GetProperties(bindingFlags);
		}

		public static bool IsFrameworkType(this Type type)
		{
			return type.IsPrimitive || type.Equals(typeof(string)) || type.Equals(typeof(DateTime));
		}

		public static bool IsArrayOrList(this Type type)
		{
			return type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));
		}

		public static Type GetEnumerableType(this Type t)
		{
			Type type = FindIEnumerable(t);
			if (type == null)
			{
				return t;
			}
			return type.GetGenericArguments()[0];
		}

		private static Type FindIEnumerable(Type seqType)
		{
			if (seqType == null || seqType == typeof(string))
			{
				return null;
			}
			if (seqType.IsArray)
			{
				return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
			}
			if (seqType.IsGenericType)
			{
				Type[] genericArguments = seqType.GetGenericArguments();
				foreach (Type type in genericArguments)
				{
					Type type2 = typeof(IEnumerable<>).MakeGenericType(type);
					if (type2.IsAssignableFrom(seqType))
					{
						return type2;
					}
				}
			}
			Type[] interfaces = seqType.GetInterfaces();
			if (interfaces != null && interfaces.Length > 0)
			{
				Type[] array = interfaces;
				foreach (Type seqType2 in array)
				{
					Type type3 = FindIEnumerable(seqType2);
					if (type3 != null)
					{
						return type3;
					}
				}
			}
			if (seqType.BaseType != null && seqType.BaseType != typeof(object))
			{
				return FindIEnumerable(seqType.BaseType);
			}
			return null;
		}

		private static MethodInfo GetMethodIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
		{
			MethodInfo method = type.GetMethod(name, bindingFlags);
			if (type.BaseType == typeof(object))
			{
				return method;
			}
			for (Type type2 = type; type2 != typeof(object); type2 = type2.BaseType)
			{
				method = type2.GetMethod(name, bindingFlags);
				if (method != null)
				{
					return method;
				}
			}
			return null;
		}

		private static FieldInfo GetFieldIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
		{
			FieldInfo field = type.GetField(name, bindingFlags);
			if (type.BaseType == typeof(object))
			{
				return field;
			}
			for (Type type2 = type; type2 != typeof(object); type2 = type2.BaseType)
			{
				field = type2.GetField(name, bindingFlags);
				if (field != null)
				{
					return field;
				}
			}
			return null;
		}

		private static PropertyInfo GetPropertyIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
		{
			PropertyInfo property = type.GetProperty(name, bindingFlags);
			if (type.BaseType == typeof(object))
			{
				return property;
			}
			for (Type type2 = type; type2 != typeof(object); type2 = type2.BaseType)
			{
				property = type2.GetProperty(name, bindingFlags);
				if (property != null)
				{
					return property;
				}
			}
			return null;
		}

		public static bool Matches(this Type type, params Type[] types)
		{
			foreach (Type type2 in types)
			{
				if (type == type2 || type.IsAssignableFrom(type2))
				{
					return true;
				}
			}
			return false;
		}
	}
}
