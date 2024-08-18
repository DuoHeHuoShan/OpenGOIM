using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class DTPropertyAttribute : PropertyAttribute
	{
		public string Label;

		public string Tooltip;

		public string Color;

		public AttributeOptionsFlags Options;

		public int Precision = -1;

		public DTPropertyAttribute(string label = "", string tooltip = "")
		{
			Label = label;
			Tooltip = tooltip;
		}
	}
}
