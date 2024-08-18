using UnityEngine;

[ExecuteInEditMode]
public class Parallax : MonoBehaviour
{
	[Range(0f, 1f)]
	public float distance;

	public Color fogColor = new Color(0.718f, 0.749f, 0.522f, 0.5f);

	private SpriteRenderer ren;

	private MaterialPropertyBlock props;

	public Vector3 AbsolutePos;

	private bool doParallax;

	private void Start()
	{
		ren = GetComponent<SpriteRenderer>();
		props = new MaterialPropertyBlock();
		ren.GetPropertyBlock(props);
		fogColor.a = distance;
		props.SetColor("_FogColor", fogColor);
		ren.SetPropertyBlock(props);
		doParallax = true;
	}

	private void LateUpdate()
	{
		Color color = new Color(fogColor.r, fogColor.g, fogColor.b, distance);
		if (props == null)
		{
			props = new MaterialPropertyBlock();
			ren.GetPropertyBlock(props);
		}
		ren.sortingOrder = 5 + (int)((1f - distance) * 100f);
		if ((Color)props.GetVector("_FogColor") != color)
		{
			props.SetColor("_FogColor", color);
			ren.SetPropertyBlock(props);
		}
		if (Application.isPlaying)
		{
			doParallax = true;
		}
		if (doParallax)
		{
			Vector3 position = Camera.main.transform.position;
			position.z = 0f;
			base.transform.position = AbsolutePos + position * distance;
		}
	}

	public void StartParallaxing()
	{
		doParallax = true;
	}

	public void StopParallaxing()
	{
		doParallax = false;
	}
}
