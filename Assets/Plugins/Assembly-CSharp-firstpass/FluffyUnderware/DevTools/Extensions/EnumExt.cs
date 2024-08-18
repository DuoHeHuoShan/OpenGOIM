using System;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class EnumExt
	{
		public static bool HasFlag(this Enum variable, params Enum[] flags)
		{
			if (flags.Length == 0)
			{
				throw new ArgumentNullException("flags");
			}
			int num = Convert.ToInt32(variable);
			Type type = variable.GetType();
			for (int i = 0; i < flags.Length; i++)
			{
				if (!Enum.IsDefined(type, flags[i]))
				{
					throw new ArgumentException(string.Format("Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.", flags[i].GetType(), type));
				}
				int num2 = Convert.ToInt32(flags[i]);
				if ((num & num2) == num2)
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasFlag<T>(this T value, T flag) where T : struct
		{
			long num = Convert.ToInt64(value);
			long num2 = Convert.ToInt64(flag);
			return (num & num2) != 0;
		}

		public static T Set<T>(this Enum value, T append)
		{
			return value.Set(append, true);
		}

		public static T Set<T>(this Enum value, T append, bool OnOff)
		{
			if (append == null)
			{
				throw new ArgumentNullException("append");
			}
			Type type = value.GetType();
			if (OnOff)
			{
				return (T)Enum.Parse(type, (Convert.ToUInt64(value) | Convert.ToUInt64(append)).ToString());
			}
			return (T)Enum.Parse(type, (Convert.ToUInt64(value) & ~Convert.ToUInt64(append)).ToString());
		}

		public static T Toggle<T>(this Enum value, T toggleValue)
		{
			if (toggleValue == null)
			{
				throw new ArgumentNullException("toggleValue");
			}
			Type type = value.GetType();
			ulong num = Convert.ToUInt64(value);
			return (T)Enum.Parse(type, (num ^ num).ToString());
		}

		public static T SetAll<T>(this Enum value)
		{
			Type type = value.GetType();
			string[] names = Enum.GetNames(type);
			string[] array = names;
			foreach (string value2 in array)
			{
				value.Set(Enum.Parse(type, value2));
			}
			return (T)(object)value;
		}
	}
}
