using System.Reflection;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class ConditionalAttribute : ActionAttribute
	{
		public enum OperatorEnum
		{
			AND = 0,
			OR = 1
		}

		public class Condition
		{
			public string FieldName;

			public FieldInfo FieldInfo;

			public PropertyInfo PropertyInfo;

			public object CompareTo;

			public bool CompareFalse;

			public OperatorEnum Operator;

			public MethodInfo MethodInfo;

			public string MethodName;
		}

		public Condition[] Conditions;

		protected ConditionalAttribute(string fieldOrProperty, object compareTo, bool compareFalse = false)
			: base(null, ActionEnum.Show)
		{
			base.TypeSort = 55;
			Conditions = new Condition[1]
			{
				new Condition
				{
					FieldName = fieldOrProperty,
					CompareTo = compareTo,
					CompareFalse = compareFalse
				}
			};
		}

		protected ConditionalAttribute(string fieldOrProperty, object compareTo, bool compareFalse, OperatorEnum op, string fieldOrProperty2, object compareTo2, bool compareFalse2)
			: base(null, ActionEnum.Show)
		{
			base.TypeSort = 55;
			Conditions = new Condition[2]
			{
				new Condition
				{
					FieldName = fieldOrProperty,
					CompareTo = compareTo,
					CompareFalse = compareFalse
				},
				new Condition
				{
					FieldName = fieldOrProperty2,
					CompareTo = compareTo2,
					CompareFalse = compareFalse2,
					Operator = op
				}
			};
		}

		protected ConditionalAttribute(string fieldOrProperty, object compareTo, bool compareFalse, OperatorEnum op, string fieldOrProperty2, object compareTo2, bool compareFalse2, string fieldOrProperty3, object compareTo3, bool compareFalse3)
			: base(null, ActionEnum.Show)
		{
			base.TypeSort = 55;
			Conditions = new Condition[3]
			{
				new Condition
				{
					FieldName = fieldOrProperty,
					CompareTo = compareTo,
					CompareFalse = compareFalse
				},
				new Condition
				{
					FieldName = fieldOrProperty2,
					CompareTo = compareTo2,
					CompareFalse = compareFalse2,
					Operator = op
				},
				new Condition
				{
					FieldName = fieldOrProperty3,
					CompareTo = compareTo3,
					CompareFalse = compareFalse3,
					Operator = op
				}
			};
		}

		protected ConditionalAttribute(string methodToQuery)
			: base(null, ActionEnum.Show)
		{
			base.TypeSort = 55;
			Conditions = new Condition[1]
			{
				new Condition
				{
					MethodName = methodToQuery,
					CompareTo = null
				}
			};
		}

		public virtual bool ConditionMet(object classInstance)
		{
			bool flag = evaluate(Conditions[0], classInstance);
			for (int i = 1; i < Conditions.Length; i++)
			{
				Condition condition = Conditions[i];
				switch (condition.Operator)
				{
				case OperatorEnum.AND:
					flag = flag && evaluate(condition, classInstance);
					break;
				case OperatorEnum.OR:
					flag = flag || evaluate(condition, classInstance);
					break;
				}
			}
			return flag;
		}

		private bool evaluate(Condition cond, object classInstance)
		{
			if (!string.IsNullOrEmpty(cond.MethodName))
			{
				if (cond.MethodInfo == null)
				{
					cond.MethodInfo = classInstance.GetType().MethodByName(cond.MethodName, true, true);
				}
				if (cond.MethodInfo != null)
				{
					if (cond.CompareFalse)
					{
						return !(bool)cond.MethodInfo.Invoke(classInstance, null);
					}
					return (bool)cond.MethodInfo.Invoke(classInstance, null);
				}
				Debug.LogWarningFormat("[DevTools] Unable to find method '{0}' at class '{1}' !", cond.MethodName, classInstance.GetType().Name);
				return cond.CompareFalse;
			}
			if (cond.FieldInfo == null)
			{
				cond.FieldInfo = classInstance.GetType().FieldByName(cond.FieldName, true, true);
				if (cond.FieldInfo == null)
				{
					cond.PropertyInfo = classInstance.GetType().PropertyByName(cond.FieldName, true, true);
				}
			}
			object obj = null;
			if (cond.FieldInfo != null)
			{
				obj = cond.FieldInfo.GetValue(classInstance);
			}
			else if (cond.PropertyInfo != null)
			{
				obj = cond.PropertyInfo.GetValue(classInstance, null);
			}
			if (obj == null)
			{
				return cond.CompareTo == null && !cond.CompareFalse;
			}
			return obj.Equals(cond.CompareTo) == !cond.CompareFalse;
		}
	}
}
