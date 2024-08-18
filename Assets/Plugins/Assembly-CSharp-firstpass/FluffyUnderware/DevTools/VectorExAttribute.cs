namespace FluffyUnderware.DevTools
{
	public class VectorExAttribute : DTPropertyAttribute
	{
		public VectorExAttribute(string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			Options = AttributeOptionsFlags.Full;
		}
	}
}
