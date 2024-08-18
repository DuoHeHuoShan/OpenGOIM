using System;
using System.Collections;
using System.Collections.Generic;
using Noodle;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public HingeJoint2D hj;

	public SliderJoint2D sj;

	public Transform tip;

	private JointMotor2D hingeJointMotorValues;

	private JointMotor2D slider;

	public float deadzone;

	public Transform fakeCursor;

	public Transform potMesh;

	private Rigidbody2D fakeCursorRB;

	private Vector2 cursorInputVelocity;

	private Vector2 prevCursorInputVelocity;

	private Vector2 prevMouseInput;

	private float mouseVelocityAverage;

	private float oldAngle;

	[NonSerialized]
	public float mouseSensitivity = 1f;

	private float angleEpsilon = 1f;

	private float pauseInputTimer;

	private bool mouseSnap;

	public bool trackpad;

	public AnimationCurve trackpadCurve;

	public AnimationCurve mouseCurve;

	public AnimationCurve mobileCurve;

	public AnimationCurve mobileCurve60;

	public AnimationCurve stickCurve;

	private bool input_enabled;

	public bool loadedFromSave;

	private PolygonCollider2D hammerCollider;

	private List<Collider2D> ghostedCols;

	private PoseControl pose;

	private ReflectionProbe probe;

	private float mobileScreenDPIAdjust;

	private bool skipfirstMoveInput;

	public float rawInputScaling = 0.75f;

	public float cursorVelocityToWorldUnits = 1f;

	private static float dpi;

	[Header("-------------------------------------------------------------")]
	[Space(4f)]
	public GameObject highAccel;

	public GameObject stdAccel;

	public GameObject lowAccel;

	public GameObject noAccel;

	private bool noAccelCurve;

	private MobileManager mobileMan;

	private int sensitivityModifier;

	public Rigidbody2D[] AttachedRigidBodies;

	private static int originalHeight;

	private static int originalWidth;

	private static float originalDpi;

	private bool menuPause;

	private Vector2 mw = Vector2.zero;

	private long fixedFrameCounter;

	public bool IsInputIdle
	{
		get
		{
			return cursorInputVelocity == Vector2.zero;
		}
	}


	private Vector2 mouseInput;
	private Vector2 oldMouse;
	private int inputsToSkip;
	private Vector3 oldHammerPos;

	private void Awake()
	{
		loadedFromSave = false;
		SaveScreenDimensions();
		dpi = Screen.dpi;
		if (dpi <= 0f)
		{
			if (DeviceDisplay.deviceID != "iPhone10,3")
			{
				Debug.LogError("Device " + DeviceDisplay.deviceID + " returned 0 dpi");
			}
			NoodleManager.Instance.SubmitNoodleEvent("DpiZeroError");
			dpi = 400f;
		}
	}

	private void Start()
	{
		inputsToSkip = 0;
		mouseInput = Vector2.zero;
		oldHammerPos = tip.position;
		ghostedCols = new List<Collider2D>();
		hammerCollider = tip.GetComponent<PolygonCollider2D>();
		cursorInputVelocity = Vector2.zero;
		hingeJointMotorValues = hj.motor;
		slider = sj.motor;
		oldAngle = 0f;
		fakeCursorRB = fakeCursor.GetComponent<Rigidbody2D>();
		mouseVelocityAverage = 0f;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		pauseInputTimer = 2f;
		mouseSnap = false;
		if (PlayerPrefs.HasKey("Trackpad"))
		{
			trackpad = PlayerPrefs.GetInt("Trackpad") == 1;
		}
		else
		{
			trackpad = true;
		}
		mobileMan = GameObject.FindGameObjectWithTag("MobileManager").GetComponent<MobileManager>();
		MobileManager.MobileScale deviceScale = mobileMan.getDeviceScale();
		if (Application.isEditor && false)
		{
			dpi = 600f;
		}
		mobileScreenDPIAdjust = 300f / dpi;
		prevCursorInputVelocity = Vector2.zero;
		cursorInputVelocity = Vector2.zero;
		switch (PlayerPrefs.GetInt("mobileAccel", 1))
		{
		case 1:
			setStandardCurve();
			break;
		case 2:
			setHighCurve();
			break;
		case 0:
			setLowCurve();
			break;
		case 3:
			setNoCurve();
			break;
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			StartCoroutine(LockCursor());
		}
	}

	private IEnumerator LockCursor()
	{
		yield return new WaitForSeconds(0.1f);
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void GhostHammer(PolygonCollider2D otherCol)
	{
		ghostedCols.Add(otherCol);
		Physics2D.IgnoreCollision(hammerCollider, otherCol);
	}

	private static void SaveScreenDimensions()
	{
		originalHeight = Screen.height;
		originalWidth = Screen.width;
		originalDpi = Screen.dpi;
		if (originalDpi < 1f)
		{
			originalDpi = 400f;
		}
	}

	public static IEnumerator AdjustScreenResolution(float resRatio)
	{
		float aspectRatio = (float)originalWidth / (float)originalHeight;
		int newHeight = originalHeight;
		if (originalHeight > 720)
		{
			newHeight = Mathf.RoundToInt(Mathf.Max(720f, (float)originalHeight * resRatio));
		}
		int newWidth = Mathf.RoundToInt((float)newHeight * aspectRatio);
		Debug.LogFormat("[ScreenRes] Original Screen {0}x{1}, dpi {2}", originalWidth, originalHeight, originalDpi);
		Debug.LogFormat("[ScreenRes] Changing Resolution to {0}x{1}", newWidth, newHeight);
		Screen.SetResolution(newWidth, newHeight, true);
		yield return null;
		dpi = originalDpi * ((float)Screen.height / (float)originalHeight);
		Debug.LogFormat("[ScreenRes] Expecting Screen.dpi to now be {0}", dpi);
		Debug.LogFormat("[ScreenRes] New Screen {0}x{1}, dpi {2}", Screen.width, Screen.height, Screen.dpi);
	}

	public void SetSensitivity(float newSensitivity)
	{
		mouseSensitivity = newSensitivity;
	}

	public void PauseInput(float timeToPause)
	{
		pauseInputTimer = timeToPause * 2f;
	}

	public void StopAnimator()
	{
		if (pose == null)
		{
			pose = GetComponentInChildren<PoseControl>();
		}
		pose.StopAnimator();
	}

	public void StartAnimator()
	{
		if (pose == null)
		{
			pose = GetComponentInChildren<PoseControl>();
		}
		pose.StartAnimator();
	}

	public void PlayOpeningAnimation()
	{
		GetComponentInChildren<PoseControl>().PlayOpeningAnimation();
	}

	public void HammerReturn()
	{
		if (!(tip.position.y - 0.4f < base.transform.position.y))
		{
			mouseSnap = true;
			fakeCursorRB.MovePosition(tip.position);
		}
	}

	public void Pause()
	{
		menuPause = true;
	}

	public void UnPause()
	{
		menuPause = false;
	}

	private void UpdateMouse() {
		float num = 0f;
		float num2 = 0f;
		if (!mobileMan.Ready)
		{
			return;
		}
		if (mouseSnap)
		{
			mouseSnap = false;
		}
		else
		{
			// trackpad = true;
			if (trackpad)
			{
				oldMouse = mouseInput;
				mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
				mouseInput *= trackpadCurve.Evaluate(mouseInput.magnitude);
				mouseInput = Vector2.Lerp(oldMouse, mouseInput, 0.5f);
			}
			else
			{
				oldMouse = mouseInput;
				mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
				mouseInput *= mouseCurve.Evaluate(mouseInput.magnitude);
				mouseInput = Vector2.Lerp(oldMouse, mouseInput, 0.5f);
			}
			Vector2 vector3 = Vector2.zero;
			if (vector3.sqrMagnitude > mouseInput.sqrMagnitude)
			{
				mouseInput.x = vector3.x;
				mouseInput.y = vector3.y;
			}
			cursorInputVelocity = mouseInput;
	}}

	private void Update()
	{
		if(Input.mousePresent)
		{
			UpdateMouse();
			return;
		}
		if (!mobileMan.Ready)
		{
			return;
		}
		bool flag = false;
		Vector2 vector = Vector2.zero;
		float num = 0f;
		TouchPhase touchPhase = TouchPhase.Canceled;
		if (Input.touchSupported)
		{
			if (Input.touchCount == 0)
			{
				return;
			}
			Touch touch = default(Touch);
			for (int i = 0; i < Input.touchCount; i++)
			{
				touch = Input.GetTouch(i);
				if (touch.fingerId == 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				vector = touch.deltaPosition;
				num = touch.deltaTime;
				touchPhase = touch.phase;
			}
		}
		else
		{
			vector = (Vector2)Input.mousePosition - prevMouseInput;
			prevMouseInput = Input.mousePosition;
			if (Mathf.Approximately(vector.magnitude, 0f))
			{
				vector = Vector2.zero;
			}
			num = Time.deltaTime;
			touchPhase = ((!Input.GetMouseButtonDown(0)) ? (Input.GetMouseButtonUp(0) ? TouchPhase.Ended : ((Input.GetMouseButton(0) && vector != Vector2.zero) ? TouchPhase.Moved : ((!Input.GetMouseButton(0) || !(vector == Vector2.zero)) ? TouchPhase.Canceled : TouchPhase.Stationary))) : TouchPhase.Began);
			flag = touchPhase != TouchPhase.Canceled;
		}
		if (!flag)
		{
			return;
		}
		switch (touchPhase)
		{
		case TouchPhase.Began:
			skipfirstMoveInput = true;
			break;
		case TouchPhase.Moved:
		{
			if (skipfirstMoveInput)
			{
				skipfirstMoveInput = false;
				break;
			}
			prevCursorInputVelocity = cursorInputVelocity;
			float num2 = dpi;
			Vector2 vector2 = vector;
			Vector2 vector3 = vector2 / num2 * 2.54f * 10f;
			Vector2 vector4 = vector3 / num;
			cursorInputVelocity = vector4;
			cursorInputVelocity *= 0.05f;
			cursorInputVelocity *= mouseSensitivity + 0.9f;
			cursorInputVelocity *= rawInputScaling;
			float magnitude = cursorInputVelocity.magnitude;
			AnimationCurve animationCurve = mobileCurve60;
			if (!noAccelCurve)
			{
				float num3 = animationCurve.Evaluate(magnitude);
				cursorInputVelocity *= num3;
			}
			cursorInputVelocity = Vector2.Lerp(prevCursorInputVelocity, cursorInputVelocity, 0.95f);
			break;
		}
		default:
			cursorInputVelocity = Vector2.zero;
			break;
		}
	}

	private void FixedUpdate()
	{
		fixedFrameCounter++;
		if (!mobileMan.Ready || menuPause)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		if (mouseSnap)
		{
			mouseSnap = false;
		}
		else
		{
			if (pauseInputTimer > 0f)
			{
				pauseInputTimer -= Time.fixedDeltaTime * 2f;
				if (pauseInputTimer < 0f)
				{
					pauseInputTimer = 0f;
				}
				float num3 = Mathf.Clamp01(2f * (1f - pauseInputTimer) - 1f);
				cursorInputVelocity *= num3;
				fakeCursor.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.8f, num3 * 0.2f);
				fakeCursorRB.MovePosition(tip.position);
				mouseVelocityAverage = 0f;
				mw = tip.position;
			}
			else
			{
				if (pauseInputTimer < 0f)
				{
					throw new InvalidOperationException("pauseInputTimer cannot be less than 0");
				}
				mw = fakeCursorRB.position;
			}
			mouseVelocityAverage = Mathf.Lerp(mouseVelocityAverage, Mathf.Max(0.1f * cursorInputVelocity.magnitude, 0.001f), 0.05f) + 0.005f;
			Vector2 vector = cursorInputVelocity;
			vector *= mouseVelocityAverage;
			vector *= cursorVelocityToWorldUnits;
			Vector2 vector2 = fakeCursorRB.position + vector;
			float magnitude = ((Vector2)base.transform.position - vector2).magnitude;
			if (magnitude > 3.5f)
			{
				vector2 = (Vector2)base.transform.position + (vector2 - (Vector2)base.transform.position).normalized * 3.5f;
			}
			Vector2 vector3 = (Vector2)tip.position - mw;
			Vector2 lhs = (tip.position - hj.transform.position).normalized;
			num2 = Vector2.Dot(lhs, vector3.normalized);
			num = vector3.magnitude * num2;
			vector2 += 0.05f * vector3 * 10f * Mathf.Clamp(0.2f - Mathf.Min(mouseVelocityAverage, 0.2f), 0f, 0.2f);
			fakeCursorRB.MovePosition(vector2);
			fakeCursorRB.velocity = vector2 - fakeCursorRB.position;
		}
		Vector2 vector4 = (Vector2)hj.transform.position - mw;
		float current = 57.29578f * Mathf.Atan2(vector4.y, vector4.x) - 180f - hj.referenceAngle;
		float num4 = 0f - Mathf.DeltaAngle(current, hj.jointAngle);
		num4 = Mathf.Sign(num4) * Mathf.Max(Mathf.Abs(num4 / 2f), Mathf.Abs(num4 * num4));
		num4 *= Mathf.Clamp01(Mathf.Abs(num4) / angleEpsilon);
		hingeJointMotorValues.motorSpeed = Mathf.Clamp((num4 * 3f + (num4 - oldAngle) * 0f) * Mathf.Pow(Mathf.Clamp01(vector4.magnitude / deadzone), 2f), -800f, 800f);
		hj.motor = hingeJointMotorValues;
		float num5 = 16f - Mathf.Max(sj.reactionTorque * 0.001f, 5f);
		float num6 = Mathf.Pow(num2, 4f);
		slider.motorSpeed = Mathf.Clamp((0f - num) * Mathf.Abs(num) * num5 * num6, -50f, 50f);
		sj.motor = slider;
		oldAngle = num4;
	}

	public void setHighCurve()
	{
		highAccel.SetActive(true);
		stdAccel.SetActive(false);
		lowAccel.SetActive(false);
		noAccel.SetActive(false);
		noAccelCurve = false;
		mobileCurve.RemoveKey(mobileCurve.keys.Length - 1);
		mobileCurve.RemoveKey(mobileCurve.keys.Length - 1);
		mobileCurve.AddKey(new Keyframe(80f, 2.6f));
		mobileCurve.AddKey(new Keyframe(100f, 2.6f));
		mobileCurve60.RemoveKey(mobileCurve60.keys.Length - 1);
		mobileCurve60.RemoveKey(mobileCurve60.keys.Length - 1);
		mobileCurve60.AddKey(new Keyframe(80f, 2.6f));
		mobileCurve60.AddKey(new Keyframe(100f, 2.6f));
		PlayerPrefs.SetInt("mobileAccel", 2);
		PlayerPrefs.Save();
	}

	public void setStandardCurve()
	{
		stdAccel.SetActive(true);
		lowAccel.SetActive(false);
		highAccel.SetActive(false);
		noAccel.SetActive(false);
		noAccelCurve = false;
		mobileCurve.RemoveKey(mobileCurve.keys.Length - 1);
		mobileCurve.RemoveKey(mobileCurve.keys.Length - 1);
		mobileCurve.AddKey(new Keyframe(80f, 2.3f));
		mobileCurve.AddKey(new Keyframe(100f, 2.3f));
		mobileCurve60.RemoveKey(mobileCurve60.keys.Length - 1);
		mobileCurve60.RemoveKey(mobileCurve60.keys.Length - 1);
		mobileCurve60.AddKey(new Keyframe(80f, 2.3f));
		mobileCurve60.AddKey(new Keyframe(100f, 2.3f));
		PlayerPrefs.SetInt("mobileAccel", 1);
		PlayerPrefs.Save();
	}

	public void setLowCurve()
	{
		lowAccel.SetActive(true);
		stdAccel.SetActive(false);
		highAccel.SetActive(false);
		noAccel.SetActive(false);
		noAccelCurve = false;
		mobileCurve.RemoveKey(mobileCurve.keys.Length - 1);
		mobileCurve.RemoveKey(mobileCurve.keys.Length - 1);
		mobileCurve.AddKey(new Keyframe(80f, 2f));
		mobileCurve.AddKey(new Keyframe(100f, 2f));
		mobileCurve60.RemoveKey(mobileCurve60.keys.Length - 1);
		mobileCurve60.RemoveKey(mobileCurve60.keys.Length - 1);
		mobileCurve60.AddKey(new Keyframe(80f, 2f));
		mobileCurve60.AddKey(new Keyframe(100f, 2f));
		PlayerPrefs.SetInt("mobileAccel", 0);
		PlayerPrefs.Save();
	}

	public void setNoCurve()
	{
		noAccel.SetActive(true);
		lowAccel.SetActive(false);
		stdAccel.SetActive(false);
		highAccel.SetActive(false);
		noAccelCurve = true;
		PlayerPrefs.SetInt("mobileAccel", 3);
		PlayerPrefs.Save();
	}
}
