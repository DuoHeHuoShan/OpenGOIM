using System;
using TMPro;
using UnityEngine;

public class SetLanguageFont : MonoBehaviour
{
	[Serializable]
	public struct TextFont
	{
		public string Text;

		public TMP_FontAsset Font;
	}

	public TextFont[] Fonts;

	private TextMeshProUGUI Text;

	private string lastText = string.Empty;

	private void Start()
	{
		Text = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (Text.text != lastText)
		{
			UpdateFont();
			lastText = Text.text;
		}
	}

	private void UpdateFont()
	{
		TextFont[] fonts = Fonts;
		for (int i = 0; i < fonts.Length; i++)
		{
			TextFont textFont = fonts[i];
			if (Text.text == textFont.Text && Text.font != textFont.Font)
			{
				Text.font = textFont.Font;
				break;
			}
		}
	}
}
