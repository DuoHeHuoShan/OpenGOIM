using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;

public class Narrator : MonoBehaviour
{
	[Serializable]
	public class DialogBit
	{
		public string textLine;

		public AudioClip audioLine;

		public bool heard;

		public float splinePos;

		public bool isMusic;

		public bool inUse;

		public bool isObservation;
	}

	public TextMeshProUGUI Subtitles;

	private List<string> currentSubtitleBuffer;

	private List<float> currentSubtitleTimeBuffer;

	public List<DialogBit> Condolences2 = new List<DialogBit>(1);

	public List<DialogBit> Observations2 = new List<DialogBit>(1);

	private List<DialogBit> KeyDialog;

	public CurvySpline levelSpline;

	public Transform keyDialogContainer;

	private float currentDistance;

	private bool showSubtitles;

	private float subtitleTimer;

	private float wordsPerSecond = 2.6f;

	private int[] charactersPerWord;

	public int lang;

	private List<DialogBit> dialogueQueue;

	private DialogBit lastBit;

	private bool paused;

	public AudioSource DJ;

	public AudioSource VO;

	public AudioSource Piano;

	private float targetPianoVolume;

	private float pianoVolumeTimer;

	private byte[] newKeyDialogDone;

	private byte[] newObservationDialogDone;

	private byte[] newCondolenceDialogDone;

	private int observationsSinceLastThing;

	private int maxObservationsPerOtherThing = 3;

	private float averageMouse;

	public float timePlayedThisGame;

	private int maxKey;

	private int currentKey;

	private DialogBit mouseBit;

	private DialogBit antennaBit;

	public AudioClip mouseClip;

	public AudioClip antennaClip;

	private int mouseClicks;

	public bool speedrun;

	private byte[] CondoluenceReturnval = new byte[0];

	private byte[] DialogDoneReturnval = new byte[0];

	private byte[] ObservationReturnval = new byte[0];

	private void AddNewObservation()
	{
		Observations2.Add(new DialogBit());
	}

	private void RemoveObservation(int index)
	{
		Observations2.RemoveAt(index);
	}

	private void AddNewCondolence()
	{
		Condolences2.Add(new DialogBit());
	}

	private void RemoveCondolence(int index)
	{
		Condolences2.RemoveAt(index);
	}

	public void Pause()
	{
		paused = true;
		if (DJ.isPlaying)
		{
			DJ.Pause();
		}
		if (VO.isPlaying)
		{
			VO.Pause();
		}
	}

	public void UnPause()
	{
		DJ.UnPause();
		VO.UnPause();
		paused = false;
	}

	public void setDialogDoneLists(byte[] keyDialogDone, byte[] observationDialogDone, byte[] condolenceDialogDone)
	{
		newKeyDialogDone = keyDialogDone;
		newObservationDialogDone = observationDialogDone;
		newCondolenceDialogDone = condolenceDialogDone;
	}

	public byte[] getKeyDialogDoneList()
	{
		for (int i = 0; i < KeyDialog.Count; i++)
		{
			DialogDoneReturnval[i] = (byte)(KeyDialog[i].heard ? 1 : 0);
		}
		return DialogDoneReturnval;
	}

	public byte[] getObservationDialogDoneList()
	{
		for (int i = 0; i < Observations2.Count; i++)
		{
			ObservationReturnval[i] = (byte)(Observations2[i].heard ? 1 : 0);
		}
		return ObservationReturnval;
	}

	public byte[] getCondolenceDialogDoneList()
	{
		for (int i = 0; i < Condolences2.Count; i++)
		{
			CondoluenceReturnval[i] = (byte)(Condolences2[i].heard ? 1 : 0);
		}
		return CondoluenceReturnval;
	}

	public void SetLanguage(int newlang)
	{
		lang = newlang;
	}

	private void Awake()
	{
		charactersPerWord = new int[5] { 5, 5, 3, 3, 3 };
		lang = 0;
		timePlayedThisGame = 0f;
		speedrun = false;
		observationsSinceLastThing = 0;
		showSubtitles = false;
		subtitleTimer = 0f;
		Subtitles.text = string.Empty;
		currentDistance = 0f;
		averageMouse = 0f;
		dialogueQueue = new List<DialogBit>();
		currentSubtitleBuffer = new List<string>();
		newKeyDialogDone = new byte[keyDialogContainer.childCount];
		newObservationDialogDone = new byte[Observations2.Count];
		newCondolenceDialogDone = new byte[Condolences2.Count];
		CondoluenceReturnval = new byte[Condolences2.Count];
		ObservationReturnval = new byte[Observations2.Count];
		targetPianoVolume = 0f;
		pianoVolumeTimer = 1f;
		antennaBit = new DialogBit();
		antennaBit.textLine = "DIALOG_STUCK_ANTENNA";
		antennaBit.audioLine = antennaClip;
		mouseBit = new DialogBit();
		mouseBit.textLine = "DIALOG_MOUSE_CLICKED";
		mouseBit.audioLine = mouseClip;
		if (PlayerPrefs.HasKey("Clicks"))
		{
			mouseClicks = PlayerPrefs.GetInt("Clicks");
			if (mouseClicks >= 500)
			{
				mouseBit.heard = true;
			}
		}
		else
		{
			mouseClicks = 0;
			PlayerPrefs.SetInt("Clicks", 0);
			PlayerPrefs.Save();
		}
		currentSubtitleBuffer = new List<string>();
		currentSubtitleTimeBuffer = new List<float>();
		mouseBit.heard = true;
		Observations2[5].heard = true;
	}

	private void LateUpdate()
	{
		if (newKeyDialogDone != null && newKeyDialogDone.Length == 0)
		{
			observationsSinceLastThing = maxObservationsPerOtherThing;
		}
		if (pianoVolumeTimer > 0f)
		{
			pianoVolumeTimer -= Time.deltaTime * 0.1f;
			if (pianoVolumeTimer < 0f)
			{
				pianoVolumeTimer = 0f;
			}
			if (Piano.volume < targetPianoVolume)
			{
				Piano.volume = Mathf.SmoothStep(1f, 0f, pianoVolumeTimer);
			}
			else if (Piano.volume > targetPianoVolume)
			{
				Piano.volume = Mathf.SmoothStep(0f, 1f, pianoVolumeTimer);
			}
		}
		averageMouse = Mathf.Lerp(averageMouse, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).sqrMagnitude, 0.01f);
		if (KeyDialog == null)
		{
			KeyDialog = new List<DialogBit>();
			DialogLine[] componentsInChildren = keyDialogContainer.GetComponentsInChildren<DialogLine>();
			foreach (DialogLine dialogLine in componentsInChildren)
			{
				DialogBit dialogBit = new DialogBit();
				dialogBit.audioLine = dialogLine.clip;
				dialogBit.textLine = dialogLine.subtitles;
				dialogBit.isMusic = false;
				dialogBit.splinePos = levelSpline.TFToDistance(levelSpline.GetNearestPointTF(dialogLine.transform.position));
				KeyDialog.Add(dialogBit);
			}
			KeyDialog.Sort((DialogBit x, DialogBit y) => x.splinePos.CompareTo(y.splinePos));
			DialogDoneReturnval = new byte[KeyDialog.Count];
		}
		if (KeyDialog != null && newKeyDialogDone != null && newKeyDialogDone.Length == KeyDialog.Count && newObservationDialogDone.Length == Observations2.Count && newCondolenceDialogDone.Length == Condolences2.Count)
		{
			for (int j = 0; j < newKeyDialogDone.Length; j++)
			{
				KeyDialog[j].heard = newKeyDialogDone[j] == 1;
				if (j > 0 && KeyDialog[j - 1].heard && !KeyDialog[j].heard)
				{
					maxKey = j - 1;
				}
			}
			for (int k = 0; k < newObservationDialogDone.Length; k++)
			{
				Observations2[k].heard = newObservationDialogDone[k] == 1;
			}
			for (int l = 0; l < newCondolenceDialogDone.Length; l++)
			{
				Condolences2[l].heard = newCondolenceDialogDone[l] == 1;
			}
			newKeyDialogDone = null;
			newObservationDialogDone = null;
			newCondolenceDialogDone = null;
		}
		for (int m = 1; m < KeyDialog.Count; m++)
		{
			if (currentDistance > KeyDialog[m - 1].splinePos && currentDistance < KeyDialog[m].splinePos)
			{
				currentKey = m - 1;
				break;
			}
		}
		if (subtitleTimer > 0f && !paused)
		{
			subtitleTimer -= Time.deltaTime;
			if (subtitleTimer < 0f)
			{
				subtitleTimer = 0f;
			}
		}
		if (showSubtitles)
		{
			if (subtitleTimer < 1f)
			{
				Subtitles.color = new Color(Subtitles.color.r, Subtitles.color.g, Subtitles.color.b, Mathf.Lerp(Subtitles.color.a, 0f, 0.1f));
			}
			else if (subtitleTimer > 0f)
			{
				Subtitles.color = new Color(Subtitles.color.r, Subtitles.color.g, Subtitles.color.b, Mathf.Lerp(Subtitles.color.a, 1f, 0.1f));
			}
		}
		else
		{
			Subtitles.color = new Color(Subtitles.color.r, Subtitles.color.g, Subtitles.color.b, 0f);
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				currentSubtitleBuffer.Clear();
				currentSubtitleTimeBuffer.Clear();
				DJ.Stop();
				VO.Stop();
				subtitleTimer = 0f;
				KeyDialog[10].heard = false;
				SayDialog(KeyDialog[10]);
			}
			for (int n = 0; n < KeyDialog.Count; n++)
			{
				if (Input.GetKeyDown(KeyCode.S))
				{
					speedrun = false;
					if (!KeyDialog[n].heard)
					{
						currentSubtitleBuffer.Clear();
						currentSubtitleTimeBuffer.Clear();
						DJ.Stop();
						VO.Stop();
						subtitleTimer = 0f;
						SayDialog(KeyDialog[n]);
						break;
					}
				}
			}
		}
		for (int num = 0; num < KeyDialog.Count; num++)
		{
			if (currentDistance > KeyDialog[num].splinePos && !KeyDialog[num].heard && !KeyDialog[num].inUse)
			{
				if ((VO.isPlaying || DJ.isPlaying) && lastBit != null && !KeyDialog.Contains(lastBit))
				{
					currentSubtitleBuffer.Clear();
					currentSubtitleTimeBuffer.Clear();
					DJ.Stop();
					VO.Stop();
					subtitleTimer = 0f;
				}
				SayDialog(KeyDialog[num]);
				try
				{
					Analytics.CustomEvent("Checkpoint", new Dictionary<string, object>
					{
						{ "index", num },
						{ "time", timePlayedThisGame }
					});
				}
				catch
				{
					Debug.LogWarning("Analytics failed");
					continue;
				}
				break;
			}
		}
		if (lastBit != null && !lastBit.heard && !VO.isPlaying)
		{
			lastBit.heard = true;
			int num2 = KeyDialog.IndexOf(lastBit);
			if (num2 != -1)
			{
				maxKey = Mathf.Max(maxKey, num2);
			}
		}
		if (dialogueQueue.Count > 0 && subtitleTimer <= 0f && !VO.isPlaying && !DJ.isPlaying)
		{
			if (ScriptLocalization.Get(dialogueQueue[0].textLine) != null)
			{
				currentSubtitleBuffer = new List<string>(ScriptLocalization.Get(dialogueQueue[0].textLine).Split("\n\r"[0]));
				for (int num3 = 0; num3 < currentSubtitleBuffer.Count - 1; num3++)
				{
					float num4 = 100f;
					if (num3 > 0)
					{
						num4 = ((float)currentSubtitleBuffer[num3 - 1].Length + 2f) * (1f / wordsPerSecond) / (float)charactersPerWord[lang];
					}
					float num5 = ((float)currentSubtitleBuffer[num3 + 1].Length + 2f) * (1f / wordsPerSecond) / (float)charactersPerWord[lang];
					float num6 = ((float)currentSubtitleBuffer[num3].Length + 2f) * (1f / wordsPerSecond) / (float)charactersPerWord[lang];
					if (num6 < 2.5f)
					{
						if (num5 < 10f)
						{
							currentSubtitleBuffer[num3 + 1] = currentSubtitleBuffer[num3] + "\n" + currentSubtitleBuffer[num3 + 1];
							currentSubtitleBuffer.RemoveAt(num3);
							num3--;
						}
						else if (num4 < 10f)
						{
							currentSubtitleBuffer[num3 - 1] = currentSubtitleBuffer[num3 - 1] + "\n" + currentSubtitleBuffer[num3];
							currentSubtitleBuffer.RemoveAt(num3);
							num3--;
						}
					}
				}
				currentSubtitleTimeBuffer = new List<float>();
				for (int num7 = 0; num7 < currentSubtitleBuffer.Count; num7++)
				{
					if (dialogueQueue[0].audioLine == null)
					{
						currentSubtitleTimeBuffer.Add(((float)currentSubtitleBuffer[num7].Length + 2f) * (1f / wordsPerSecond) / (float)charactersPerWord[lang]);
					}
					else if (!dialogueQueue[0].isMusic)
					{
						float a = (float)currentSubtitleBuffer[num7].Length / (float)ScriptLocalization.Get(dialogueQueue[0].textLine).Length * dialogueQueue[0].audioLine.length;
						a = Mathf.Max(a, 2f);
						if (num7 == currentSubtitleBuffer.Count - 1)
						{
							a += 2f;
						}
						currentSubtitleTimeBuffer.Add(a);
					}
					else
					{
						currentSubtitleTimeBuffer.Add(8f);
					}
				}
			}
			if (dialogueQueue[0].isMusic)
			{
				if (dialogueQueue[0].audioLine != null)
				{
					DJ.clip = dialogueQueue[0].audioLine;
					DJ.Play();
					if (Piano.volume > 0f)
					{
						targetPianoVolume = 0f;
						pianoVolumeTimer = 1f;
					}
				}
			}
			else if (dialogueQueue[0].audioLine != null)
			{
				VO.clip = dialogueQueue[0].audioLine;
				VO.Play();
				if (Piano.volume < 1f && !dialogueQueue[0].isObservation && dialogueQueue[0] != KeyDialog[KeyDialog.Count - 1])
				{
					targetPianoVolume = 1f;
					pianoVolumeTimer = 1f;
				}
				else if (Piano.volume > 0f && dialogueQueue[0].isObservation)
				{
					targetPianoVolume = 0f;
					pianoVolumeTimer = 1f;
				}
			}
			lastBit = dialogueQueue[0];
			dialogueQueue.RemoveAt(0);
		}
		if (currentSubtitleBuffer.Count > 0 && subtitleTimer <= 0f)
		{
			Subtitles.text = currentSubtitleBuffer[0];
			subtitleTimer = currentSubtitleTimeBuffer[0];
			currentSubtitleBuffer.RemoveAt(0);
			currentSubtitleTimeBuffer.RemoveAt(0);
		}
		if (!DJ.isPlaying && !VO.isPlaying && dialogueQueue.Count == 0 && Piano.volume == 1f)
		{
			targetPianoVolume = 0f;
			pianoVolumeTimer = 1f;
		}
		timePlayedThisGame += Time.deltaTime;
		if (!mouseBit.heard && !paused && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
		{
			mouseClicks++;
			if (mouseClicks % 100 == 0)
			{
				PlayerPrefs.SetInt("Clicks", mouseClicks);
				PlayerPrefs.Save();
			}
			if (mouseClicks >= 500)
			{
				SayDialog(mouseBit);
			}
		}
	}

	public void ToggleSubtitles(bool show)
	{
		showSubtitles = show;
	}

	public void SayDialog(DialogBit bit)
	{
		if (speedrun && bit != antennaBit)
		{
			bit.heard = true;
		}
		else if (!bit.inUse)
		{
			if (!bit.isObservation)
			{
				observationsSinceLastThing = 0;
			}
			else
			{
				observationsSinceLastThing++;
			}
			dialogueQueue.Add(bit);
			bit.inUse = true;
		}
	}

	public void SlowProgress()
	{
		if (Condolences2[Condolences2.Count - 1].heard || KeyDialog[21].heard || averageMouse < 0.05f || dialogueQueue.Count > 0 || VO.isPlaying || DJ.isPlaying)
		{
			return;
		}
		if (!KeyDialog[0].heard)
		{
			SayDialog(KeyDialog[0]);
			maxKey = 0;
			Analytics.CustomEvent("Checkpoint", new Dictionary<string, object>
			{
				{ "index", 0 },
				{ "time", timePlayedThisGame }
			});
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (!Observations2[3 * currentKey + i].heard && !Observations2[3 * currentKey + i].inUse)
			{
				SayDialog(Observations2[i]);
				Observations2[3 * currentKey + i].heard = true;
				return;
			}
		}
		if (!Observations2[3 * currentKey + 2].heard && !Observations2[3 * currentKey + 2].inUse && maxKey > currentKey)
		{
			SayDialog(Observations2[3 * currentKey + 2]);
			Observations2[3 * currentKey + 2].heard = true;
		}
	}

	public void FastRetreat()
	{
		if (dialogueQueue.Count > 0)
		{
			return;
		}
		if (VO.isPlaying || DJ.isPlaying)
		{
			bool flag = false;
			foreach (DialogBit item in Observations2)
			{
				if (!item.inUse)
				{
					continue;
				}
				if (lastBit == item)
				{
					flag = true;
				}
				foreach (DialogBit item2 in dialogueQueue)
				{
					if (item2 == item)
					{
						dialogueQueue.Remove(item2);
					}
				}
			}
			if (!flag)
			{
				return;
			}
		}
		for (int i = 0; i < Condolences2.Count; i++)
		{
			if (!Condolences2[i].heard)
			{
				SayDialog(Condolences2[i]);
				Condolences2[i].heard = true;
				break;
			}
		}
	}

	public void StuckOnAntenna()
	{
		SayDialog(antennaBit);
		antennaBit.heard = true;
	}

	public void UpdateDistance(float d)
	{
		currentDistance = d;
	}
}
