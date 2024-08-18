namespace FluffyUnderware.DevTools
{
	public class GroupConditionAttribute : ConditionalAttribute, IDTGroupRenderAttribute
	{
		public GroupConditionAttribute(string fieldOrProperty, object compareTo, bool compareFalse = false)
			: base(fieldOrProperty, compareTo, compareFalse)
		{
		}

		public GroupConditionAttribute(string fieldOrProperty, object compareTo, bool compareFalse, OperatorEnum op, string fieldOrProperty2, object compareTo2, bool compareFalse2)
			: base(fieldOrProperty, compareTo, compareFalse, op, fieldOrProperty2, compareTo2, compareFalse2)
		{
		}

		public GroupConditionAttribute(string fieldOrProperty, object compareTo, bool compareFalse, OperatorEnum op, string fieldOrProperty2, object compareTo2, bool compareFalse2, string fieldOrProperty3, object compareTo3, bool compareFalse3)
			: base(fieldOrProperty, compareTo, compareFalse, op, fieldOrProperty2, compareTo2, compareFalse2, fieldOrProperty3, compareTo3, compareFalse3)
		{
		}

		public GroupConditionAttribute(string methodToQuery)
			: base(methodToQuery)
		{
		}
	}
}
