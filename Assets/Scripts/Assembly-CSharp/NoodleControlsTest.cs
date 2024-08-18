using UnityEngine;
using UnityEngine.UI;

public class NoodleControlsTest : MonoBehaviour
{
	public Text label;

	public Text dpiLabel;

	private void Start()
	{
		Debug.Log("[NewScene] dpi1 = " + Screen.dpi);
		Screen.SetResolution(Screen.width / 4, Screen.height / 4, true, 60);
		Debug.Log("[NewScene] dpi2 = " + Screen.dpi);
	}

	private void Update()
	{
		if (Input.touchCount > 0)
		{
			Vector2 position = Input.GetTouch(0).position;
			label.text = string.Format("{0} x {1}", position.x, position.y);
		}
		else
		{
			label.text = "no touch";
		}
		dpiLabel.text = Screen.dpi.ToString();
	}
}
