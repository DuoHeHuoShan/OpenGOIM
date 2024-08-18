using System;

namespace FluffyUnderware.DevTools
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class DTAttribute : Attribute, IComparable
	{
		public int Sort = 100;

		public bool ShowBelowProperty;

		public int Space;

		public int TypeSort { get; protected set; }

		public DTAttribute(int sortOrder, bool showBelow = false)
		{
			TypeSort = sortOrder;
			ShowBelowProperty = showBelow;
		}

		public virtual int CompareTo(object obj)
		{
			DTAttribute dTAttribute = (DTAttribute)obj;
			int num = ShowBelowProperty.CompareTo(dTAttribute.ShowBelowProperty);
			if (num == 0)
			{
				int num2 = TypeSort.CompareTo(dTAttribute.TypeSort);
				if (num2 == 0)
				{
					return Sort.CompareTo(dTAttribute.Sort);
				}
				return num2;
			}
			return num;
		}
	}
}
