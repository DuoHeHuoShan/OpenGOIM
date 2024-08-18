using UnityEngine;

public class FogControl : MonoBehaviour
{
	public MeshRenderer sky;

	public Light sun;

	public AnimationCurve blendCurve;

	public ColorSet[] colorSets;

	private Color topColor;

	private Color bottomColor;

	private Color midColor;

	private float contrast;

	private float exposure;

	private float saturation;

	private void Start()
	{
		topColor = Color.white;
		bottomColor = Color.white;
		midColor = Color.white;
		contrast = 1f;
		exposure = 0f;
		saturation = 1f;
	}

	private void Update()
	{
		float num = 999999f;
		float num2 = -999999f;
		float num3 = 0f;
		float num4 = 0f;
		int num5 = colorSets.Length - 1;
		int num6 = 0;
		float y = base.transform.position.y;
		for (int i = 0; i < colorSets.Length; i++)
		{
			float y2 = colorSets[i].transform.position.y;
			float num7 = y2 - y;
			if (num7 < num && num7 >= 0f)
			{
				num = num7;
				num5 = i;
				num3 = y2;
			}
			else if (num7 > num2 && num7 < 0f)
			{
				num2 = num7;
				num6 = i;
				num4 = y2;
			}
		}
		float t = Mathf.Clamp01((y - num4) / (num3 - num4));
		sun.color = blendColor(colorSets[num6].sun, colorSets[num5].sun, t);
		topColor = blendColor(colorSets[num6].sky.Evaluate(1f), colorSets[num5].sky.Evaluate(1f), t);
		midColor = blendColor(colorSets[num6].sky.Evaluate(0.5f), colorSets[num5].sky.Evaluate(0.5f), t);
		bottomColor = blendColor(colorSets[num6].sky.Evaluate(0f), colorSets[num5].sky.Evaluate(0f), t);
		sky.sharedMaterial.SetColor("_BottomColor", bottomColor);
		sky.sharedMaterial.SetColor("_MidColor", midColor);
		sky.sharedMaterial.SetColor("_TopColor", topColor);
		exposure = Mathf.Lerp(colorSets[num6].exposure, colorSets[num5].exposure, t);
		contrast = Mathf.Lerp(colorSets[num6].contrast, colorSets[num5].contrast, t);
		saturation = Mathf.Lerp(colorSets[num6].saturation, colorSets[num5].saturation, t);
		RenderSettings.ambientSkyColor = topColor * 1.2f;
		RenderSettings.ambientEquatorColor = midColor;
		RenderSettings.ambientGroundColor = bottomColor * 0.5f;
		RenderSettings.fogColor = blendColor(colorSets[num6].fog, colorSets[num5].fog, t);
		RenderSettings.fogDensity = ((!(y > 82f)) ? Mathf.Lerp(0.03f, 0.013f, Mathf.Abs(y) / 82f) : 0.013f);
		RenderSettings.skybox.SetColor("_BottomColor", bottomColor);
		RenderSettings.skybox.SetColor("_MidColor", midColor);
		RenderSettings.skybox.SetColor("_TopColor", topColor);
	}

	private Color blendColor(Color c1, Color c2, float t)
	{
		float num = blendCurve.Evaluate(t);
		float r = c1.r * num + c2.r * (1f - num);
		float g = c1.g * num + c2.g * (1f - num);
		float b = c1.b * num + c2.b * (1f - num);
		return new Color(r, g, b);
	}
}
