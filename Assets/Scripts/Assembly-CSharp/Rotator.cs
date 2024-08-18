using UnityEngine;

public class Rotator : MonoBehaviour
{
	public float SpinSpeed = 1f;

	public Vector3 Axis = new Vector3(0f, 1f, 0f);

	[SerializeField]
	private Material Skybox;

	public bool rotateSky;

	private float time;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		time += Time.deltaTime;
		base.transform.Rotate(Axis, SpinSpeed, Space.World);
		if (rotateSky && (bool)Skybox)
		{
			Skybox.SetFloat("_Rotation", 0f - base.transform.eulerAngles.y);
		}
	}
}
