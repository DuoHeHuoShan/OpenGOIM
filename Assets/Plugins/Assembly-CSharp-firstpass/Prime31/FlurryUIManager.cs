using System.Collections.Generic;
using UnityEngine;

namespace Prime31
{
	public class FlurryUIManager : MonoBehaviourGUI
	{
		private void OnGUI()
		{
			beginColumn();
			if (button("Start Flurry Session"))
			{
				FlurryAnalytics.startSession("JQVT87W7TGN5W7SWY2FH", true);
			}
			if (button("Log Timed Event"))
			{
				FlurryAnalytics.logEvent("timed", true);
			}
			if (button("End Timed Event"))
			{
				FlurryAnalytics.endTimedEvent("timed");
			}
			if (button("Log Event"))
			{
				FlurryAnalytics.logEvent("myFancyEvent");
			}
			if (button("Log Event with Params"))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("akey1", "value1");
				dictionary.Add("bkey2", "value2");
				dictionary.Add("ckey3", "value3");
				dictionary.Add("dkey4", "value4");
				FlurryAnalytics.logEvent("EventWithParams", dictionary);
			}
			if (button("Log Payment"))
			{
				FlurryAnalytics.logPayment("Candy", "yummy_candy_id", 1, 1.99, "USD", "123456789");
			}
			if (button("Log Page View"))
			{
				FlurryAnalytics.onPageView();
			}
			if (button("Log Error"))
			{
				FlurryAnalytics.onError("666", "bad things happend", "Exception");
			}
			endColumn(true);
			GUILayout.Label("Interstitial");
			if (button("Fetch Interstitial"))
			{
				FlurryAds.fetchInterstitial("InterstitialTest");
			}
			if (button("Is Interstitial Loaded?"))
			{
				bool flag = FlurryAds.isInterstitialAvailable("InterstitialTest");
				Debug.Log("is interstitial loaded? " + flag);
			}
			if (button("Display Interstitial"))
			{
				FlurryAds.displayInterstitial("InterstitialTest");
			}
			GUILayout.Label("Banners");
			if (button("Display Banner (bottom)"))
			{
				FlurryAds.fetchAndDisplayBannerAd("StandardBannerTestAd", FlurryAdPlacement.Bottom);
			}
			if (button("Display Banner (top)"))
			{
				FlurryAds.fetchAndDisplayBannerAd("StandardBannerTestAd", FlurryAdPlacement.Top);
			}
			if (button("Destroy Banner"))
			{
				FlurryAds.destroyBannerAd();
			}
			GUILayout.Space(10f);
			GUILayout.Label("Banners (manual)");
			if (button("Display Banner (Top)"))
			{
				FlurryAds.fetchBannerAd("StandardBannerTestAd", FlurryAdPlacement.Top);
			}
			if (button("Display Banner"))
			{
				FlurryAds.displayBannerAd();
			}
			endColumn();
		}
	}
}
