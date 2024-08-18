using System;
using UnityEngine;

namespace Noodle
{
	internal class DialogInterfaceOnDismissListener : AndroidJavaProxy
	{
		public Action Callback;

		public DialogInterfaceOnDismissListener(Action callback)
			: base("android.content.DialogInterface$OnDismissListener")
		{
			Callback = callback;
		}

		public void onDismiss(AndroidJavaObject dialog)
		{
			if (Callback != null)
			{
				Callback();
			}
		}
	}
}
