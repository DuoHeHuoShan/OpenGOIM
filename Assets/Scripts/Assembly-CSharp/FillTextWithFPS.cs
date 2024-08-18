using TMPro;
using UnityEngine;

public class FillTextWithFPS : MonoBehaviour
{
	public MobileManager mobileManager;

	private TextMeshProUGUI label;

	private void Start()
	{
		label = GetComponent<TextMeshProUGUI>();
		InvokeRepeating("UpdateFPSLabel", 0.5f, 0.5f);
	}

	private void UpdateFPSLabel()
	{
		label.text = mobileManager.m_CurrentFps.ToString("0");
	}
}
