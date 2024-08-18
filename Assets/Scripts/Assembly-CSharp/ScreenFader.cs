using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
	public enum ScreenFaderExitType
	{
		ReloadMain = 0,
		ResetPlayerButNotDialogue = 1,
		LoadReward = 2,
		Default = 3
	}

	public MeshRenderer rend;

	public TitleFade Title;

	public Saviour saveManager;

	public float fadeDuration = 1f;

	private float fadeInStartTime;

	private float fadeOutStartTime;

	private float fadeInProgress;

	private float fadeOutProgress;

	private static Material mat;

	private static int shaderInt;

	private bool fadingIn;

	private bool fadingOut;

	private bool fadeInInterrupted;

	private bool fadeOutInterrupted;

	private Color32 prevColor;

	private void Awake()
	{
		rend.transform.localScale = new Vector3(2000f, 2000f, 1f);
		mat = rend.sharedMaterial;
		Shader.EnableKeyword("_Color");
		shaderInt = Shader.PropertyToID("_Color");
		mat.SetColor(shaderInt, Color.black);
		rend.enabled = true;
	}

	private void Start()
	{
		if (saveManager == null)
		{
			saveManager = GameObject.FindGameObjectWithTag("Player").GetComponent<Saviour>();
		}
	}

	private void FadeToClear()
	{
		prevColor = mat.GetColor(shaderInt);
		mat.SetColor(shaderInt, Color.Lerp(prevColor, Color.clear, fadeInProgress / fadeDuration));
	}

	private void FadeToBlack()
	{
		prevColor = mat.GetColor(shaderInt);
		mat.SetColor(shaderInt, Color.Lerp(prevColor, Color.black, fadeOutProgress / fadeDuration));
	}

	public void StartScene()
	{
		if (!fadingOut)
		{
			fadeInProgress = 0f;
			fadeInStartTime = Time.realtimeSinceStartup;
			mat.SetColor(shaderInt, Color.black);
		}
		else
		{
			fadeInInterrupted = true;
		}
		fadingIn = true;
		StartCoroutine("StartSceneRoutine");
	}

	private IEnumerator StartSceneRoutine()
	{
		rend.enabled = true;
		while (true)
		{
			if (fadingOut)
			{
				yield return null;
			}
			if (fadeInInterrupted)
			{
				fadeInInterrupted = false;
				fadeInProgress = 0f;
				fadeInStartTime = Time.realtimeSinceStartup;
				fadingIn = true;
				mat.SetColor(shaderInt, Color.black);
			}
			FadeToClear();
			fadeInProgress = Time.realtimeSinceStartup - fadeInStartTime;
			if (fadeInProgress >= 1f)
			{
				break;
			}
			yield return null;
		}
		mat.SetColor(shaderInt, Color.clear);
		rend.enabled = false;
		fadingIn = false;
		fadeInProgress = 0f;
		Time.timeScale = 1f;
	}

	public IEnumerator EndSceneRoutine(ScreenFaderExitType exit)
	{
		while (true)
		{
			if (fadingIn)
			{
				yield return null;
			}
			if (fadeOutInterrupted)
			{
				fadeOutInterrupted = false;
				fadeOutStartTime = Time.realtimeSinceStartup;
				fadeOutProgress = 0f;
				rend.enabled = true;
				mat.SetColor(shaderInt, Color.clear);
			}
			FadeToBlack();
			fadeOutProgress = Time.realtimeSinceStartup - fadeOutStartTime;
			if (fadeOutProgress >= 1f)
			{
				break;
			}
			yield return null;
		}
		switch (exit)
		{
		case ScreenFaderExitType.ReloadMain:
			SceneManager.LoadScene("Mian");
			break;
		case ScreenFaderExitType.LoadReward:
			SceneManager.LoadScene("Reward Loader Mobile");
			break;
		case ScreenFaderExitType.ResetPlayerButNotDialogue:
			saveManager.ResetPlayerButNotDialogue();
			break;
		}
	}

	public void EndScene(ScreenFaderExitType exit)
	{
		if (!fadingIn)
		{
			fadeOutStartTime = Time.realtimeSinceStartup;
			fadeOutProgress = 0f;
			rend.enabled = true;
			fadeOutInterrupted = false;
			mat.SetColor(shaderInt, Color.clear);
		}
		else
		{
			fadeOutInterrupted = true;
		}
		fadingOut = true;
		StartCoroutine("EndSceneRoutine", exit);
	}
}
