using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
	private float progress;

	private float lerpProgress;

	public Image humble;

	public Button continueButton;

	public Button newgameButton;

	public Image continueBar;

	public Image newgameBar;

	public Gradient loadingGradient;

	public Image fader;

	private bool canContinue;

	public ParticleSystem scrapeParticles;

	public Animator hammerAnim;

	public RectTransform menu;

	public AnimationCurve appearCurve;

	public RectTransform titleMask;

	public Transform rock;

	public Transform hammer;

	private Vector3 rockStartPos;

	private Vector3 titleStartPos;

	private bool introAnimDone;

	private int menuItemClicked;

	public GameObject fog;

	public AudioClip attachSound;

	public AudioClip releaseSound;

	public GameObject settingsMenuLeft;

	public GameObject settingsMenuRight;

	public GameObject mouseQuery;

	private bool shouldShowMouseQuery;

	private bool safeToClick;

	private void Start()
	{
		safeToClick = false;
		if (!PlayerPrefs.HasKey("Trackpad"))
		{
			shouldShowMouseQuery = true;
		}
		else if (Application.isEditor)
		{
			shouldShowMouseQuery = true;
		}
		else
		{
			shouldShowMouseQuery = false;
		}
		progress = 0f;
		lerpProgress = 0f;
		menuItemClicked = -1;
		canContinue = false;
		if (PlayerPrefs.GetString("SaveGame0").Length > 0)
		{
			canContinue = true;
		}
		if (PlayerPrefs.GetString("SaveGame1").Length > 0)
		{
			canContinue = true;
		}
		TextMeshProUGUI[] componentsInChildren = menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Color color = componentsInChildren[i].color;
			color.a = 0f;
			componentsInChildren[i].color = color;
		}
		continueBar.color = new Color(0f, 0f, 0f, 0f);
		newgameBar.color = new Color(0f, 0f, 0f, 0f);
		scrapeParticles.gameObject.SetActive(true);
		fog.SetActive(true);
		introAnimDone = false;
		titleMask.sizeDelta = new Vector2(0f, 216f);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		safeToClick = true;
		hammerAnim.Play("HammerUp");
	}

	public void Resume()
	{
		mouseQuery.SetActive(false);
	}

	private void Update()
	{
		if (!canContinue)
		{
			continueButton.interactable = false;
			continueButton.gameObject.SetActive(false);
		}
		lerpProgress = Mathf.Lerp(lerpProgress, progress, 0.1f);
		if (progress < 1f)
		{
			if (canContinue)
			{
				continueButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.LOADING;
				continueButton.interactable = true;
				continueBar.rectTransform.localScale = new Vector3(lerpProgress, 1f, 1f);
				if (introAnimDone)
				{
					continueBar.color = loadingGradient.Evaluate(lerpProgress);
				}
			}
			newgameButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.LOADING;
			newgameButton.interactable = false;
			newgameBar.rectTransform.localScale = new Vector3(lerpProgress, 1f, 1f);
			if (introAnimDone)
			{
				newgameBar.color = loadingGradient.Evaluate(lerpProgress);
			}
			return;
		}
		if (canContinue)
		{
			continueButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.CONTINUE_GAME;
			continueButton.interactable = true;
			continueBar.rectTransform.localScale = new Vector3(lerpProgress, 1f, 1f);
			if (introAnimDone)
			{
				continueBar.color = loadingGradient.Evaluate(lerpProgress);
			}
		}
		newgameButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.START_NEW_GAME;
		newgameButton.interactable = true;
		newgameBar.rectTransform.localScale = new Vector3(lerpProgress, 1f, 1f);
		if (introAnimDone)
		{
			newgameBar.color = loadingGradient.Evaluate(lerpProgress);
		}
	}

	public void SaveTrackpadSetting(bool trackpad)
	{
		PlayerPrefs.SetInt("Trackpad", trackpad ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void QuitGame()
	{
		if (!safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == "Quit")
			{
				menuItemClicked = i;
			}
		}
		StartCoroutine("FadeOutAnd", "DoQuit");
	}

	public void ShowSettings()
	{
		if (!safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == "Settings")
			{
				menuItemClicked = i;
			}
		}
		StartCoroutine("ShuntEverythingLeft");
		hammerAnim.Play("HammerDown");
	}

	private IEnumerator ShuntEverythingLeft()
	{
		StopCoroutine("RevealMenu");
		TextMeshProUGUI[] items = menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 0f; t <= 1.0001f; t += 0.05f)
		{
			titleMask.position = titleStartPos + new Vector3(Mathf.SmoothStep(0f, -10f, t), 0f, 0f);
			rock.position = rockStartPos + new Vector3(Mathf.SmoothStep(0f, -2f, t), 0f, 0f);
			for (int i = 0; i < items.Length; i++)
			{
				Color c = items[i].color;
				c.a = Mathf.Clamp01(1f - t);
				c.a *= c.a;
				items[i].color = c;
			}
			yield return null;
		}
		settingsMenuLeft.SetActive(true);
		settingsMenuRight.SetActive(true);
		introAnimDone = false;
	}

	public void HideSettings()
	{
		StartCoroutine("ShuntEverythingRight");
		hammerAnim.Play("HammerUp");
	}

	private IEnumerator ShuntEverythingRight()
	{
		titleMask.sizeDelta = new Vector2(0f, 216f);
		settingsMenuLeft.SetActive(false);
		settingsMenuRight.SetActive(false);
		for (float t = 1f; t >= -0.0001f; t -= 0.05f)
		{
			titleMask.position = titleStartPos + new Vector3(Mathf.SmoothStep(0f, -900f, t), 0f, 0f);
			rock.position = rockStartPos + new Vector3(Mathf.SmoothStep(0f, -2f, t), 0f, 0f);
			yield return null;
		}
	}

	public void ShowCredits()
	{
		if (!safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == "Credits")
			{
				menuItemClicked = i;
			}
		}
		StartCoroutine("FadeOutAnd", "DoCredits");
	}

	public void DoCredits()
	{
		SceneManager.LoadScene("Credits");
	}

	public void ContinueGame()
	{
		SceneManager.LoadScene("Mian");
	}

	public void StartGame()
	{
		SceneManager.LoadScene("Mian");
	}

	private IEnumerator FadeOutAnd(string cbName)
	{
		hammerAnim.Play("HammerDown");
		GetComponent<AudioSource>().PlayOneShot(releaseSound);
		scrapeParticles.Play();
		for (float f = 0f; f <= 1.001f; f += 0.05f)
		{
			Color c = fader.color;
			c.a = f;
			fader.color = c;
			TextMeshProUGUI[] items = menu.GetComponentsInChildren<TextMeshProUGUI>();
			for (int i = 0; i < items.Length; i++)
			{
				if (i != menuItemClicked)
				{
					Color color = items[i].color;
					color.a = appearCurve.Evaluate(1f - f);
					items[i].color = color;
				}
			}
			yield return null;
		}
		scrapeParticles.gameObject.SetActive(false);
		fog.SetActive(false);
		SendMessage(cbName);
	}

	public void DoQuit()
	{
		Application.Quit();
	}

	private IEnumerator FadeInTitle()
	{
		while (mouseQuery.activeSelf)
		{
			yield return null;
		}
		fader.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			Color c = fader.color;
			c.a = f;
			fader.color = c;
			yield return null;
		}
		safeToClick = true;
		hammerAnim.Play("HammerUp");
	}

	private IEnumerator RevealMenu()
	{
		TextMeshProUGUI[] items = menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 0f; t <= 1.0001f + 0.1f * ((float)items.Length - 1f); t += 0.01f)
		{
			Color barColor = loadingGradient.Evaluate(lerpProgress);
			for (int i = 0; i < items.Length; i++)
			{
				float a = appearCurve.Evaluate(Mathf.Clamp01(t - (float)i * 0.1f));
				Color color = items[i].color;
				color.a = a;
				items[i].color = color;
				if (items[i].transform.parent == continueBar.transform.parent)
				{
					continueBar.color = barColor;
				}
				else if (items[i].transform.parent == newgameBar.transform.parent)
				{
					newgameBar.color = barColor;
				}
			}
			yield return null;
		}
		introAnimDone = true;
	}

	private IEnumerator RevealTitle()
	{
		for (float t = 0f; t <= 1.0001f; t += 0.15f)
		{
			titleMask.sizeDelta = new Vector2(700f * t, 216f);
			yield return null;
		}
	}

	private IEnumerator FadeInHumble()
	{
		yield return null;
		if (shouldShowMouseQuery)
		{
			PlayerPrefs.SetInt("Trackpad", 1);
			GameObject.FindWithTag("SettingsManager").GetComponent<SettingsManager>().SetMouseSensitivity(1f);
		}
		StartCoroutine("FadeInTitle");
		humble.gameObject.SetActive(false);
	}

	public void EndIntroAnim()
	{
		StartCoroutine("RevealMenu");
		StartCoroutine("RevealTitle");
		rockStartPos = rock.transform.position;
		titleStartPos = titleMask.position;
		GetComponent<AudioSource>().PlayOneShot(attachSound);
	}

	public void SetLanguage(int newLang)
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages();
		LocalizationManager.CurrentLanguage = allLanguages[newLang];
	}
}
