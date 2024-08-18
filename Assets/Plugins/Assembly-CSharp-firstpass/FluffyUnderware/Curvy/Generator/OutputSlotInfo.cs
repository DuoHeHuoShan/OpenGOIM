using System;

namespace FluffyUnderware.Curvy.Generator
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputSlotInfo : SlotInfo
	{
		public Type DataType
		{
			get
			{
				return DataTypes[0];
			}
		}

		public OutputSlotInfo(Type type)
			: this(null, type)
		{
		}

		public OutputSlotInfo(string name, Type type)
			: base(name, type)
		{
		}
	}
}
