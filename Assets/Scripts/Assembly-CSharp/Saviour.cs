// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Saviour
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DigitalRuby.Threading;
using UnityEngine;

public class Saviour : MonoBehaviour
{
	public Narrator narrator;

	public Rigidbody2D hammer;

	public SliderJoint2D slider;

	public HingeJoint2D hinge;

	public Rigidbody2D cursor;

	public HingeJoint2D hubJoint;

	public Transform playerTransform;

	private JointMotor2D stillHingeMotor;

	private JointMotor2D stillSliderMotor;

	private JointMotor2D stillHubMotor;

	public string ApplicationVersion;

	private SaveState[] debugSaves;

	private int currentSave;

	public PlayerControl pc;

	public ScreenFader sf;

	public GravityControl gc;

	private StringBuilder stream;

	private StringWriter streamWriter;

	private TextWriter textWriter;

	private TextReader textReader;

	private XmlSerializer serializer;

	private int frame;

	private int anxiousFrame;

	private int saveNum;

	private bool willPlayAnimation;

	private int savesSinceWrite;

	private int savesPerWrite = 20;

	private bool readyToGo;

	private bool exiting;

	private Rigidbody2D[] rbs;

	private Vector2[] rbLinearVelocities = new Vector2[6];

	private float[] rbAngularVelocities = new float[6];

	private Vector2[] rbPositions = new Vector2[6];

	private float[] rbAngles = new float[6];

	private string saveString;

	private string saveString0 = "SaveGame0";

	private string saveString1 = "SaveGame1";

	private byte[] buffer = new byte[0];

	private string encodedstring;

	private bool dispatched;

	private string utfString;

	private bool anxiousToSave;

	private SaveState newSave;

	public Transform spine1;

	public Transform spine2;

	private void Start()
	{
		savesPerWrite = 100;
		if (sf == null)
		{
			sf = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
		if (gc == null)
		{
			gc = GameObject.FindGameObjectWithTag("GravityControl").GetComponent<GravityControl>();
		}
		if (pc == null)
		{
			pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		}
		Rigidbody2D[] attachedRigidBodies = pc.AttachedRigidBodies;
		rbLinearVelocities = new Vector2[attachedRigidBodies.Length];
		rbAngularVelocities = new float[attachedRigidBodies.Length];
		rbPositions = new Vector2[attachedRigidBodies.Length];
		rbAngles = new float[attachedRigidBodies.Length];
		stillHingeMotor = hinge.motor;
		stillHingeMotor.motorSpeed = 0f;
		stillSliderMotor = hinge.motor;
		stillSliderMotor.motorSpeed = 0f;
		stillHubMotor = hubJoint.motor;
		stillHubMotor.motorSpeed = 0f;
		stillHubMotor.maxMotorTorque = 100f;
		cursor.position = hammer.position;
		slider.motor = stillSliderMotor;
		hinge.motor = stillHingeMotor;
		hubJoint.motor = stillHubMotor;
		debugSaves = new SaveState[2];
		debugSaves[0] = new SaveState();
		debugSaves[1] = new SaveState();
		currentSave = 0;
		stream = new StringBuilder();
		streamWriter = new StringWriter(stream);
		serializer = new XmlSerializer(typeof(SaveState));
		frame = 0;
		exiting = false;
		if (PlayerPrefs.HasKey("NumSaves"))
		{
			if (PlayerPrefs.GetInt("NumSaves") != 0)
			{
				saveNum = PlayerPrefs.GetInt("NumSaves");
			}
			else
			{
				saveNum = 0;
				PlayerPrefs.SetInt("NumSaves", 0);
			}
		}
		else
		{
			saveNum = 0;
			PlayerPrefs.SetInt("NumSaves", 0);
		}
		if (saveNum > 0)
		{
			if (LoadNewestSave())
			{
				willPlayAnimation = false;
			}
			else
			{
				Debug.LogWarning("couldn't load save");
				willPlayAnimation = true;
				PlayerPrefs.SetInt("NumSaves", 0);
				if (PlayerPrefs.HasKey("SaveGame0"))
				{
					PlayerPrefs.DeleteKey("SaveGame0");
				}
				if (PlayerPrefs.HasKey("SaveGame1"))
				{
					PlayerPrefs.DeleteKey("SaveGame1");
				}
			}
		}
		else
		{
			Debug.LogWarning("no save detected");
			willPlayAnimation = true;
		}
		ApplicationVersion = Application.version;
		readyToGo = true;
	}

	private void OnApplicationQuit()
	{
		exiting = true;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			anxiousToSave = true;
		}
	}

	public void ResetPlayerButNotDialogue()
	{
		SaveState saveState = new SaveState();
		saveState.hingePos = -1109.8397f;
		saveState.hingeVel = 0f;
		saveState.sliderPos = -0.7706918f;
		saveState.sliderVel = 0f;
		saveState.camPos = new Vector3(-42.46717f, -1.342597f, -20f);
		saveState.playerPos = new Vector3(-44.292595f, -2.4218144f, 0f);
		saveState.playerRot = Quaternion.Euler(0f, 0f, 359.9106f);
		saveState.rbLinearVelocities = new Vector2[6]
		{
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero
		};
		saveState.rbAngularVelocities = new float[6];
		saveState.rbPositions = new Vector2[6]
		{
			new Vector2(-44.292595f, -2.4218144f),
			new Vector2(-44.292595f, -1.8218141f),
			new Vector2(-44.292595f, -1.8218148f),
			new Vector2(-44.39062f, -1.7534542f),
			new Vector2(-43.430946f, -2.4262254f),
			new Vector2(-42.546608f, -3.046185f)
		};
		saveState.rbAngles = new float[6] { -0.08937922f, 0f, -34.891838f, 145.10817f, 144.96793f, 144.96788f };
		saveState.saveNum = 1;
		saveState.version = ApplicationVersion;
		saveState.keyDialogDone = Convert.ToBase64String(narrator.getKeyDialogDoneList());
		saveState.observationDialogDone = Convert.ToBase64String(narrator.getObservationDialogDoneList());
		saveState.condolenceDialogDone = Convert.ToBase64String(narrator.getCondolenceDialogDoneList());
		Load(saveState);
		sf.StartScene();
		willPlayAnimation = false;
	}

	private void OnApplicationPause(bool paused)
	{
		SaveGameNow(true);
	}

	public void SaveGameNow(bool writeToDisk)
	{
		if (stream != null && !dispatched && !gc.creditsUp)
		{
			currentSave = ((currentSave == 0) ? 1 : 0);
			saveString = ((currentSave != 0) ? saveString1 : saveString0);
			debugSaves[currentSave] = Save();
			if (writeToDisk)
			{
				dispatched = true;
				EZThread.ExecuteInBackground(SerializeSaveThreadWrite, SerialzeSaveThreadResultWrite);
			}
			else
			{
				dispatched = true;
				EZThread.ExecuteInBackground(SerializeSaveThread, SerialzeSaveThreadResult);
			}
		}
	}

	private string SerializeSaveThread()
	{
		if (exiting)
		{
			return null;
		}
		if (gc.creditsUp)
		{
			return null;
		}
		stream.Clear();
		if (exiting)
		{
			return null;
		}
		try
		{
			serializer.Serialize(streamWriter, debugSaves[currentSave]);
		}
		catch
		{
			Debug.LogError("save serialization failed");
			return null;
		}
		if (exiting)
		{
			return null;
		}
		try
		{
			utfString = stream.ToString();
		}
		catch
		{
			Debug.LogError("UTF8 encode failed");
			return null;
		}
		return utfString;
	}

	private void SerialzeSaveThreadResult(object result)
	{
		dispatched = false;
		if (!exiting && !gc.creditsUp && result != null)
		{
			encodedstring = result as string;
			PlayerPrefs.SetString(saveString, encodedstring);
			PlayerPrefs.SetInt("NumSaves", saveNum);
		}
	}

	private string SerializeSaveThreadWrite()
	{
		if (exiting)
		{
			return null;
		}
		if (gc.creditsUp)
		{
			return null;
		}
		stream.Clear();
		if (exiting)
		{
			return null;
		}
		try
		{
			serializer.Serialize(streamWriter, debugSaves[currentSave]);
		}
		catch
		{
			Debug.LogError("save serialization failed");
			return null;
		}
		if (exiting)
		{
			return null;
		}
		try
		{
			utfString = stream.ToString();
		}
		catch
		{
			Debug.LogError("UTF8 encode failed");
			return null;
		}
		return utfString;
	}

	private void SerialzeSaveThreadResultWrite(object result)
	{
		dispatched = false;
		if (!exiting && !gc.creditsUp && result != null)
		{
			encodedstring = result as string;
			PlayerPrefs.SetString(saveString, encodedstring);
			PlayerPrefs.SetInt("NumSaves", saveNum);
			PlayerPrefs.Save();
		}
	}

	private void Update()
	{
		if (willPlayAnimation)
		{
			if (pc == null)
			{
				pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
			}
			if (pc != null)
			{
				pc.PlayOpeningAnimation();
				willPlayAnimation = false;
			}
		}
		if (gc.creditsUp || !readyToGo)
		{
			return;
		}
		frame++;
		if (anxiousToSave)
		{
			anxiousFrame++;
			if (pc.IsInputIdle || anxiousFrame > 1200)
			{
				SaveGameNow(true);
				savesSinceWrite = 0;
				anxiousToSave = false;
				frame = 0;
				anxiousFrame = 0;
				return;
			}
		}
		if (frame > 60 && Time.timeScale > 0f)
		{
			frame = 0;
			savesSinceWrite++;
			if (savesSinceWrite > savesPerWrite)
			{
				if (pc.IsInputIdle)
				{
					SaveGameNow(true);
					savesSinceWrite = 0;
					anxiousFrame = 0;
					anxiousToSave = false;
					return;
				}
				anxiousToSave = true;
				anxiousFrame = 0;
			}
			SaveGameNow(false);
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.L))
			{
				LoadNewestSave();
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				ResetPlayerButNotDialogue();
			}
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				Debug.Log("Deleting old save");
				PlayerPrefs.DeleteKey("SaveGame0");
				PlayerPrefs.DeleteKey("SaveGame1");
			}
		}
	}

	public SaveState Save()
	{
		newSave = new SaveState();
		newSave.hingeVel = hinge.jointSpeed;
		newSave.hingePos = hinge.jointAngle;
		newSave.sliderVel = slider.jointSpeed;
		newSave.sliderPos = slider.jointTranslation;
		newSave.playerPos = playerTransform.position;
		newSave.playerRot = playerTransform.rotation;
		rbs = pc.AttachedRigidBodies;
		newSave.rbLinearVelocities = rbLinearVelocities;
		newSave.rbAngularVelocities = rbAngularVelocities;
		newSave.rbPositions = rbPositions;
		newSave.rbAngles = rbAngles;
		for (int i = 0; i < rbs.Length; i++)
		{
			newSave.rbLinearVelocities[i] = rbs[i].velocity;
			newSave.rbAngularVelocities[i] = rbs[i].angularVelocity;
			newSave.rbPositions[i] = rbs[i].position;
			newSave.rbAngles[i] = rbs[i].rotation;
		}
		saveNum++;
		newSave.saveNum = saveNum;
		newSave.camPos = Camera.main.transform.position;
		newSave.keyDialogDone = Convert.ToBase64String(narrator.getKeyDialogDoneList());
		newSave.observationDialogDone = Convert.ToBase64String(narrator.getObservationDialogDoneList());
		newSave.condolenceDialogDone = Convert.ToBase64String(narrator.getCondolenceDialogDoneList());
		newSave.timePlayed = narrator.timePlayedThisGame;
		newSave.speedrun = narrator.speedrun;
		newSave.version = ApplicationVersion;
		return newSave;
	}

	public bool LoadNewestSave()
	{
		pc.loadedFromSave = true;
		string @string = PlayerPrefs.GetString("SaveGame0");
		string string2 = PlayerPrefs.GetString("SaveGame1");
		SaveState saveState = null;
		SaveState saveState2 = null;
		if (@string.Length > 0)
		{
			using (TextReader textReader = new StringReader(@string))
			{
				try
				{
					saveState = (SaveState)serializer.Deserialize(textReader);
				}
				catch
				{
					Debug.LogError("Save state 1 deserialize failed");
					saveState = null;
				}
			}
		}
		if (string2.Length > 0)
		{
			using (TextReader textReader2 = new StringReader(string2))
			{
				try
				{
					saveState2 = (SaveState)serializer.Deserialize(textReader2);
				}
				catch
				{
					Debug.LogError("Save state 2 deserialize failed");
					saveState2 = null;
				}
			}
		}
		if (saveState == null && saveState2 == null)
		{
			Debug.LogWarning("no save file to load");
			return false;
		}
		if (saveState == null && saveState2 != null)
		{
			Debug.LogWarning("loading save 2 since 1 was not present");
			Load(saveState2);
			saveNum = saveState2.saveNum;
		}
		else if (saveState != null && saveState2 == null)
		{
			Debug.LogWarning("loading save 1 since 2 was not present");
			Load(saveState);
		}
		else if (saveState.saveNum > saveState2.saveNum)
		{
			Load(saveState);
		}
		else
		{
			Load(saveState2);
		}
		return true;
	}

	public void Load(SaveState loadedSave)
	{
		Rigidbody2D[] componentsInChildren = playerTransform.GetComponentsInChildren<Rigidbody2D>();
		if (loadedSave.rbPositions.Length == componentsInChildren.Length)
		{
			Rigidbody2D[] array = componentsInChildren;
			foreach (Rigidbody2D rigidbody2D in array)
			{
				rigidbody2D.Sleep();
			}
			spine1.localScale = Vector3.one;
			spine2.localScale = Vector3.one;
			playerTransform.gameObject.SetActive(false);
			playerTransform.position = loadedSave.playerPos;
			playerTransform.rotation = loadedSave.playerRot;
			playerTransform.gameObject.SetActive(true);
			Camera.main.SendMessage("Teleport", loadedSave.camPos);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].position = loadedSave.rbPositions[j];
				componentsInChildren[j].rotation = loadedSave.rbAngles[j];
				componentsInChildren[j].velocity = loadedSave.rbLinearVelocities[j];
				componentsInChildren[j].angularVelocity = loadedSave.rbAngularVelocities[j];
			}
			cursor.position = hammer.position;
			slider.motor = stillSliderMotor;
			hinge.motor = stillHingeMotor;
			hubJoint.motor = stillHubMotor;
			narrator.setDialogDoneLists(Convert.FromBase64String(loadedSave.keyDialogDone), Convert.FromBase64String(loadedSave.observationDialogDone), Convert.FromBase64String(loadedSave.condolenceDialogDone));
			narrator.speedrun = loadedSave.speedrun;
			Rigidbody2D[] array2 = componentsInChildren;
			foreach (Rigidbody2D rigidbody2D2 in array2)
			{
				rigidbody2D2.WakeUp();
			}
			Time.timeScale = 1f;
			narrator.timePlayedThisGame = loadedSave.timePlayed;
			pc.StartAnimator();
		}
	}
}
