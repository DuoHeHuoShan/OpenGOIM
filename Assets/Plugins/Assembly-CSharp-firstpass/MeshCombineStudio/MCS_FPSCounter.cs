using UnityEngine;

namespace MeshCombineStudio
{
	public class MCS_FPSCounter : MonoBehaviour
	{
		public enum GUIType
		{
			DisplayRunning = 0,
			DisplayResults = 1,
			DisplayNothing = 2
		}

		public static MCS_FPSCounter instance;

		[Header("___ Settings ___________________________________________________________________________________________________________")]
		public float interval = 0.25f;

		public GUIType displayType;

		public Vector2 gradientRange = new Vector2(15f, 60f);

		public Font fontRun;

		public Font fontResult;

		public Texture logo;

		public bool showLogoOnResultsScreen = true;

		public KeyCode showHideButton = KeyCode.Backspace;

		public bool acceptInput = true;

		public bool reset;

		[Header("___ Results ___________________________________________________________________________________________________________")]
		public float currentFPS;

		public float averageFPS;

		public float minimumFPS;

		public float maximumFPS;

		private int totalFrameCount;

		private int tempFrameCount;

		private double tStamp;

		private double tStampTemp;

		private string currentFPSText;

		private string avgFPSText;

		private string minFPSText;

		private string maxFSPText;

		private GUIStyle bigStyle = new GUIStyle();

		private GUIStyle bigStyleShadow;

		private GUIStyle smallStyle = new GUIStyle();

		private GUIStyle smallStyleShadow;

		private GUIStyle smallStyleLabel;

		private GUIStyle headerStyle = new GUIStyle();

		private Rect[] rectsRun = new Rect[14]
		{
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect)
		};

		private Rect[] rectsResult = new Rect[10]
		{
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect),
			default(Rect)
		};

		private Gradient gradient = new Gradient();

		private const float line1 = 4f;

		private const float line2 = 30f;

		private const float line3 = 44f;

		private const float line4 = 58f;

		private const float labelWidth = 26f;

		private const float paddingH = 8f;

		private const float lineHeight = 22f;

		private float columnRight;

		private float columnLeft;

		private Color fontShadow = new Color(0f, 0f, 0f, 0.5f);

		private Color label = new Color(0.8f, 0.8f, 0.8f, 1f);

		private Color colorCurrent;

		private Color colorAvg;

		private const string resultHeader = "BENCHMARK RESULTS";

		private const string resultLabelAvg = "AVERAGE FPS:";

		private const string resultLabelMin = "MINIMUM FPS:";

		private const string resultLabelMax = "MAXIMUM FPS:";

		private GUIContent resultHeaderGUI = new GUIContent("BENCHMARK RESULTS");

		private GUIContent reslutLabelAvgGUI = new GUIContent("AVERAGE FPS:");

		private GUIContent avgTextGUI = new GUIContent();

		private GUIContent instructions = new GUIContent("PRESS SPACEBAR TO RERUN BENCHMARK | PRESS ESCAPE TO RETURN TO MENU");

		private const string runLabelAvg = "Avg:";

		private const string runLabelMin = "Min:";

		private const string runLabelMax = "Max:";

		private Vector2 screenSize = new Vector2(0f, 0f);

		private GUIType oldDisplayType = GUIType.DisplayNothing;

		private void Awake()
		{
			instance = this;
			gradient.colorKeys = new GradientColorKey[3]
			{
				new GradientColorKey(new Color(1f, 0f, 0f, 1f), 0f),
				new GradientColorKey(new Color(1f, 1f, 0f, 1f), 0.5f),
				new GradientColorKey(new Color(0f, 1f, 0f, 1f), 1f)
			};
		}

		private void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}

		private void OnGUI()
		{
			if (displayType == GUIType.DisplayNothing)
			{
				return;
			}
			if (displayType == GUIType.DisplayRunning)
			{
				if ((float)Screen.width != screenSize.x || (float)Screen.height != screenSize.y)
				{
					screenSize.x = Screen.width;
					screenSize.y = Screen.height;
					SetRectsRun();
				}
				GUI.Label(rectsRun[0], currentFPSText, bigStyleShadow);
				GUI.Label(rectsRun[1], avgFPSText, smallStyleShadow);
				GUI.Label(rectsRun[2], minFPSText, smallStyleShadow);
				GUI.Label(rectsRun[3], maxFSPText, smallStyleShadow);
				GUI.Label(rectsRun[4], "Avg:", smallStyleShadow);
				GUI.Label(rectsRun[5], "Min:", smallStyleShadow);
				GUI.Label(rectsRun[6], "Max:", smallStyleShadow);
				GUI.Label(rectsRun[7], currentFPSText, bigStyle);
				GUI.Label(rectsRun[8], avgFPSText, smallStyle);
				GUI.Label(rectsRun[9], minFPSText, smallStyle);
				GUI.Label(rectsRun[10], maxFSPText, smallStyle);
				GUI.Label(rectsRun[11], "Avg:", smallStyleLabel);
				GUI.Label(rectsRun[12], "Min:", smallStyleLabel);
				GUI.Label(rectsRun[13], "Max:", smallStyleLabel);
			}
			else
			{
				if ((float)Screen.width != screenSize.x || (float)Screen.height != screenSize.y)
				{
					screenSize.x = Screen.width;
					screenSize.y = Screen.height;
					SetRectsResult();
				}
				if (showLogoOnResultsScreen)
				{
					GUI.DrawTexture(rectsResult[8], logo);
				}
				GUI.Label(rectsResult[0], resultHeaderGUI, headerStyle);
				GUI.DrawTexture(rectsResult[1], Texture2D.whiteTexture);
				GUI.Label(rectsResult[2], reslutLabelAvgGUI, smallStyle);
				GUI.Label(rectsResult[4], "MINIMUM FPS:", smallStyleLabel);
				GUI.Label(rectsResult[6], "MAXIMUM FPS:", smallStyleLabel);
				GUI.Label(rectsResult[3], avgTextGUI, bigStyle);
				GUI.Label(rectsResult[5], minFPSText, smallStyle);
				GUI.Label(rectsResult[7], maxFSPText, smallStyle);
				GUI.Label(rectsResult[9], instructions, smallStyleLabel);
			}
		}

		private void SetRectsRun()
		{
			columnRight = (float)Screen.width - 34f;
			columnLeft = columnRight - 34f;
			float num = 0f;
			rectsRun[0].Set((float)Screen.width - 48f + 1f, num + 4f + 2f, 40f, 22f);
			rectsRun[1].Set(columnRight + 1f, num + 30f + 2f, 26f, 22f);
			rectsRun[2].Set(columnRight + 1f, num + 44f + 2f, 26f, 22f);
			rectsRun[3].Set(columnRight + 1f, num + 58f + 2f, 26f, 22f);
			rectsRun[4].Set(columnLeft + 1f, num + 30f + 2f, 26f, 22f);
			rectsRun[5].Set(columnLeft + 1f, num + 44f + 2f, 26f, 22f);
			rectsRun[6].Set(columnLeft + 1f, num + 58f + 2f, 26f, 22f);
			rectsRun[7].Set((float)Screen.width - 53f, num + 4f, 45f, 22f);
			rectsRun[8].Set(columnRight, num + 30f, 26f, 22f);
			rectsRun[9].Set(columnRight, num + 44f, 26f, 22f);
			rectsRun[10].Set(columnRight, num + 58f, 26f, 22f);
			rectsRun[11].Set(columnLeft, num + 30f, 26f, 22f);
			rectsRun[12].Set(columnLeft, num + 44f, 26f, 22f);
			rectsRun[13].Set(columnLeft, num + 58f, 26f, 22f);
		}

		private void SetRectsResult()
		{
			float num = 256f;
			rectsResult[8].Set(Screen.width / 2 - logo.width / 2, (float)(Screen.height / 2) - num, logo.width, logo.height);
			Vector2 vector = headerStyle.CalcSize(resultHeaderGUI);
			rectsResult[0].Set((float)(Screen.width / 2) - vector.x / 2f, (float)(Screen.height / 2) - (num - 256f), vector.x, vector.y);
			vector.x += 10f;
			rectsResult[1].Set((float)(Screen.width / 2) - vector.x / 2f, (float)(Screen.height / 2) - (num - 256f - 30f), vector.x, 1f);
			rectsResult[2].Set(Screen.width / 2 - 200, (float)(Screen.height / 2) - (num - 256f - 30f - 30f), 200f, 24f);
			rectsResult[4].Set(Screen.width / 2 - 200, (float)(Screen.height / 2) - (num - 256f - 30f - 30f - 20f), 200f, 24f);
			rectsResult[6].Set(Screen.width / 2 - 200, (float)(Screen.height / 2) - (num - 256f - 30f - 30f - 20f - 20f), 200f, 24f);
			rectsResult[3].Set(Screen.width / 2, (float)(Screen.height / 2) - (num - 256f - 30f - 18f), 65f, 24f);
			rectsResult[5].Set(Screen.width / 2, (float)(Screen.height / 2) - (num - 256f - 30f - 30f - 20f), 65f, 24f);
			rectsResult[7].Set(Screen.width / 2, (float)(Screen.height / 2) - (num - 256f - 30f - 30f - 20f - 20f), 65f, 24f);
			vector = smallStyleLabel.CalcSize(instructions);
			rectsResult[9].Set((float)(Screen.width / 2) - vector.x / 2f, (float)(Screen.height / 2) - (num - 256f - 30f - 30f - 20f - 20f - 40f), vector.x, vector.y);
		}

		private void Start()
		{
			headerStyle.normal.textColor = label;
			headerStyle.fontSize = 24;
			headerStyle.font = fontResult;
			headerStyle.alignment = TextAnchor.UpperCenter;
			bigStyle.alignment = TextAnchor.UpperRight;
			bigStyle.font = fontRun;
			bigStyle.fontSize = 24;
			bigStyle.normal.textColor = Color.green;
			bigStyleShadow = new GUIStyle(bigStyle);
			bigStyleShadow.normal.textColor = fontShadow;
			smallStyle.alignment = TextAnchor.UpperRight;
			smallStyle.font = fontRun;
			smallStyle.fontSize = 12;
			smallStyle.normal.textColor = Color.white;
			smallStyleShadow = new GUIStyle(smallStyle);
			smallStyleShadow.normal.textColor = fontShadow;
			smallStyleLabel = new GUIStyle(smallStyle);
			smallStyleLabel.normal.textColor = label;
			Invoke("Reset", 0.5f);
		}

		private void Update()
		{
			if (displayType != oldDisplayType)
			{
				if (displayType == GUIType.DisplayResults)
				{
					SetRectsResult();
					colorAvg = EvaluateGradient(averageFPS);
					bigStyle.normal.textColor = colorAvg;
					avgTextGUI.text = avgFPSText;
				}
				else if (displayType == GUIType.DisplayRunning)
				{
					Reset();
					SetRectsRun();
				}
				oldDisplayType = displayType;
			}
			if (Input.GetKeyDown(showHideButton) && acceptInput && displayType != GUIType.DisplayResults)
			{
				if (displayType == GUIType.DisplayNothing)
				{
					displayType = GUIType.DisplayRunning;
				}
				else
				{
					displayType = GUIType.DisplayNothing;
				}
			}
			if (displayType != GUIType.DisplayNothing)
			{
				if (displayType == GUIType.DisplayRunning)
				{
					GetFPS();
				}
				if (reset)
				{
					reset = false;
					Reset();
				}
			}
		}

		public void StartBenchmark()
		{
			Reset();
			SetRectsRun();
			displayType = GUIType.DisplayRunning;
		}

		public void StopBenchmark()
		{
			SetRectsResult();
			displayType = GUIType.DisplayResults;
			colorAvg = EvaluateGradient(averageFPS);
			bigStyle.normal.textColor = colorAvg;
		}

		private void GetFPS()
		{
			tempFrameCount++;
			totalFrameCount++;
			if ((double)Time.realtimeSinceStartup - tStampTemp > (double)interval)
			{
				currentFPS = (float)((double)tempFrameCount / ((double)Time.realtimeSinceStartup - tStampTemp));
				averageFPS = (float)((double)totalFrameCount / ((double)Time.realtimeSinceStartup - tStamp));
				if (currentFPS < minimumFPS)
				{
					minimumFPS = currentFPS;
				}
				if (currentFPS > maximumFPS)
				{
					maximumFPS = currentFPS;
				}
				tStampTemp = Time.realtimeSinceStartup;
				tempFrameCount = 0;
				currentFPSText = "FPS " + currentFPS.ToString("0.0");
				avgFPSText = averageFPS.ToString("0.0");
				minFPSText = minimumFPS.ToString("0.0");
				maxFSPText = maximumFPS.ToString("0.0");
				colorCurrent = EvaluateGradient(currentFPS);
				bigStyle.normal.textColor = colorCurrent;
			}
		}

		public void Reset()
		{
			tStamp = Time.realtimeSinceStartup;
			tStampTemp = Time.realtimeSinceStartup;
			currentFPS = 0f;
			averageFPS = 0f;
			minimumFPS = 999.9f;
			maximumFPS = 0f;
			tempFrameCount = 0;
			totalFrameCount = 0;
		}

		private Color EvaluateGradient(float f)
		{
			return gradient.Evaluate(Mathf.Clamp01((f - gradientRange.x) / (gradientRange.y - gradientRange.x)));
		}
	}
}
