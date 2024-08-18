using System;
using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RewardLogic : MonoBehaviour
{
	public GameObject browserMenu;

	public GameObject nameMenu;

	public GameObject gatePanel;

	public Toggle disclaimer;

	public Button yes;

	public TextMeshProUGUI timeText;

	public Image fader;

	private float mostRecentTime;

	private bool giftshopOpen;

	private void Start()
	{
		mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		TimeSpan timeSpan = TimeSpan.FromSeconds(mostRecentTime);
		string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D3}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
		timeText.text = ScriptLocalization.CLEAR_TIME + text;
		StartCoroutine("FadeUp");
		Screen.SetResolution(PlayerPrefs.GetInt("NativeWidth"), PlayerPrefs.GetInt("NativeHeight"), Screen.fullScreen, PlayerPrefs.GetInt("NativeRefresh"));
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		saveBestTime();
	}

	public void saveBestTime()
	{
		float num = ((!PlayerPrefs.HasKey("LastTime")) ? 999999f : PlayerPrefs.GetFloat("LastTime"));
		float num2 = ((!PlayerPrefs.HasKey("BestTime")) ? num : PlayerPrefs.GetFloat("BestTime"));
		if (num < num2)
		{
			num2 = num;
		}
		PlayerPrefs.SetFloat("BestTime", num2);
		PlayerPrefs.Save();
	}

	public void No()
	{
		SceneManager.LoadScene("Loader");
	}

	public void NoMobile()
	{
		StartCoroutine("FadeToQuit");
	}

	public void SendEmail()
	{
		mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		TimeSpan timeSpan = TimeSpan.FromSeconds(mostRecentTime);
		string s = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
		Application.OpenURL(string.Format("mailto:{0}?subject={1}", "bennett.foddy@foddy.net", "I%20Got%20Over%20It%20with%20a%20time%20of%20" + WWW.EscapeURL(s)));
	}

	public void ReviewThisGame()
	{
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.noodlecake." + Application.identifier);
	}

	private IEnumerator FadeUp()
	{
		fader.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			Color c = fader.color;
			c.a = f;
			fader.color = c;
			yield return null;
		}
	}

	private IEnumerator FadeToQuit()
	{
		fader.color = new Color(0f, 0f, 0f, 0f);
		for (float f = 0f; f <= 1.01f; f += 0.05f)
		{
			Color c = fader.color;
			c.a = f;
			fader.color = c;
			yield return null;
		}
		SceneManager.LoadScene("Mian");
	}

	public void EnableYes(bool shouldEnable)
	{
		yes.interactable = shouldEnable;
	}

	private void Update()
	{
	}

	public void GiftShop()
	{
		if (!giftshopOpen)
		{
			Application.OpenURL("http://goo.gl/PWcJPP");
			giftshopOpen = true;
		}
	}
}
