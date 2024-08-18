using TMPro;
using UnityEngine;

public class LoadingIndicatorFadeout : MonoBehaviour
{
	private TextMeshProUGUI loadingLabel;

	private float duration = 0.25f;

	private void Awake()
	{
		loadingLabel = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		float b = loadingLabel.alpha - Time.deltaTime / duration;
		loadingLabel.alpha = Mathf.Max(0f, b);
		if (loadingLabel.alpha < 0.1f)
		{
			base.gameObject.SetActive(false);
		}
	}
}
