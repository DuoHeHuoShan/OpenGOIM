using UnityEngine;

public class ExplorationCamera : MonoBehaviour
{
	[Range(1f, 50f)]
	public float sensitivity = 30f;

	[Range(0.1f, 50f)]
	public float smoothing = 5f;

	private float speedZ;

	private float speedX;

	private float speedY;

	[Range(0.1f, 10f)]
	public float Speed = 0.1f;

	[HideInInspector]
	public float InitialSpeed = 0.1f;

	[HideInInspector]
	public float FOV;

	private float initialFOV;

	private Vector2 initialLook;

	private Vector2 Look;

	private Vector3 MovementDirection;

	public float MaxAcceleration = 4f;

	[Range(1f, 10f)]
	public float AccelerationSmoothing = 10f;

	[Range(1f, 10f)]
	public float SpeedSmooth = 1f;

	private Vector2 _smoothMouse;

	public bool HideCursor;

	private float focalLength;

	private float focalSize = 1f;

	public bool ConstantMove;

	[HideInInspector]
	public float tilt;

	private Vector2 lastPos;

	public float tiltSmoothing = 50f;

	public float FOVTransitionSpeed = 0.1f;

	public float inclination = 13f;

	public bool ApplyGravity;

	[Range(0f, 1f)]
	public float gravityAccelerationScale = 0.1f;

	private float TriggerValue;

	private void Start()
	{
		try
		{
			Input.GetAxis("LookHorizontal");
		}
		catch
		{
			MonoBehaviour.print("Import the custom input to support gamepad in this camera script\n http://davidmiranda.me/files/FogVolume3/InputManager.asset");
		}
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseDown)
		{
			lastPos = Event.current.mousePosition;
		}
		else if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove)
		{
			Vector3 vector = Event.current.mousePosition - lastPos;
			Look += new Vector2(vector.x * sensitivity / 50f, (0f - vector.y) * sensitivity / 50f);
			lastPos = Event.current.mousePosition;
		}
	}

	private void OnDestroy()
	{
		Cursor.visible = true;
	}

	private void FixedUpdate()
	{
		try
		{
			if (Mathf.Abs(Input.GetAxis("LookHorizontal")) > 0.017f || Mathf.Abs(Input.GetAxis("LookVertical")) > 0.017f)
			{
				Look += new Vector2(Input.GetAxis("LookHorizontal"), 0f - Input.GetAxis("LookVertical")) * sensitivity;
				if (Input.GetAxis("LookHorizontal") > 0f)
				{
					tilt = Mathf.Lerp(tilt, (0f - inclination) * Mathf.Abs(Input.GetAxis("LookHorizontal")) * sensitivity, 1f / tiltSmoothing);
				}
				else
				{
					tilt = Mathf.Lerp(tilt, inclination * Mathf.Abs(Input.GetAxis("LookHorizontal")) * sensitivity, 1f / tiltSmoothing);
				}
			}
		}
		catch
		{
		}
		tilt = Mathf.Lerp(tilt, 0f, 1f / tiltSmoothing);
		if (Input.GetKey(KeyCode.PageDown))
		{
			focalLength -= 0.05f;
		}
		if (Input.GetKey(KeyCode.PageUp))
		{
			focalLength += 0.05f;
		}
		if (Input.GetKey(KeyCode.End))
		{
			focalSize -= 0.05f;
		}
		if (Input.GetKey(KeyCode.Home))
		{
			focalSize += 0.05f;
		}
		_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, Look.x, 1f / smoothing);
		_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, Look.y, 1f / smoothing);
		base.transform.localEulerAngles = new Vector3(0f - _smoothMouse.y, _smoothMouse.x, tilt);
		if (Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0f || ConstantMove)
		{
			MovementDirection = new Vector3(0f, 0f, 1f);
			speedZ = Mathf.Lerp(speedZ, Speed, 1f / smoothing);
		}
		if (Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < 0f)
		{
			MovementDirection = new Vector3(0f, 0f, -1f);
			speedZ = Mathf.Lerp(speedZ, 0f - Speed, 1f / smoothing);
		}
		if (Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0f)
		{
			MovementDirection = new Vector3(-1f, 0f, 0f);
			speedX = Mathf.Lerp(speedX, 0f - Speed, 1f / smoothing);
		}
		if (Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0f)
		{
			MovementDirection = new Vector3(1f, 0f, 0f);
			speedX = Mathf.Lerp(speedX, Speed, 1f / smoothing);
		}
		if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.JoystickButton4))
		{
			MovementDirection = new Vector3(0f, -1f, 0f);
			speedY = Mathf.Lerp(speedY, 0f - Speed, 1f / smoothing);
		}
		if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton5))
		{
			MovementDirection = new Vector3(0f, 1f, 0f);
			speedY = Mathf.Lerp(speedY, Speed, 1f / smoothing);
		}
		speedZ = Mathf.Lerp(speedZ, 0f, 1f / smoothing);
		MovementDirection.z = speedZ;
		speedY = Mathf.Lerp(speedY, 0f, 1f / smoothing);
		MovementDirection.y = speedY;
		speedX = Mathf.Lerp(speedX, 0f, 1f / smoothing);
		MovementDirection = new Vector3(speedX, speedY, speedZ);
		if (ApplyGravity)
		{
			MovementDirection += Physics.gravity * gravityAccelerationScale;
		}
		base.transform.Translate(MovementDirection);
		Speed = Mathf.Clamp(Speed, 0.001f, 100f);
		if (Input.GetMouseButton(1))
		{
			InitialSpeed += Input.GetAxis("Mouse ScrollWheel") * 0.5f;
			InitialSpeed = Mathf.Max(0.1f, InitialSpeed);
		}
		if (Input.GetMouseButton(1) & Input.GetKey(KeyCode.C) & (FOV > 5f))
		{
			FOV -= 1f;
		}
		if (Input.GetMouseButton(1) & Input.GetKey(KeyCode.Z) & (FOV < 120f))
		{
			FOV += 1f;
		}
		if (!Input.GetMouseButton(1))
		{
			FOV = Mathf.Lerp(FOV, initialFOV, FOVTransitionSpeed);
		}
		GetComponent<Camera>().fieldOfView = FOV;
	}

	private void Update()
	{
		try
		{
			TriggerValue = Mathf.Pow(Input.GetAxis("FasterCamera") * 1f / AccelerationSmoothing + 1f, 2f);
		}
		catch
		{
		}
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			Speed *= MaxAcceleration;
		}
		try
		{
			if (Input.GetAxis("FasterCamera") > 0f && Speed < InitialSpeed * MaxAcceleration)
			{
				Speed *= TriggerValue;
			}
			else if (!Input.GetKey(KeyCode.LeftShift))
			{
				Speed = Mathf.Lerp(Speed, InitialSpeed, 1f / AccelerationSmoothing);
			}
		}
		catch
		{
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				Speed = Mathf.Lerp(Speed, InitialSpeed, 1f / AccelerationSmoothing);
			}
		}
		if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			Speed /= MaxAcceleration;
		}
	}

	private void OnEnable()
	{
		InitialSpeed = Speed;
		if (HideCursor)
		{
			Cursor.visible = false;
		}
		FOV = base.gameObject.GetComponent<Camera>().fieldOfView;
		initialFOV = FOV;
		initialLook = new Vector2(base.transform.eulerAngles.y, base.transform.eulerAngles.x * -1f);
		Look = initialLook;
		_smoothMouse = initialLook;
	}
}
