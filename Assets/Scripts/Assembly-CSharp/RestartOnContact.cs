using UnityEngine;

public class RestartOnContact : MonoBehaviour
{
	public ScreenFader ScreenFader;

	private bool resetting;

	private void Start()
	{
		resetting = false;
		if (ScreenFader == null)
		{
			ScreenFader = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
	}

	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (!resetting && coll.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			ScreenFader.EndScene(ScreenFader.ScreenFaderExitType.ResetPlayerButNotDialogue);
			resetting = true;
			Invoke("ResetResetting", 1f);
		}
	}

	private void ResetResetting()
	{
		resetting = false;
	}
}
