using UnityEngine;

[ExecuteInEditMode]
public class ColorAnimation : MonoBehaviour
{
	private float X;

	private float Y;

	private float Z;

	[Range(0f, 10f)]
	public float _ColorSpeed = 6f;

	private float ColorSpeed;

	private Vector3 RandomRangeXYZ;

	[SerializeField]
	[Range(1f, 300f)]
	private float Intensity = 8f;

	private void OnEnable()
	{
		RandomRangeXYZ.x = Random.Range(0f, 1f);
		RandomRangeXYZ.y = Random.Range(0f, 1f);
		RandomRangeXYZ.z = Random.Range(0f, 1f);
	}

	private void Update()
	{
		ColorSpeed += Time.deltaTime * _ColorSpeed;
		X = Mathf.Sin(ColorSpeed * RandomRangeXYZ.x) * 0.5f + 0.5f;
		Y = Mathf.Sin(ColorSpeed * RandomRangeXYZ.y) * 0.5f + 0.5f;
		Z = Mathf.Sin(ColorSpeed * RandomRangeXYZ.z) * 0.5f + 0.5f;
		float num = Mathf.Sin(ColorSpeed * RandomRangeXYZ.z) * 0.5f + 0.5f;
		Color color = new Color(X, Y, Z, 1f);
		GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", num * Intensity * color);
	}
}
