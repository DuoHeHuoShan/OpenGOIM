namespace FluffyUnderware.DevTools
{
	public class FieldActionAttribute : ActionAttribute, IDTFieldRenderAttribute
	{
		public FieldActionAttribute(string actionData, ActionEnum action = ActionEnum.Callback)
			: base(actionData, action)
		{
		}
	}
}
