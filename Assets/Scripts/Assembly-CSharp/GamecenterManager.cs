using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GamecenterManager : MonoBehaviour
{
	public GameObject leaderboardsButton;

	public GameObject gamecenterButton;

	public bool loggedIn;

	public bool tryingToLogin;

	public int nClimbersAtTop;

	public int numberOfWinsLocal;

	public int numberOfWinsRemote;

	private SettingsManager settingsMan;

	private ILeaderboard winsLeaderboard;

	private string userID;

	private ILeaderboard scoresLeaderboard;

	private string[] userIDs;

	private bool isACheater;

	private void Start()
	{
		settingsMan = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
	}

	public void InitializeGameCenter()
	{
	}

	private void logIn()
	{
	}

	private void ProcessAuthentication(bool success)
	{
		if (success)
		{
			PlayerPrefs.SetInt("GCLogin", 1);
			PlayerPrefs.Save();
			loggedIn = true;
			tryingToLogin = false;
			userID = Social.localUser.id;
			userIDs = new string[1] { userID };
			numberOfWinsLocal = PlayerPrefs.GetInt("NumWins");
			reportScore();
			scoresLeaderboard = Social.CreateLeaderboard();
			scoresLeaderboard.id = "speedrun";
			scoresLeaderboard.userScope = UserScope.FriendsOnly;
			scoresLeaderboard.SetUserFilter(userIDs);
			scoresLeaderboard.LoadScores(delegate(bool loaded)
			{
				myScoreLoaded(loaded);
			});
			Social.LoadScores("speedrun", delegate(IScore[] scores)
			{
				scoresLoaded(scores);
			});
			winsLeaderboard = Social.CreateLeaderboard();
			winsLeaderboard.id = "numberofwins";
			winsLeaderboard.userScope = UserScope.FriendsOnly;
			winsLeaderboard.SetUserFilter(userIDs);
			winsLeaderboard.LoadScores(delegate(bool loaded)
			{
				winsLoaded(loaded);
			});
			gamecenterButton.SetActive(false);
			leaderboardsButton.SetActive(true);
		}
		else
		{
			tryingToLogin = false;
			gamecenterButton.SetActive(true);
			leaderboardsButton.SetActive(false);
		}
	}

	private void winsLoaded(bool loaded)
	{
		if (!loaded || winsLeaderboard.localUserScore == null)
		{
			return;
		}
		numberOfWinsRemote = (int)winsLeaderboard.localUserScore.value;
		if (numberOfWinsLocal > numberOfWinsRemote)
		{
			if (isACheater && (numberOfWinsLocal > 1 || numberOfWinsRemote > 1))
			{
				punishCheater();
			}
			Social.ReportScore(numberOfWinsLocal, "numberofwins", delegate(bool result)
			{
				if (!result)
				{
					Debug.LogWarning("Scores submission failed");
				}
			});
		}
		else if (numberOfWinsRemote > numberOfWinsLocal)
		{
			if (isACheater && (numberOfWinsLocal > 1 || numberOfWinsRemote > 1))
			{
				punishCheater();
			}
			PlayerPrefs.SetInt("NumWins", numberOfWinsRemote);
			if (settingsMan != null)
			{
				settingsMan.UpdateGoldPot();
			}
		}
	}

	private void myScoreLoaded(bool loaded)
	{
		if (loaded && scoresLeaderboard.localUserScore == null)
		{
			isACheater = true;
		}
	}

	private void punishCheater()
	{
		Social.ReportScore(-5507L, "numberofwins", delegate(bool result)
		{
			if (!result)
			{
				Debug.LogWarning("Scores submission failed");
			}
		});
	}

	private void scoresLoaded(IScore[] scores)
	{
		if (scores != null && scores.Length > 0)
		{
			nClimbersAtTop = scores.Length;
			PlayerPrefs.SetInt("playersAtTop", nClimbersAtTop);
			PlayerPrefs.Save();
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			logIn();
		}
	}

	public void reportScore()
	{
		if (!loggedIn)
		{
			logIn();
		}
		else
		{
			if (tryingToLogin || (!PlayerPrefs.HasKey("BestTime") && !PlayerPrefs.HasKey("LastTime")))
			{
				return;
			}
			float num = ((!PlayerPrefs.HasKey("BestTime")) ? PlayerPrefs.GetFloat("LastTime") : PlayerPrefs.GetFloat("BestTime"));
			num = Mathf.FloorToInt(num * 100f);
			if (num <= 1f)
			{
				Social.ReportScore(550755075507L, "speedrun", delegate(bool result)
				{
					if (!result)
					{
						Debug.LogWarning("Scores submission failed");
					}
				});
			}
			long score = (long)num;
			Social.ReportScore(score, "speedrun", delegate(bool result)
			{
				if (!result)
				{
					Debug.LogWarning("Scores submission failed");
				}
			});
		}
	}

	public void showLeaderboards()
	{
		if (loggedIn)
		{
			Social.ShowLeaderboardUI();
		}
		else
		{
			logIn();
		}
	}
}
