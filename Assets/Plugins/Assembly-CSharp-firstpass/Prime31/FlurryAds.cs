using UnityEngine;

namespace Prime31
{
	public class FlurryAds
	{
		private static AndroidJavaObject _plugin;

		static FlurryAds()
		{
			if (Application.platform != RuntimePlatform.Android)
			{
				return;
			}
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.prime31.ads.FlurryAds"))
			{
				_plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}

		public static void fetchInterstitial(string adSpace)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				_plugin.Call("fetchInterstitial", adSpace);
			}
		}

		public static void displayInterstitial(string adSpace = null)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				_plugin.Call("displayInterstitial", adSpace);
			}
		}

		public static bool isInterstitialAvailable(string adSpace = null)
		{
			if (Application.platform != RuntimePlatform.Android)
			{
				return false;
			}
			return _plugin.Call<bool>("isInterstitialAvailable", new object[1] { adSpace });
		}

		public static void fetchAndDisplayBannerAd(string adSpace, FlurryAdPlacement location)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				_plugin.Call("fetchAndDisplayBannerAd", adSpace, (int)location);
			}
		}

		public static void fetchBannerAd(string adSpace, FlurryAdPlacement location)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				_plugin.Call("fetchBannerAd", adSpace, (int)location);
			}
		}

		public static void displayBannerAd()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				_plugin.Call("displayBannerAd");
			}
		}

		public static void destroyBannerAd()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				_plugin.Call("destroyBannerAd");
			}
		}
	}
}
