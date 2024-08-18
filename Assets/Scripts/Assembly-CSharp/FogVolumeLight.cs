using UnityEngine;

[ExecuteInEditMode]
public class FogVolumeLight : MonoBehaviour
{
	public bool IsAddedToNormalLight;

	public bool IsPointLight;

	public bool Enabled = true;

	public Color Color = Color.white;

	public float Intensity = 1f;

	public float Range = 10f;

	public float Angle = 30f;

	private void OnEnable()
	{
		SphereCollider component = GetComponent<SphereCollider>();
		if (component != null)
		{
			Object.Destroy(component);
		}
	}
}
