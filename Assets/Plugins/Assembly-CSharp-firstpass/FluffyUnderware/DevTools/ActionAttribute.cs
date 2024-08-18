using System.Reflection;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class ActionAttribute : DTAttribute
	{
		public enum ActionEnum
		{
			Show = 0,
			Hide = 1,
			Enable = 2,
			Disable = 3,
			ShowInfo = 4,
			ShowWarning = 5,
			ShowError = 6,
			Callback = 7
		}

		public enum ActionPositionEnum
		{
			Above = 0,
			Below = 1
		}

		public ActionEnum Action = ActionEnum.Callback;

		public ActionPositionEnum Position = ActionPositionEnum.Below;

		public object ActionData;

		private MethodInfo mCallback;

		protected ActionAttribute(string actionData, ActionEnum action = ActionEnum.Callback)
			: base(50)
		{
			ActionData = actionData;
			Action = action;
		}

		public void Callback(object classInstance)
		{
			string text = ActionData as string;
			if (!string.IsNullOrEmpty(text))
			{
				if (mCallback == null)
				{
					mCallback = classInstance.GetType().MethodByName(text, true, true);
				}
				if (mCallback != null)
				{
					mCallback.Invoke(classInstance, null);
					return;
				}
				Debug.LogWarningFormat("[DevTools] Unable to find method '{0}' at class '{1}' !", text, classInstance.GetType().Name);
			}
		}
	}
}
