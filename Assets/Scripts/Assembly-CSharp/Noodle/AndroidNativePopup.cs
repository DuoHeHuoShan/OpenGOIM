using System;
using UnityEngine;

namespace Noodle
{
	public static class AndroidNativePopup
	{
		public static void Show(string title, string message, string buttonText, Action onClose)
		{
			if (Application.platform != RuntimePlatform.Android)
			{
				Debug.LogFormat("{0} : {1}", title, message);
				return;
			}
			using (AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
			{
				using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("android.app.AlertDialog$Builder", androidJavaObject))
				{
					androidJavaObject2.Call<AndroidJavaObject>("setTitle", new object[1] { title }).Call<AndroidJavaObject>("setMessage", new object[1] { message }).Call<AndroidJavaObject>("setCancelable", new object[1] { false })
						.Call<AndroidJavaObject>("setPositiveButton", new object[2] { buttonText, null })
						.Call<AndroidJavaObject>("setOnDismissListener", new object[1]
						{
							new DialogInterfaceOnDismissListener(onClose)
						})
						.Call<AndroidJavaObject>("show", new object[0]);
				}
			}
		}

		public static void Show(string title, string message, string positiveButtonText, string negativeButtonText, Action<bool?> onClose)
		{
			if (Application.platform != RuntimePlatform.Android)
			{
				Debug.LogFormat("{0} : {1}", title, message);
				return;
			}
			Action callback = delegate
			{
				onClose(true);
			};
			Action callback2 = delegate
			{
				onClose(false);
			};
			Action callback3 = delegate
			{
				onClose(null);
			};
			using (AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
			{
				using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("android.app.AlertDialog$Builder", androidJavaObject))
				{
					androidJavaObject2.Call<AndroidJavaObject>("setTitle", new object[1] { title }).Call<AndroidJavaObject>("setMessage", new object[1] { message }).Call<AndroidJavaObject>("setCancelable", new object[1] { false })
						.Call<AndroidJavaObject>("setPositiveButton", new object[2]
						{
							positiveButtonText,
							new DialogInterfaceOnClickListener(callback)
						})
						.Call<AndroidJavaObject>("setNegativeButton", new object[2]
						{
							negativeButtonText,
							new DialogInterfaceOnClickListener(callback2)
						})
						.Call<AndroidJavaObject>("setOnDismissListener", new object[1]
						{
							new DialogInterfaceOnDismissListener(callback3)
						})
						.Call<AndroidJavaObject>("show", new object[0]);
				}
			}
		}
	}
}
