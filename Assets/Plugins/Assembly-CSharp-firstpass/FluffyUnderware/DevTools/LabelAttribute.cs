namespace FluffyUnderware.DevTools
{
	public class LabelAttribute : DTPropertyAttribute
	{
		public LabelAttribute()
			: base(string.Empty, string.Empty)
		{
		}

		public LabelAttribute(string label, string tooltip = "")
			: base(label, tooltip)
		{
		}
	}
}
