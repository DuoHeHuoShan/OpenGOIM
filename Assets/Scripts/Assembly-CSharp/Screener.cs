using UnityEngine;

public class Screener : MonoBehaviour
{
	private int num;

	private void Start()
	{
	}

	private void Update()
	{
		if (Application.isEditor && Input.GetMouseButtonDown(0))
		{
			Debug.Log("trying to capture screen");
			ScreenCapture.CaptureScreenshot("gettingitscreen" + num);
			num++;
		}
	}
}
