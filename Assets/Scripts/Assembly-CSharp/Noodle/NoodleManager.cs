using System.Collections;
using System.Collections.Generic;
using Prime31;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Noodle
{
	public class NoodleManager : MonoBehaviour
	{
		[Header("Flurry IDs")]
		public string androidIdentifier;

		public bool flurryLogging;

		private bool DidGetPermissionResponse;

		private bool PermissionGranted = true;

		public static NoodleManager Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			// FlurryAnalytics.startSession(androidIdentifier, flurryLogging);
		}

		private void Start()
		{
			SceneManager.LoadScene("Mian");
		}

		private IEnumerator RequestAndroidPermissionForObb()
		{
			string androidVersion = GetAndroidVersionString();
			int androidApiVersion = GetAndroidApiVersion();
			Debug.LogFormat("Android VERSION.RELEASE = '{0}' VERSION.SDK_INT = '{1}'", androidVersion, androidApiVersion);
			if (androidApiVersion == 23 || androidApiVersion == 24 || androidApiVersion == 25)
			{
				NoodlePermissionGranter.PermissionRequestCallback = PermissionRequestCallback;
				NoodlePermissionGranter.GrantPermission(NoodlePermissionGranter.NoodleAndroidPermission.WRITE_EXTERNAL_STORAGE);
			}
			else
			{
				DidGetPermissionResponse = true;
			}
			yield return new WaitUntil(() => DidGetPermissionResponse);
		}

		private void PermissionRequestCallback(bool granted)
		{
			NoodlePermissionGranter.PermissionRequestCallback = null;
			PermissionGranted = granted;
			DidGetPermissionResponse = true;
			if (!granted)
			{
				Debug.LogError("[NoodlePermissionGranter] User denied permission. Expect problems.");
			}
		}

		public static string GetAndroidVersionString()
		{
			if(Application.platform != RuntimePlatform.Android) return "8.0";
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION"))
			{
				return androidJavaClass.GetStatic<string>("RELEASE");
			}
		}

		public static int GetAndroidApiVersion()
		{
			if(Application.platform != RuntimePlatform.Android) return 21;
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION"))
			{
				return androidJavaClass.GetStatic<int>("SDK_INT");
			}
		}

		public void SubmitNoodleEvent(string eventIdentifier)
		{
			// FlurryAnalytics.logEvent(eventIdentifier, false);
		}

		public void SubmitNoodleEvent(string eventIdentifier, Dictionary<string, string> parameters)
		{
			// FlurryAnalytics.logEvent(eventIdentifier, parameters, false);
		}
	}
}
