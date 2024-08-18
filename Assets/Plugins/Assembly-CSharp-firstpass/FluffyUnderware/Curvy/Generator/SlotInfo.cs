using System;

namespace FluffyUnderware.Curvy.Generator
{
	public class SlotInfo : Attribute, IComparable
	{
		public readonly Type[] DataTypes;

		public string Name;

		public string Tooltip;

		public bool Array;

		protected SlotInfo(string name, params Type[] type)
		{
			DataTypes = type;
			Name = name;
		}

		protected SlotInfo(params Type[] type)
			: this(null, type)
		{
		}

		public int CompareTo(object obj)
		{
			return ((SlotInfo)obj).Name.CompareTo(Name);
		}
	}
}
