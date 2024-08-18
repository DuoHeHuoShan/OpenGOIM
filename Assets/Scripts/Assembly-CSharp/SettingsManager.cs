using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	public GameObject menu;

	public Toggle subtitleToggle;

	public Slider mouseSensitivitySlider;

	public Slider SFXVolumeSlider;

	public Slider MusicVolumeSlider;

	public Slider VOVolumeSlider;

	public GameObject player;

	public GameObject cursor;

	public Narrator narrator;

	public GameObject menuMobile;

	public Toggle subtitleToggleMobile;

	public Slider mouseSensitivitySliderMobile;

	public Slider SFXVolumeSliderMobile;

	public Slider MusicVolumeSliderMobile;

	public Slider VOVolumeSliderMobile;

	public TMP_Dropdown LanguageDropdown;

	public AudioMixer mixer;

	private float mouseSensitivity;

	public GameObject resetGame1;

	public GameObject resetGame2;

	public TextMeshProUGUI progressLabel;

	public ProgressMeter progressMeter;

	public Camera fgCam;

	public Camera bgCam;

	public bool canMenu;

	private Resolution currentRes;

	private Resolution newRes;

	private int currentLanguageNum;

	public Toggle[] languageToggles;

	[Header("-------------------------------------------------------------")]
	[Space(4f)]
	public TextMeshProUGUI inputText;

	public TextMeshProUGUI AVText;

	public TextMeshProUGUI streamingText;

	public GameObject inputPanel;

	public GameObject AVPanel;

	public GameObject streamingPanel;

	public GameObject SkipCreditsButton;

	public GameObject TrySkipCreditsButton;

	public Toggle camToggle;

	public Toggle micToggle;

	public ReplayKitManager replayKit;

	public TextMeshProUGUI URLText;

	public GamecenterManager gcManager;

	private MobileManager mobileMan;

	private ScreenFader ScreenFader;

	public Button QuitButton;

	public GameObject QuitConfirmationWindow;

	public GameObject ResetGameConfirmationWindow;

	public RatePopup RatePopupWindow;

	public TextMeshProUGUI BestTime;

	public GameObject BestTimeLabel;

	public TextMeshProUGUI NumberOfWins;

	public GameObject NumberOfWinsLabel;

	public GameObject WaitOverlay;

	[Header("-------------------------------------------------------------")]
	[Space(4f)]
	// public ProceduralMaterial potMat;
	public Material potMat;

	public bool IsMenuActive
	{
		get
		{
			return menu.activeSelf;
		}
	}

	public bool IsQuitConfirmationWindowOpen
	{
		get
		{
			return QuitConfirmationWindow.activeSelf;
		}
	}

	public bool IsResetGameConfirmationWindowOpen
	{
		get
		{
			return ResetGameConfirmationWindow.activeSelf;
		}
	}

	public bool IsRatePopupWindowOpen
	{
		get
		{
			return RatePopupWindow.gameObject.activeSelf;
		}
	}

	public void SetLanguage(int newLang)
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages();
		if (LocalizationManager.HasLanguage(allLanguages[newLang]))
		{
			LocalizationManager.CurrentLanguage = allLanguages[newLang];
		}
		OnLanguageChanged(newLang);
	}

	public void OnLanguageChanged(int newLanguage)
	{
		if (newLanguage != currentLanguageNum)
		{
			currentLanguageNum = newLanguage;
			PlayerPrefs.SetInt("Language", newLanguage);
			PlayerPrefs.Save();
			if (narrator != null)
			{
				narrator.SendMessage("SetLanguage", newLanguage);
			}
			LanguageDropdown.value = newLanguage;
		}
	}

	public void UpdateGoldPot()
	{
		if (PlayerPrefs.HasKey("NumWins"))
		{
			int @int = PlayerPrefs.GetInt("NumWins");
			if (potMat != null)
			{
				float num = (float)Mathf.Min(@int, 50) / 50f;
				num *= num;
				// potMat.SetProceduralFloat("Goldness", num);
				// potMat.RebuildTextures();
				potMat.SetFloat("_Goldness", num);
			}
		}
	}

	private void Start()
	{
		mobileMan = GameObject.FindGameObjectWithTag("MobileManager").GetComponent<MobileManager>();
		UpdateGoldPot();
		menu = menuMobile;
		subtitleToggle = subtitleToggleMobile;
		mouseSensitivitySlider = mouseSensitivitySliderMobile;
		SFXVolumeSlider = SFXVolumeSliderMobile;
		MusicVolumeSlider = MusicVolumeSliderMobile;
		VOVolumeSlider = VOVolumeSliderMobile;
		DisableSkipCredits();
		if (ScreenFader == null)
		{
			ScreenFader = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
		PlayerPrefs.SetInt("NativeWidth", Screen.currentResolution.width);
		PlayerPrefs.SetInt("NativeHeight", Screen.currentResolution.height);
		PlayerPrefs.SetInt("NativeRefresh", Screen.currentResolution.refreshRate);
		canMenu = true;
		int num = 0;
		currentLanguageNum = -1;
		if (PlayerPrefs.HasKey("Language"))
		{
			num = PlayerPrefs.GetInt("Language");
		}
		else
		{
			num = -1;
			switch (Application.systemLanguage)
			{
			case SystemLanguage.English:
				num = 0;
				break;
			case SystemLanguage.Russian:
				num = 1;
				break;
			case SystemLanguage.Japanese:
				num = 2;
				break;
			case SystemLanguage.ChineseSimplified:
				num = 3;
				break;
			case SystemLanguage.ChineseTraditional:
				num = 0;
				break;
			case SystemLanguage.Chinese:
				num = 3;
				break;
			case SystemLanguage.Korean:
				num = 4;
				break;
			default:
				num = 0;
				break;
			}
			PlayerPrefs.SetInt("Language", num);
		}
		SetLanguage(num);
		LanguageDropdown.onValueChanged.AddListener(delegate(int index)
		{
			SetLanguage(index);
		});
		if (PlayerPrefs.HasKey("MouseSensitivity"))
		{
			mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
			SetMouseSensitivity(mouseSensitivity);
			mouseSensitivitySlider.value = mouseSensitivity;
		}
		else
		{
			mouseSensitivity = 1f;
			PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
			SetMouseSensitivity(mouseSensitivity);
			mouseSensitivitySlider.value = mouseSensitivity;
		}
		if (PlayerPrefs.HasKey("SubtitlesOn"))
		{
			subtitleToggle.isOn = PlayerPrefs.GetInt("SubtitlesOn") == 1;
			ToggleSubtitles();
		}
		else
		{
			PlayerPrefs.SetInt("SubtitlesOn", 1);
			subtitleToggle.isOn = true;
			ToggleSubtitles();
		}
		if (PlayerPrefs.HasKey("SFXVolume"))
		{
			mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVolume"));
			mixer.SetFloat("AmbienceVol", PlayerPrefs.GetFloat("SFXVolume"));
			SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
		}
		else
		{
			PlayerPrefs.SetFloat("SFXVolume", SFXVolumeSlider.value);
		}
		if (PlayerPrefs.HasKey("MusicVolume"))
		{
			mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVolume") * 0.9f);
			MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
		}
		else
		{
			PlayerPrefs.SetFloat("MusicVolume", MusicVolumeSlider.value);
		}
		if (PlayerPrefs.HasKey("VoiceVolume"))
		{
			mixer.SetFloat("VoiceVol", PlayerPrefs.GetFloat("VoiceVolume") * 0.8f);
			VOVolumeSlider.value = PlayerPrefs.GetFloat("VoiceVolume");
		}
		else
		{
			PlayerPrefs.SetFloat("VoiceVolume", VOVolumeSlider.value);
		}
		PlayerPrefs.Save();
	}

	public void SetSFXVolume(float newVolume)
	{
		mixer.SetFloat("SFXVol", newVolume);
		mixer.SetFloat("AmbienceVol", newVolume);
		PlayerPrefs.SetFloat("SFXVolume", newVolume);
		PlayerPrefs.Save();
	}

	public void SetVoiceoverVolume(float newVolume)
	{
		mixer.SetFloat("VoiceVol", newVolume * 0.8f);
		PlayerPrefs.SetFloat("VoiceVolume", newVolume);
		PlayerPrefs.Save();
	}

	public void SetMusicVolume(float newVolume)
	{
		mixer.SetFloat("MusicVol", newVolume * 0.9f);
		PlayerPrefs.SetFloat("MusicVolume", newVolume);
		PlayerPrefs.Save();
	}

	public void showReset1()
	{
		if (resetGame1 != null && resetGame2 != null)
		{
			resetGame1.SetActive(true);
			resetGame2.SetActive(false);
		}
	}

	public void showReset2()
	{
		if (resetGame1 != null && resetGame2 != null)
		{
			resetGame2.SetActive(true);
			resetGame1.SetActive(false);
		}
	}

	public void newGame()
	{
		PlayerPrefs.DeleteKey("NumSaves");
		PlayerPrefs.DeleteKey("SaveGame0");
		PlayerPrefs.DeleteKey("SaveGame1");
		PlayerPrefs.Save();
		Time.timeScale = 0f;
		ScreenFader.EndScene(ScreenFader.ScreenFaderExitType.ReloadMain);
		menu.SetActive(!menu.activeSelf);
	}

	private IEnumerator ResetDone()
	{
		int wait = 30;
		while (wait > 0)
		{
			wait--;
			yield return null;
		}
		Time.timeScale = 1f;
	}

	public void SetMouseSensitivity(float newSensitivity)
	{
		if (player != null)
		{
			player.SendMessage("SetSensitivity", newSensitivity);
		}
		mouseSensitivitySlider.value = newSensitivity;
		PlayerPrefs.SetFloat("MouseSensitivity", newSensitivity);
		PlayerPrefs.Save();
	}

	public void ToggleMenu()
	{
		if (!canMenu)
		{
			return;
		}
		menu.SetActive(!menu.activeSelf);
		Cursor.lockState = ((!menu.activeSelf) ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = menu.activeSelf;
		Time.timeScale = ((!menu.activeSelf) ? 1f : 0f);
		if (menu.activeSelf && narrator != null)
		{
			narrator.SendMessage("Pause");
		}
		else
		{
			narrator.SendMessage("UnPause");
		}
		if (menu.activeSelf && player != null)
		{
			player.SendMessage("Pause");
		}
		else
		{
			player.SendMessage("UnPause");
		}
		if (menu.activeSelf)
		{
			progressLabel.text = progressMeter.progress.ToString("0.0");
			showBestWinsIfWon();
			if (RatePopupWindow.ShouldPrompt())
			{
				RatePopupWindow.gameObject.SetActive(true);
			}
		}
		showReset1();
	}

	private void showBestWinsIfWon()
	{
		int @int = PlayerPrefs.GetInt("NumWins", 0);
		if (@int > 0)
		{
			NumberOfWins.gameObject.SetActive(true);
			NumberOfWins.text = @int.ToString();
			float @float = PlayerPrefs.GetFloat("BestTime", 0f);
			TimeSpan timeSpan = TimeSpan.FromSeconds(@float);
			string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			BestTime.gameObject.SetActive(true);
			BestTime.text = text;
		}
		else
		{
			NumberOfWins.gameObject.SetActive(false);
			BestTime.gameObject.SetActive(false);
		}
		NumberOfWinsLabel.SetActive(NumberOfWins.gameObject.activeSelf);
		BestTimeLabel.SetActive(BestTime.gameObject.activeSelf);
	}

	public void ToggleSubtitles()
	{
		bool isOn = subtitleToggle.isOn;
		if (narrator != null)
		{
			narrator.SendMessage("ToggleSubtitles", isOn);
		}
		PlayerPrefs.SetInt("SubtitlesOn", isOn ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void OpenQuitConfirmationWindow()
	{
		QuitConfirmationWindow.SetActive(true);
	}

	public void CloseQuitConfirmationWindow()
	{
		QuitConfirmationWindow.SetActive(false);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void OpenResetGameConfirmationWindow()
	{
		ResetGameConfirmationWindow.SetActive(true);
	}

	public void CloseResetGameConfirmationWindow()
	{
		ResetGameConfirmationWindow.SetActive(false);
	}

	public void ResetGame()
	{
		CloseResetGameConfirmationWindow();
		newGame();
	}

	public void CloseRatePopupWindow()
	{
		RatePopupWindow.gameObject.SetActive(false);
	}

	public void EnableSkipCredits()
	{
		TrySkipCreditsButton.SetActive(true);
	}

	public void DisableSkipCredits()
	{
		TrySkipCreditsButton.SetActive(false);
		SkipCreditsButton.SetActive(false);
	}

	public void TrySkipCredits()
	{
		TrySkipCreditsButton.SetActive(false);
		SkipCreditsButton.SetActive(true);
	}

	public void SkipCredits()
	{
		SceneManager.LoadScene("Reward Loader Mobile");
	}

	public void goToInput()
	{
		inputPanel.SetActive(true);
		AVPanel.SetActive(false);
		streamingPanel.SetActive(false);
		inputText.color = Color.white;
		AVText.color = new Color(0.7f, 0.7f, 0.7f);
		streamingText.color = new Color(0.7f, 0.7f, 0.7f);
	}

	public void goToAV()
	{
		inputPanel.SetActive(false);
		AVPanel.SetActive(true);
		streamingPanel.SetActive(false);
		inputText.color = new Color(0.7f, 0.7f, 0.7f);
		AVText.color = Color.white;
		streamingText.color = new Color(0.7f, 0.7f, 0.7f);
	}

	public void goToStreaming()
	{
		inputPanel.SetActive(false);
		AVPanel.SetActive(false);
		streamingPanel.SetActive(true);
		inputText.color = new Color(0.7f, 0.7f, 0.7f);
		AVText.color = new Color(0.7f, 0.7f, 0.7f);
		streamingText.color = Color.white;
	}

	public void setBeautiful()
	{
		StartCoroutine(SwitchToBatterySaverGraphics());
	}

	public void setSixty()
	{
		StartCoroutine(SwitchToBeautifulGraphics());
	}

	public void setBatterySave()
	{
		StartCoroutine(SwitchToSixtyFramesGraphics());
	}

	private IEnumerator SwitchToSixtyFramesGraphics()
	{
		WaitOverlay.SetActive(true);
		yield return null;
		try
		{
			mobileMan.SixtyFramesOn();
		}
		finally
		{
			WaitOverlay.SetActive(false);
		}
	}

	private IEnumerator SwitchToBatterySaverGraphics()
	{
		WaitOverlay.SetActive(true);
		yield return null;
		try
		{
			mobileMan.BatterySaverOn();
		}
		finally
		{
			WaitOverlay.SetActive(false);
		}
	}

	private IEnumerator SwitchToBeautifulGraphics()
	{
		WaitOverlay.SetActive(true);
		yield return null;
		try
		{
			mobileMan.BeautifulOn();
		}
		finally
		{
			WaitOverlay.SetActive(false);
		}
	}

	public void toggleCamera(bool camera)
	{
		camToggle.isOn = camera;
	}

	public void toggleMicrophone(bool microphone)
	{
		micToggle.isOn = microphone;
	}

	public void startStreaming()
	{
	}

	public void stopStreaming()
	{
	}

	public void shareURL()
	{
		ios_sharesheet.show(URLText.text);
	}

	public void showLeaderboards()
	{
		gcManager.showLeaderboards();
	}
}
