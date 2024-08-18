using System;

namespace FluffyUnderware.Curvy.Generator
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InputSlotInfo : SlotInfo
	{
		public bool RequestDataOnly;

		public bool Optional;

		public bool ModifiesData;

		public InputSlotInfo(string name, params Type[] type)
			: base(name, type)
		{
		}

		public InputSlotInfo(params Type[] type)
			: this(null, type)
		{
		}

		public bool IsValidFrom(Type outType)
		{
			for (int i = 0; i < DataTypes.Length; i++)
			{
				if (outType == DataTypes[i] || outType.IsSubclassOf(DataTypes[i]))
				{
					return true;
				}
			}
			return false;
		}
	}
}
