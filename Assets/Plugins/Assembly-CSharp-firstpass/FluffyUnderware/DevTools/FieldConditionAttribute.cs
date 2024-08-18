namespace FluffyUnderware.DevTools
{
	public class FieldConditionAttribute : ConditionalAttribute, IDTFieldRenderAttribute
	{
		public FieldConditionAttribute(string fieldOrProperty, object compareTo, bool compareFalse = false, ActionEnum action = ActionEnum.Show, object actionData = null, ActionPositionEnum position = ActionPositionEnum.Below)
			: base(fieldOrProperty, compareTo, compareFalse)
		{
			Action = action;
			ActionData = actionData;
			Position = position;
		}

		public FieldConditionAttribute(string fieldOrProperty, object compareTo, bool compareFalse, OperatorEnum op, string fieldOrProperty2, object compareTo2, bool compareFalse2)
			: base(fieldOrProperty, compareTo, compareFalse, op, fieldOrProperty2, compareTo2, compareFalse2)
		{
		}

		public FieldConditionAttribute(string fieldOrProperty, object compareTo, bool compareFalse, OperatorEnum op, string fieldOrProperty2, object compareTo2, bool compareFalse2, string fieldOrProperty3, object compareTo3, bool compareFalse3)
			: base(fieldOrProperty, compareTo, compareFalse, op, fieldOrProperty2, compareTo2, compareFalse2, fieldOrProperty3, compareTo3, compareFalse3)
		{
		}

		public FieldConditionAttribute(string methodToQuery)
			: base(methodToQuery)
		{
		}
	}
}
