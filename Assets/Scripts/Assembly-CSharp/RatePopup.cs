using System;
using UnityEngine;

public class RatePopup : MonoBehaviour
{
	public ProgressMeter progressMeter;

	public float minProgressForRating;

	public bool ShouldPrompt()
	{
		return PlayerPrefs.GetInt("noodle.didPromptPlayerToReviewGame", 0) == 0 && progressMeter.progress >= minProgressForRating && (DateTimeOffset.UtcNow - progressMeter.LastSeriousLossTimestamp).TotalSeconds > 60.0;
	}

	private void OnEnable()
	{
		PlayerPrefs.SetInt("noodle.didPromptPlayerToReviewGame", 1);
	}

	public void ReviewGame()
	{
		base.gameObject.SetActive(false);
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.noodlecake.gettingoverit");
	}
}
