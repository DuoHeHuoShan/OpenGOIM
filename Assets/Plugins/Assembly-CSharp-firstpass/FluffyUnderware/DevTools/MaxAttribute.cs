namespace FluffyUnderware.DevTools
{
	public class MaxAttribute : DTPropertyAttribute
	{
		public float MaxValue;

		public string MaxFieldOrPropertyName;

		public MaxAttribute(float value, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MaxValue = value;
		}

		public MaxAttribute(string fieldOrProperty, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MaxFieldOrPropertyName = fieldOrProperty;
		}
	}
}
