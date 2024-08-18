namespace FluffyUnderware.DevTools
{
	public class AnimationCurveExAttribute : DTPropertyAttribute
	{
		public AnimationCurveExAttribute(string label = "", string tooltip = "")
			: base(label, tooltip)
		{
			Options = AttributeOptionsFlags.Clipboard;
		}
	}
}
