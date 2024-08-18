using System;
using UnityEngine;

namespace Noodle
{
	internal class DialogInterfaceOnClickListener : AndroidJavaProxy
	{
		public Action Callback;

		public DialogInterfaceOnClickListener(Action callback)
			: base("android.content.DialogInterface$OnClickListener")
		{
			Callback = callback;
		}

		public void onClick(AndroidJavaObject dialog, int which)
		{
			if (Callback != null)
			{
				Callback();
			}
		}
	}
}
