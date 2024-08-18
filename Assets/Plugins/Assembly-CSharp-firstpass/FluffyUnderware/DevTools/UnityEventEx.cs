using System.Reflection;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine.Events;

namespace FluffyUnderware.DevTools
{
	public class UnityEventEx<T0> : UnityEvent<T0>
	{
		private object mCallerList;

		private MethodInfo mCallsCount;

		private int mCount = -1;

		public void AddListenerOnce(UnityAction<T0> call)
		{
			RemoveListener(call);
			AddListener(call);
			CheckForListeners();
		}

		public bool HasListeners()
		{
			if (mCallsCount == null)
			{
				FieldInfo fieldInfo = typeof(UnityEventBase).FieldByName("m_Calls", false, true);
				if (fieldInfo != null)
				{
					mCallerList = fieldInfo.GetValue(this);
					if (mCallerList != null)
					{
						mCallsCount = mCallerList.GetType().PropertyByName("Count").GetGetMethod();
					}
				}
			}
			if (mCount == -1)
			{
				if (mCallerList != null && mCallsCount != null)
				{
					mCount = (int)mCallsCount.Invoke(mCallerList, null);
				}
				mCount += GetPersistentEventCount();
			}
			return mCount > 0;
		}

		public void CheckForListeners()
		{
			mCount = -1;
		}
	}
}
