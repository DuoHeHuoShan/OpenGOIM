using System.Runtime.InteropServices;
using UnityEngine;

public class NoodleNewsClient : MonoBehaviour
{
	private static AndroidJavaClass noodleNewsClientClass = new AndroidJavaClass("com.noodlecake.noodlenews.NoodleNewsClient");

	private static bool _initialized = false;

	private static string _campaignFromLastIntent;

	public static event WillShowCreative OnWillShowCreative;

	public static event DidDismissCreative OnDidDismissCreative;

	public static event WillShowMoreGames OnWillShowMoreGames;

	public static event DidDismissMoreGames OnDidDismissMoreGames;

	[DllImport("nativeNoodleNews")]
	private static extern void setNewsCreativeShowCallback(WillShowCreative showCallback);

	[DllImport("nativeNoodleNews")]
	private static extern void setNewsCreativeDismissCallback(DidDismissCreative dismissCallback);

	[DllImport("nativeNoodleNews")]
	private static extern void setNewsMoreGamesShowCallback(WillShowMoreGames showCallback);

	[DllImport("nativeNoodleNews")]
	private static extern void setNewsMoreGamesDismissCallback(DidDismissMoreGames dismissCallback);

	public static void StartSession(string platform)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getIntent", new object[0]);
		if (!_initialized)
		{
			noodleNewsClientClass.CallStatic("startSession", @static, platform);
			_initialized = true;
		}
		else if (androidJavaObject != null)
		{
			string text = androidJavaObject.Call<string>("getStringExtra", new object[1] { "notification_campaign_id" });
			if (text != _campaignFromLastIntent)
			{
				_campaignFromLastIntent = text;
				noodleNewsClientClass.CallStatic("onNewIntent", androidJavaObject);
			}
		}
		setNewsCreativeShowCallback(InternalWillShowCreativeCallback);
		setNewsCreativeDismissCallback(InternalDidDismissCreativeCallback);
		setNewsMoreGamesShowCallback(InternalWillShowMoreGamesCallback);
		setNewsMoreGamesDismissCallback(InternalDidDismissMoreGamesCallback);
	}

	public static void SetDebugLoggingEnabled(bool enabled)
	{
		noodleNewsClientClass.CallStatic("setDebugLoggingEnabled", enabled);
	}

	public static void InternalWillShowCreativeCallback(bool shouldMuteAudio)
	{
		if (NoodleNewsClient.OnWillShowCreative != null)
		{
			NoodleNewsClient.OnWillShowCreative(shouldMuteAudio);
		}
	}

	public static void InternalDidDismissCreativeCallback(bool positiveActionTaken)
	{
		if (NoodleNewsClient.OnDidDismissCreative != null)
		{
			NoodleNewsClient.OnDidDismissCreative(positiveActionTaken);
		}
	}

	public static void InternalWillShowMoreGamesCallback()
	{
		if (NoodleNewsClient.OnWillShowMoreGames != null)
		{
			NoodleNewsClient.OnWillShowMoreGames();
		}
	}

	public static void InternalDidDismissMoreGamesCallback()
	{
		if (NoodleNewsClient.OnDidDismissMoreGames != null)
		{
			NoodleNewsClient.OnDidDismissMoreGames();
		}
	}

	public static void ShowCreative()
	{
		bool explicitRequest = false;
		ShowCreative(explicitRequest);
	}

	public static void ShowCreative(bool explicitRequest)
	{
		noodleNewsClientClass.CallStatic("showCreative", explicitRequest);
	}

	public static void ShowMoreGames()
	{
		noodleNewsClientClass.CallStatic("showMoreGames");
	}

	public static bool ShowPushCampaign()
	{
		bool flag = false;
		flag = noodleNewsClientClass.CallStatic<bool>("showPushCampaign", new object[0]);
		Debug.Log("[NoodleNews] Tried showing pushed campaign - " + ((!flag) ? "NO" : "YES"));
		return flag;
	}

	public static string GetSupportIdentifier()
	{
		string text = null;
		return noodleNewsClientClass.CallStatic<string>("getSupportIdentifier", new object[0]);
	}

	public static string GetSupportResponseContent(int id)
	{
		string text = null;
		return noodleNewsClientClass.CallStatic<string>("getSupportResponseContent", new object[1] { id });
	}

	public static int GetSupportResponseId()
	{
		int num = -1;
		return noodleNewsClientClass.CallStatic<int>("getSupportResponseID", new object[0]);
	}

	public static void AcknowledgeSupportResponse(int id)
	{
		noodleNewsClientClass.CallStatic("acknowledgeSupportResponse", id);
	}

	public static bool HasPendingCreative()
	{
		bool explicitRequest = false;
		return HasPendingCreative(explicitRequest);
	}

	public static bool HasPendingCreative(bool explicitRequest)
	{
		bool flag = false;
		return noodleNewsClientClass.CallStatic<bool>("hasPendingCreative", new object[1] { explicitRequest });
	}

	public static bool HasPushCampaign()
	{
		bool flag = false;
		return noodleNewsClientClass.CallStatic<bool>("hasPushCampaign", new object[0]);
	}

	public static int AvailableCampaignCount()
	{
		int num = 0;
		return noodleNewsClientClass.CallStatic<int>("availableCampaignCount", new object[0]);
	}

	public static string GetNativeCreative()
	{
		string text = null;
		return noodleNewsClientClass.CallStatic<string>("getNativeCreative", new object[0]);
	}

	public static void HitNativeCreative(int creativeId)
	{
		noodleNewsClientClass.CallStatic("hitNativeCreative", creativeId);
	}

	public static void DismissedNativeCreative(int creativeId)
	{
		noodleNewsClientClass.CallStatic("dismissedNativeCreative", creativeId);
	}

	public static void ResetCreativeViews()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.noodlecake.noodlenews.NoodleNewsClient$Debug");
		androidJavaClass.CallStatic("resetCreativeViews");
	}
}
