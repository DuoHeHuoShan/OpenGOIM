using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeshCombineStudio
{
	public class Console : MonoBehaviour
	{
		public class LogEntry
		{
			public string logString;

			public string stackTrace;

			public LogType logType;

			public int commandType;

			public bool unityLog;

			public float tStamp;

			public GameObject go;

			public MeshCombiner meshCombiner;

			public LogEntry(string logString, string stackTrace, LogType logType, bool unityLog = false, int commandType = 0, GameObject go = null, MeshCombiner meshCombiner = null)
			{
				this.logString = logString;
				this.stackTrace = stackTrace;
				this.logType = logType;
				this.unityLog = unityLog;
				this.commandType = commandType;
				this.go = go;
				this.meshCombiner = meshCombiner;
			}
		}

		public static Console instance;

		public KeyCode consoleKey = KeyCode.F1;

		public bool logActive = true;

		public bool showConsole;

		public bool showOnError;

		public bool combineAutomatic = true;

		private bool showLast = true;

		private bool setFocus;

		private GameObject selectGO;

		public List<LogEntry> logs = new List<LogEntry>();

		private Rect window;

		private Rect inputRect = default(Rect);

		private Rect logRect = default(Rect);

		private Rect vScrollRect = default(Rect);

		private string inputText;

		private float scrollPos;

		private int lines = 20;

		private bool showUnityLog = true;

		private bool showInputLog = true;

		private MeshCombiner[] meshCombiners;

		private MeshCombiner selectedMeshCombiner;

		private void Awake()
		{
			instance = this;
			FindMeshCombiners();
			window = default(Rect);
			inputText = string.Empty;
			ReportStartup();
		}

		private void ReportStartup()
		{
			Log("---------------------------------");
			Log("*** MeshCombineStudio Console ***");
			Log("---------------------------------");
			Log(string.Empty);
			ReportMeshCombiners(false);
			Log("combine automatic " + ((!combineAutomatic) ? "off" : "on"));
			if (meshCombiners != null && meshCombiners.Length > 0)
			{
				SelectMeshCombiner(meshCombiners[0].name);
			}
			Log(string.Empty);
			Log("Type '?' to show commands");
		}

		private void FindMeshCombiners()
		{
			meshCombiners = UnityEngine.Object.FindObjectsOfType<MeshCombiner>();
		}

		private void ReportMeshCombiners(bool reportSelected = true)
		{
			for (int i = 0; i < meshCombiners.Length; i++)
			{
				ReportMeshCombiner(meshCombiners[i], true);
			}
			if (selectedMeshCombiner != null)
			{
				Log("Selected MCS -> " + selectedMeshCombiner.name);
			}
		}

		private void ReportMeshCombiner(MeshCombiner meshCombiner, bool foundText = false)
		{
			Log(((!foundText) ? string.Empty : "Found MCS -> ") + meshCombiner.name + " (" + ((!meshCombiner.combined) ? "*color-blue#Uncombined" : "*color-green#Combined") + ") -> Cell Size " + meshCombiner.cellSize + ((!meshCombiner.searchOptions.useMaxBoundsFactor) ? string.Empty : (" | Max Bounds Factor " + meshCombiner.searchOptions.maxBoundsFactor)) + ((!meshCombiner.searchOptions.useVertexInputLimit) ? string.Empty : (" | Vertex Input Limit " + ((!meshCombiner.searchOptions.useVertexInputLimit) ? 65534 : meshCombiner.searchOptions.vertexInputLimit))) + ((!meshCombiner.searchOptions.useVertexInputLimitLod) ? string.Empty : (" | Vertex Input Limit Lod " + ((!meshCombiner.searchOptions.useVertexInputLimitLod) ? 65534 : meshCombiner.searchOptions.vertexInputLimitLod))), 0, null, meshCombiner);
		}

		public int SelectMeshCombiner(string name)
		{
			if (meshCombiners == null && meshCombiners.Length == 0)
			{
				return 0;
			}
			for (int i = 0; i < meshCombiners.Length; i++)
			{
				MeshCombiner meshCombiner = meshCombiners[i];
				if (meshCombiner.name == name)
				{
					Log("Selected MCS -> " + meshCombiner.name + " (" + ((!meshCombiner.combined) ? "*color-blue#Uncombined" : "*color-green#Combined") + ")", 0, null, meshCombiner);
					selectedMeshCombiner = meshCombiner;
					return 2;
				}
			}
			return 0;
		}

		private void OnEnable()
		{
			Application.logMessageReceived += HandleLog;
		}

		private void OnDisable()
		{
			Application.logMessageReceived -= HandleLog;
		}

		private void OnDestroy()
		{
			instance = null;
		}

		public static void Log(string logString, int commandType = 0, GameObject go = null, MeshCombiner meshCombiner = null)
		{
			instance.logs.Add(new LogEntry(logString, string.Empty, LogType.Log, false, commandType, go, meshCombiner));
		}

		private void HandleLog(string logString, string stackTrace, LogType logType)
		{
			if (logActive)
			{
				logs.Add(new LogEntry(logString, stackTrace, logType, true));
				if (showOnError && (logType == LogType.Error || logType == LogType.Warning))
				{
					SetConsoleActive(true);
					showLast = true;
					showUnityLog = true;
				}
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(consoleKey))
			{
				SetConsoleActive(!showConsole);
			}
		}

		private void SetConsoleActive(bool active)
		{
			showConsole = active;
			if (showConsole)
			{
				setFocus = true;
			}
		}

		private void ExecuteCommand(string cmd)
		{
			logs.Add(new LogEntry(cmd, string.Empty, LogType.Log, false, 1));
			LogEntry logEntry = logs[logs.Count - 1];
			switch (cmd)
			{
			case "?":
				Log("'F1' to show/hide console");
				Log("'dir', 'dirAll', 'dirSort', 'cd', 'show', 'showAll', 'hide', 'hideAll'");
				Log("'components', 'lines', 'clear', 'gc collect'");
				Log("'select (MeshCombineStudio name)', ");
				Log("'report MeshCombineStudio'");
				Log("'combine', 'uncombine', 'combine automatic on/off'");
				Log("'max bounds factor (float)/off'");
				Log("'vertex input limit (float)/off'");
				Log("'vertex input limit lod (float)/off'");
				Log("'cell size (float)'");
				logEntry.commandType = 2;
				return;
			case "gc collect":
				GC.Collect();
				logEntry.commandType = 2;
				return;
			case "dir":
				Dir();
				logEntry.commandType = 2;
				return;
			case "components":
				Components(logEntry);
				return;
			}
			if (cmd.Contains("lines "))
			{
				int.TryParse(cmd.Replace("lines ", string.Empty), out lines);
				lines = Mathf.Clamp(lines, 5, 50);
				logEntry.commandType = 2;
				return;
			}
			if (cmd == "cd..")
			{
				CD(logEntry, "..");
				return;
			}
			if (cmd == "cd\\")
			{
				CD(logEntry, "\\");
				return;
			}
			if (cmd.Contains("cd "))
			{
				CD(logEntry, cmd.Replace("cd ", string.Empty));
				return;
			}
			if (cmd.Contains("show "))
			{
				GameObject gameObject = Methods.Find<GameObject>(selectGO, cmd.Replace("show ", string.Empty));
				if (gameObject != null)
				{
					gameObject.SetActive(true);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd == "show")
			{
				if (selectGO != null)
				{
					selectGO.SetActive(true);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd.Contains("showAll "))
			{
				SetActiveContains(cmd.Replace("showAll ", string.Empty), true);
				logEntry.commandType = 2;
				return;
			}
			if (cmd.Contains("hide "))
			{
				GameObject gameObject2 = GameObject.Find(cmd.Replace("hide ", string.Empty));
				if (gameObject2 != null)
				{
					gameObject2.SetActive(false);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd.Contains("hideAll "))
			{
				SetActiveContains(cmd.Replace("hideAll ", string.Empty), false);
				logEntry.commandType = 2;
				return;
			}
			if (cmd == "hide")
			{
				if (selectGO != null)
				{
					selectGO.SetActive(false);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd.Contains("clear"))
			{
				Clear(logEntry, cmd.Replace("clear ", string.Empty));
				return;
			}
			if (cmd.Contains("dir "))
			{
				DirContains(cmd.Replace("dir ", string.Empty));
				logEntry.commandType = 2;
				return;
			}
			if (cmd == "dirAll")
			{
				DirAll();
				logEntry.commandType = 2;
				return;
			}
			if (cmd.Contains("dirSort "))
			{
				DirSort(cmd.Replace("dirSort ", string.Empty));
				logEntry.commandType = 2;
				return;
			}
			if (cmd == "dirSort")
			{
				DirSort();
				logEntry.commandType = 2;
				return;
			}
			if (cmd.Contains("cell size "))
			{
				int result;
				int.TryParse(cmd.Replace("cell size ", string.Empty), out result);
				if (result < 4)
				{
					Log("cell size should be bigger than 4");
				}
				else if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.cellSize = result;
					selectedMeshCombiner.AddObjectsAutomatically();
					if (combineAutomatic)
					{
						selectedMeshCombiner.CombineAll();
					}
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			switch (cmd)
			{
			case "report MeshCombineStudio":
				ReportMeshCombiners();
				logEntry.commandType = 2;
				return;
			case "combine":
				if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.octreeContainsObjects = false;
					selectedMeshCombiner.CombineAll();
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			case "uncombine":
				if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.DestroyCombinedObjects();
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			case "combine automatic off":
				combineAutomatic = false;
				logEntry.commandType = 2;
				return;
			case "combine automatic on":
				combineAutomatic = true;
				logEntry.commandType = 2;
				return;
			}
			if (cmd.Contains("select "))
			{
				if (SelectMeshCombiner(cmd.Replace("select ", string.Empty)) == 2)
				{
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd == "max bounds factor off")
			{
				if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.searchOptions.useMaxBoundsFactor = false;
					selectedMeshCombiner.AddObjectsAutomatically();
					if (combineAutomatic)
					{
						selectedMeshCombiner.CombineAll();
					}
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd.Contains("max bounds factor "))
			{
				float result2;
				float.TryParse(cmd.Replace("max bounds factor ", string.Empty), out result2);
				if (result2 < 1f)
				{
					Log("max bounds factor needs to be bigger than 1");
				}
				else if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.searchOptions.useMaxBoundsFactor = true;
					selectedMeshCombiner.searchOptions.maxBoundsFactor = result2;
					selectedMeshCombiner.AddObjectsAutomatically();
					if (combineAutomatic)
					{
						selectedMeshCombiner.CombineAll();
					}
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd == "vertex input limit off")
			{
				if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.searchOptions.useVertexInputLimit = false;
					selectedMeshCombiner.AddObjectsAutomatically();
					if (combineAutomatic)
					{
						selectedMeshCombiner.CombineAll();
					}
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd == "vertex input limit lod off")
			{
				if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.searchOptions.useVertexInputLimitLod = false;
					selectedMeshCombiner.AddObjectsAutomatically();
					if (combineAutomatic)
					{
						selectedMeshCombiner.CombineAll();
					}
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			if (cmd.Contains("vertex input limit "))
			{
				int result3;
				int.TryParse(cmd.Replace("vertex input limit ", string.Empty), out result3);
				if (result3 < 1)
				{
					Log("vertex input limit needs to be bigger than 1");
				}
				else if (selectedMeshCombiner != null)
				{
					selectedMeshCombiner.searchOptions.useVertexInputLimit = true;
					selectedMeshCombiner.searchOptions.vertexInputLimit = result3;
					selectedMeshCombiner.AddObjectsAutomatically();
					if (combineAutomatic)
					{
						selectedMeshCombiner.CombineAll();
					}
					ReportMeshCombiner(selectedMeshCombiner);
					logEntry.commandType = 2;
				}
				return;
			}
			if (!cmd.Contains("vertex input limit lod "))
			{
				return;
			}
			int result4;
			int.TryParse(cmd.Replace("vertex input limit lod ", string.Empty), out result4);
			if (result4 < 1)
			{
				Log("vertex input limit needs to be bigger than 1");
			}
			else if (selectedMeshCombiner != null)
			{
				selectedMeshCombiner.searchOptions.useVertexInputLimitLod = true;
				selectedMeshCombiner.searchOptions.vertexInputLimitLod = result4;
				selectedMeshCombiner.AddObjectsAutomatically();
				if (combineAutomatic)
				{
					selectedMeshCombiner.CombineAll();
				}
				ReportMeshCombiner(selectedMeshCombiner);
				logEntry.commandType = 2;
			}
		}

		private void DirSort()
		{
			GameObject[] gos = Methods.Search<GameObject>(selectGO);
			SortLog(gos, true);
		}

		private void DirSort(string name)
		{
			GameObject[] array = Methods.Search<GameObject>();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				if (Methods.Contains(array[i].name, name))
				{
					list.Add(array[i]);
				}
			}
			SortLog(list.ToArray());
		}

		public void SortLog(GameObject[] gos, bool showMeshInfo = false)
		{
			List<GameObject> list = new List<GameObject>();
			List<int> list2 = new List<int>();
			int num = 0;
			int meshCount = 0;
			foreach (GameObject gameObject in gos)
			{
				GetMeshInfo(gameObject, ref meshCount);
				string text = gameObject.name;
				int num2 = -1;
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].name == text)
					{
						num2 = j;
						break;
					}
				}
				if (num2 == -1)
				{
					list.Add(gameObject);
					list2.Add(1);
					num++;
				}
				else
				{
					list2[num2]++;
					num++;
				}
			}
			int meshCount2 = 0;
			for (int k = 0; k < list.Count; k++)
			{
				string logString = list[k].name + " -> " + list2[k] + " " + GetMeshInfo(list[k], ref meshCount2);
				Log(logString);
			}
			Log("Total amount " + num + " Total items " + list.Count + " Total shared meshes " + meshCount);
		}

		private string GetMeshInfo(GameObject go, ref int meshCount)
		{
			MeshFilter component = go.GetComponent<MeshFilter>();
			if (component != null)
			{
				Mesh sharedMesh = component.sharedMesh;
				if (sharedMesh != null)
				{
					meshCount++;
					return "(vertices " + sharedMesh.vertexCount + ", combine " + Mathf.FloorToInt(65000 / sharedMesh.vertexCount) + ")";
				}
			}
			return string.Empty;
		}

		private void TimeStep(string cmd)
		{
			float result;
			float.TryParse(cmd, out result);
			Time.fixedDeltaTime = result;
		}

		private void TimeScale(string cmd)
		{
			float result;
			float.TryParse(cmd, out result);
			Time.timeScale = result;
		}

		private void Clear(LogEntry log, string cmd)
		{
			switch (cmd)
			{
			case "clear":
				logs.Clear();
				log.commandType = 2;
				break;
			case "input":
			{
				for (int j = 0; j < logs.Count; j++)
				{
					if (!logs[j].unityLog)
					{
						logs.RemoveAt(j--);
					}
				}
				log.commandType = 2;
				break;
			}
			case "unity":
			{
				for (int i = 0; i < logs.Count; i++)
				{
					if (logs[i].unityLog)
					{
						logs.RemoveAt(i--);
					}
				}
				log.commandType = 2;
				break;
			}
			}
		}

		private void DirAll()
		{
			GameObject[] array = Methods.Search<GameObject>(selectGO);
			int meshCount = 0;
			for (int i = 0; i < array.Length; i++)
			{
				Log(GetPath(array[i]) + "\\" + array[i].transform.childCount + " " + GetMeshInfo(array[i], ref meshCount), 0, array[i]);
			}
			Log(array.Length + " (meshes " + meshCount + ")\\..");
		}

		private void Dir()
		{
			int meshCount = 0;
			if (selectGO == null)
			{
				GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
				for (int i = 0; i < rootGameObjects.Length; i++)
				{
					Log(rootGameObjects[i].name + "\\" + rootGameObjects[i].transform.childCount + " " + GetMeshInfo(rootGameObjects[i], ref meshCount), 0, rootGameObjects[i]);
				}
				Log(rootGameObjects.Length + " (meshes " + meshCount + ")\\..");
				return;
			}
			ShowPath();
			Transform transform = selectGO.transform;
			for (int j = 0; j < transform.childCount; j++)
			{
				Transform child = transform.GetChild(j);
				Log(child.name + "\\" + child.childCount + " " + GetMeshInfo(child.gameObject, ref meshCount), 0, child.gameObject);
			}
			Log(transform.childCount + " (meshes " + meshCount + ")\\..");
		}

		private void Components(LogEntry log)
		{
			if (selectGO == null)
			{
				log.commandType = 1;
				return;
			}
			Component[] components = selectGO.GetComponents<Component>();
			ShowPath();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] != null)
				{
					Log(components[i].GetType().Name);
				}
			}
			log.commandType = 2;
		}

		private void ShowPath(bool showLines = true)
		{
			string path = GetPath(selectGO);
			if (path != string.Empty)
			{
				Log(path);
			}
			else
			{
				Log("Root\\");
			}
			if (showLines)
			{
				Log("---------------------------------");
			}
		}

		private string GetPath(GameObject go)
		{
			if (go != null)
			{
				string text = go.name;
				Transform parent = go.transform;
				while (parent.parent != null)
				{
					text = text.Insert(0, parent.parent.name + "\\");
					parent = parent.parent;
				}
				return text;
			}
			return string.Empty;
		}

		private void CD(LogEntry log, string name)
		{
			if (name == "..")
			{
				if (selectGO != null)
				{
					if (selectGO.transform.parent != null)
					{
						selectGO = selectGO.transform.parent.gameObject;
					}
					else
					{
						selectGO = null;
					}
					log.commandType = 2;
					ShowPath(false);
					return;
				}
			}
			else if (name == "\\")
			{
				selectGO = null;
				log.commandType = 2;
				return;
			}
			GameObject gameObject = Methods.Find<GameObject>(selectGO, name);
			if (gameObject != null)
			{
				selectGO = gameObject;
				ShowPath(false);
				log.commandType = 2;
			}
		}

		public void SetActiveContains(string textContains, bool active)
		{
			GameObject[] array = Methods.Search<GameObject>(selectGO);
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (Methods.Contains(array[i].name, textContains) && (array[i].transform.parent.name.IndexOf("GUI") == 0 || array[i].transform.parent.parent == null || array[i].transform.parent.parent.name.IndexOf("GUI") == 0))
				{
					array[i].SetActive(active);
					num++;
				}
			}
			Log("Total amount set to " + active + " : " + num);
		}

		public void DirContains(string textContains)
		{
			GameObject[] array = Methods.Search<GameObject>(selectGO);
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (Methods.Contains(array[i].name, textContains))
				{
					Log(array[i].name, 0, array[i]);
					num++;
				}
			}
			Log("Total amount: " + num);
		}

		private void OnGUI()
		{
			if (!showConsole)
			{
				return;
			}
			window.x = 225f;
			window.y = 5f;
			window.yMax = lines * 20 + 30;
			window.xMax = (float)Screen.width - window.x;
			GUI.Box(window, "Console");
			inputRect.x = window.x + 5f;
			inputRect.y = window.yMax - 25f;
			inputRect.xMax = window.xMax - 10f;
			inputRect.yMax = window.yMax - 5f;
			if (showInputLog)
			{
				if (GUI.GetNameOfFocusedControl() == "ConsoleInput" && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
				{
					showLast = true;
					ExecuteCommand(inputText);
					inputText = string.Empty;
				}
				GUI.SetNextControlName("ConsoleInput");
				GUI.changed = false;
				inputText = GUI.TextField(inputRect, inputText);
				if (GUI.changed && inputText.Contains("`"))
				{
					inputText = inputText.Replace("`", string.Empty);
					SetConsoleActive(!showConsole);
				}
				if (setFocus)
				{
					setFocus = false;
					GUI.FocusControl("ConsoleInput");
				}
			}
			if (showInputLog)
			{
				GUI.color = Color.green;
			}
			else
			{
				GUI.color = Color.grey;
			}
			if (GUI.Button(new Rect(window.xMin + 5f, window.yMin + 5f, 75f, 20f), "Input Log"))
			{
				showInputLog = !showInputLog;
			}
			if (showUnityLog)
			{
				GUI.color = Color.green;
			}
			else
			{
				GUI.color = Color.grey;
			}
			if (GUI.Button(new Rect(window.xMin + 85f, window.yMin + 5f, 75f, 20f), "Unity Log"))
			{
				showUnityLog = !showUnityLog;
			}
			GUI.color = Color.white;
			if (!showInputLog && !showUnityLog)
			{
				showInputLog = true;
			}
			logRect.x = window.x + 5f;
			logRect.y = window.y + 25f;
			logRect.xMax = window.xMax - 20f;
			logRect.yMax = logRect.y + 20f;
			vScrollRect.x = window.xMax - 15f;
			vScrollRect.y = logRect.y;
			vScrollRect.xMax = window.xMax - 5f;
			vScrollRect.yMax = window.yMax - 45f;
			float num = Mathf.Ceil(vScrollRect.height / 20f);
			if (showLast && Event.current.type != EventType.Repaint)
			{
				scrollPos = logs.Count;
			}
			GUI.changed = false;
			scrollPos = GUI.VerticalScrollbar(vScrollRect, scrollPos, (!(num > (float)(logs.Count - 1))) ? (num - 1f) : ((float)(logs.Count - 1)), 0f, logs.Count - 1);
			if (GUI.changed)
			{
				showLast = false;
			}
			int num2 = (int)scrollPos;
			if (num2 < 0)
			{
				num2 = 0;
			}
			int num3 = num2 + (int)num;
			if (num3 > logs.Count)
			{
				num3 = logs.Count;
			}
			int num4 = num3 - num2;
			int num5 = num2;
			int num6 = 0;
			while (num6 != num4 && num5 < logs.Count)
			{
				LogEntry logEntry = logs[num5];
				if ((logEntry.unityLog && showUnityLog) || (!logEntry.unityLog && showInputLog))
				{
					if (logEntry.logType == LogType.Warning)
					{
						AnimateColor(Color.yellow, logEntry, 0.75f);
					}
					else if (logEntry.logType == LogType.Error)
					{
						AnimateColor(Color.red, logEntry, 0.75f);
					}
					else if (logEntry.logType == LogType.Exception)
					{
						AnimateColor(Color.magenta, logEntry, 0.75f);
					}
					else if (logEntry.unityLog)
					{
						AnimateColor(Color.white, logEntry, 0.75f);
					}
					else if (logEntry.commandType == 1)
					{
						GUI.color = new Color(0f, 0.5f, 0f);
					}
					else if (logEntry.commandType == 2)
					{
						GUI.color = Color.green;
					}
					else if (logEntry.go != null)
					{
						GUI.color = ((!logEntry.go.activeSelf) ? (Color.white * 0.7f) : Color.white);
					}
					string text = logEntry.logString;
					if (text.Contains("*color-"))
					{
						if (text.Contains("*color-green#"))
						{
							text = text.Replace("*color-green#", string.Empty);
							GUI.color = Color.green;
						}
						else if (text.Contains("*color-blue#"))
						{
							text = text.Replace("*color-blue#", string.Empty);
							GUI.color = Color.blue;
						}
					}
					GUI.Label(logRect, num5 + ") ");
					logRect.xMin += 55f;
					GUI.Label(logRect, text + ((!(logEntry.stackTrace != string.Empty)) ? string.Empty : (" (" + logEntry.stackTrace + ")")));
					logRect.xMin -= 55f;
					GUI.color = Color.white;
					logRect.y += 20f;
					num6++;
				}
				num5++;
			}
		}

		private void AnimateColor(Color col, LogEntry log, float multi)
		{
			GUI.color = Color.Lerp(col * multi, col, Mathf.Abs(Mathf.Sin(Time.time)));
		}
	}
}
