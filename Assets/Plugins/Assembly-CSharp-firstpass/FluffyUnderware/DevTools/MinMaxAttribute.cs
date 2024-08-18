namespace FluffyUnderware.DevTools
{
	public class MinMaxAttribute : DTPropertyAttribute
	{
		public readonly string MaxValueField;

		public float Min;

		public string MinBoundFieldOrPropertyName;

		public float Max;

		public string MaxBoundFieldOrPropertyName;

		public MinMaxAttribute(string maxValueField, string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			MaxValueField = maxValueField;
			Min = 0f;
			Max = 1f;
		}
	}
}
