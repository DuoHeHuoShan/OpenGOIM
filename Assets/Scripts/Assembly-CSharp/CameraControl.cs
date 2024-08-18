using FluffyUnderware.Curvy;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	public GameObject water;

	public GameObject player;

	public ScreenFader fader;

	public float lastTFValue;

	private Vector3 vel;

	private Vector3 target;

	private float waterLevel;

	private int blurTimer;

	public CurvySpline spline;

	private Camera mainCam;

	private Vector3 lookaheadPos;

	private float lastTF;

	private float lastFogDensity;

	private void OnPreRender()
	{
		RenderSettings.fog = false;
		lastFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = 0f;
	}

	private void OnPostRender()
	{
		RenderSettings.fogDensity = lastFogDensity;
		RenderSettings.fog = true;
	}

	private void Start()
	{
		mainCam = Camera.main;
		vel = Vector3.zero;
		lookaheadPos = Vector3.zero;
		waterLevel = water.GetComponent<MeshRenderer>().bounds.max.y;
		if (player != null)
		{
		}
		target = player.transform.position + new Vector3(0f, 0f, -20f);
		target.y = Mathf.Max(waterLevel + Camera.main.orthographicSize - 2f, target.y);
		Teleport(target);
	}

	public void EnableBlur(bool enable)
	{
	}

	private void Update()
	{
		lastTFValue = spline.GetNearestPointTF(player.transform.position);
		lastTF = Mathf.Lerp(lastTF, lastTFValue, 0.3f);
	}

	private void FixedUpdate()
	{
		if (Application.isPlaying)
		{
			if (player == null)
			{
				player = GameObject.FindGameObjectWithTag("Player");
			}
			if (lastTF > 1f)
			{
				lookaheadPos = player.transform.position;
			}
			else
			{
				Vector3 vector = spline.Interpolate(lastTF + 0.01f);
				Vector3 vector2 = spline.Interpolate(lastTF + 0.02f);
				Vector3 vector3 = spline.Interpolate(lastTF + 0.03f);
				Vector3 b = 0.3333f * (vector + vector2 + vector3);
				lookaheadPos = Vector3.Lerp(lookaheadPos, b, 0.3f);
			}
			Vector3 vector4 = lookaheadPos - player.transform.position;
			vector4.z = 0f;
			target = player.transform.position + vector4.normalized * 2f;
			target.y = Mathf.Max(waterLevel + mainCam.orthographicSize - 2f, target.y);
			target.z = -20f;
			float num = 0.001f * Mathf.Sin(Time.time);
			Vector3 vector5 = new Vector3(num, num, 0f);
			target += vector5;
			Vector3 vector6 = target - base.transform.position;
			vel += 60f * vector6 * Time.fixedDeltaTime - 0.12f * vel;
			base.transform.position = base.transform.position + vel * Time.fixedDeltaTime;
		}
	}

	private Vector3 SuperSmoothLerp(Vector3 x0, Vector3 y0, Vector3 yt, float t, float k)
	{
		Vector3 vector = x0 - y0 + (yt - y0) / (k * t);
		return yt - (yt - y0) / (k * t) + vector * Mathf.Exp((0f - k) * t);
	}

	public void Teleport(Vector3 newPosition)
	{
		base.transform.position = newPosition;
		Time.timeScale = 0.01f;
		lastTF = spline.GetNearestPointTF(player.transform.position);
		if (lastTF < 0f)
		{
			CancelInvoke();
			Invoke("Reteleport", 0.01f);
		}
		else
		{
			lookaheadPos = spline.Interpolate(lastTF + 0.01f) * 0.333f;
			lookaheadPos += spline.Interpolate(lastTF + 0.02f) * 0.333f;
			lookaheadPos += spline.Interpolate(lastTF + 0.03f) * 0.333f;
		}
		vel = Vector3.zero;
	}

	public void Reteleport()
	{
		lastTF = spline.GetNearestPointTF(player.transform.position);
		lookaheadPos = spline.Interpolate(lastTF + 0.01f) * 0.333f;
		lookaheadPos += spline.Interpolate(lastTF + 0.02f) * 0.333f;
		lookaheadPos += spline.Interpolate(lastTF + 0.03f) * 0.333f;
		vel = Vector3.zero;
	}
}
