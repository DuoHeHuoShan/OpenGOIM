using UnityEngine;

public class Scroller : MonoBehaviour
{
	private RectTransform canvas;

	private RectTransform myRect;

	public bool inGame;

	private float scrollDelta;

	private AudioSource aud;

	public Texture2D fadeTex;

	public Narrator narrator;

	public AudioClip endgameClip;

	public AudioClip menuClip;

	private ScreenFader fader;

	private bool didTriggerEndScene;

	private void Start()
	{
		if (canvas == null)
		{
			canvas = base.transform.parent.GetComponentInParent<RectTransform>();
		}
		if (fader == null)
		{
			fader = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
		myRect = GetComponent<RectTransform>();
		aud = GetComponent<AudioSource>();
		aud.clip = endgameClip;
		scrollDelta = 0.0071f;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Narrator");
		if (gameObject != null)
		{
			narrator = gameObject.GetComponent<Narrator>();
		}
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		position.y += scrollDelta * 60f * Time.deltaTime;
		base.transform.position = position;
		float y = myRect.TransformPoint(myRect.rect.min).y;
		float y2 = canvas.TransformPoint(canvas.rect.max).y;
		if (y > y2 && !didTriggerEndScene)
		{
			aud.Stop();
			fader.EndScene(ScreenFader.ScreenFaderExitType.LoadReward);
			didTriggerEndScene = true;
		}
	}
}
