using TMPro;
using UnityEngine;

public class VersionReadout : MonoBehaviour
{
	private TextMeshProUGUI text;

	private void Start()
	{
		text = GetComponent<TextMeshProUGUI>();
		text.text = "version " + Application.version;
	}
}
