namespace FluffyUnderware.DevTools
{
	public class RangeExAttribute : DTPropertyAttribute
	{
		public float MinValue;

		public string MinFieldOrPropertyName;

		public float MaxValue;

		public string MaxFieldOrPropertyName;

		public bool Slider = true;

		public RangeExAttribute(float minValue, float maxValue, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MinValue = minValue;
			MaxValue = maxValue;
		}

		public RangeExAttribute(string minFieldOrProperty, float maxValue, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MinFieldOrPropertyName = minFieldOrProperty;
			MaxValue = maxValue;
		}

		public RangeExAttribute(float minValue, string maxFieldOrProperty, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MinValue = minValue;
			MaxFieldOrPropertyName = maxFieldOrProperty;
		}

		public RangeExAttribute(string minFieldOrProperty, string maxFieldOrProperty, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MinFieldOrPropertyName = minFieldOrProperty;
			MaxFieldOrPropertyName = maxFieldOrProperty;
		}
	}
}
