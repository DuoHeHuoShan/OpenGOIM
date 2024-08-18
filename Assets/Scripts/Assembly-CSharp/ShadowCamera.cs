using System;
using FogVolumeUtilities;
using UnityEngine;

[ExecuteInEditMode]
public class ShadowCamera : MonoBehaviour
{
	[Serializable]
	public enum TextureSize
	{
		_64 = 0x40,
		_128 = 0x80,
		_256 = 0x100,
		_512 = 0x200,
		_1024 = 0x400
	}

	private Camera ThisCamera;

	private GameObject Dad;

	private FogVolume Fog;

	public RenderTexture RT_Opacity;

	public RenderTexture RT_OpacityBlur;

	public RenderTexture RT_PostProcess;

	public TextureSize textureSize = TextureSize._128;

	[Range(0f, 10f)]
	public int iterations = 3;

	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	public int Downsampling = 2;

	private Shader blurShader;

	private Shader PostProcessShader;

	private Material blurMaterial;

	private Material postProcessMaterial;

	public TextureSize SetTextureSize
	{
		get
		{
			return textureSize;
		}
		set
		{
			if (value != textureSize)
			{
				SetQuality(value);
			}
		}
	}

	protected Material BlurMaterial
	{
		get
		{
			if (blurMaterial == null)
			{
				blurMaterial = new Material(blurShader);
				blurMaterial.hideFlags = HideFlags.DontSave;
			}
			return blurMaterial;
		}
	}

	protected Material PostProcessMaterial
	{
		get
		{
			if (postProcessMaterial == null)
			{
				postProcessMaterial = new Material(PostProcessShader);
				postProcessMaterial.hideFlags = HideFlags.DontSave;
			}
			return postProcessMaterial;
		}
	}

	public RenderTexture GetOpacityRT()
	{
		return RT_Opacity;
	}

	public RenderTexture GetOpacityBlurRT()
	{
		return RT_OpacityBlur;
	}

	private void SetQuality(TextureSize value)
	{
		textureSize = value;
	}

	protected void GetRT(ref RenderTexture rt, int size, string name)
	{
		ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, BlurMaterial, new Vector2(0f - num, 0f - num), new Vector2(0f - num, num), new Vector2(num, num), new Vector2(num, 0f - num));
	}

	private void DownSample(RenderTexture source, RenderTexture dest)
	{
		float num = 1f;
		Graphics.BlitMultiTap(source, dest, BlurMaterial, new Vector2(0f - num, 0f - num), new Vector2(0f - num, num), new Vector2(num, num), new Vector2(num, 0f - num));
	}

	private void Blur(RenderTexture Input, int BlurRTSize)
	{
		RenderTexture rt = null;
		GetRT(ref RT_OpacityBlur, BlurRTSize, "Shadow blurred");
		GetRT(ref rt, BlurRTSize, "Shadow blurred");
		DownSample(Input, RT_OpacityBlur);
		for (int i = 0; i < iterations; i++)
		{
			FourTapCone(RT_OpacityBlur, rt, i);
			ExtensionMethods.Swap(ref RT_OpacityBlur, ref rt);
		}
		Shader.SetGlobalTexture("RT_OpacityBlur", RT_OpacityBlur);
		Fog.RT_OpacityBlur = RT_OpacityBlur;
	}

	private void RenderShadowMap()
	{
		Fog.FogVolumeShader.maximumLOD = 100;
		SetQuality(textureSize);
		GetRT(ref RT_Opacity, (int)textureSize, "Opacity");
		ThisCamera.targetTexture = RT_Opacity;
		ThisCamera.Render();
		Fog.RT_Opacity = RT_Opacity;
		if (RT_Opacity != null)
		{
			GetRT(ref RT_PostProcess, (int)textureSize, "Shadow PostProcess");
			PostProcessMaterial.SetFloat("ShadowColor", Fog.ShadowColor.a);
			Graphics.Blit(RT_Opacity, RT_PostProcess, PostProcessMaterial);
			Graphics.Blit(RT_PostProcess, RT_Opacity);
			if (iterations > 0)
			{
				Blur(RT_Opacity, (int)textureSize >> Downsampling);
			}
			else
			{
				Shader.SetGlobalTexture("RT_OpacityBlur", RT_Opacity);
				Fog.RT_OpacityBlur = RT_Opacity;
			}
			Fog.RT_Opacity = RT_Opacity;
		}
		BlurMaterial.SetFloat("ShadowColor", Fog.ShadowColor.a);
		Fog.FogVolumeShader.maximumLOD = 600;
	}

	private void ShaderLoad()
	{
		blurShader = Shader.Find("Hidden/Fog Volume/BlurEffectConeTap");
		if (blurShader == null)
		{
			MonoBehaviour.print("Hidden / Fog Volume / BlurEffectConeTap #SHADER ERROR#");
		}
		PostProcessShader = Shader.Find("Hidden/Fog Volume/Shadow Postprocess");
		if (PostProcessShader == null)
		{
			MonoBehaviour.print("Hidden/Fog Volume/Shadow Postprocess #SHADER ERROR#");
		}
	}

	private void OnEnable()
	{
		ShaderLoad();
		Dad = base.transform.parent.gameObject;
		Fog = Dad.GetComponent<FogVolume>();
		ThisCamera = base.gameObject.GetComponent<Camera>();
		CameraTransform();
	}

	public void CameraTransform()
	{
		if (ThisCamera != null)
		{
			ThisCamera.orthographicSize = Dad.GetComponent<FogVolume>().fogVolumeScale.x / 2f;
			ThisCamera.transform.position = Dad.transform.position;
			ThisCamera.farClipPlane = Fog.fogVolumeScale.y + (float)Fog.shadowCameraPosition;
			Vector3 translation = new Vector3(0f, 0f, Fog.fogVolumeScale.y / 2f - (float)Fog.shadowCameraPosition);
			ThisCamera.transform.Translate(translation, Space.Self);
			Quaternion quaternion = Quaternion.Euler(90f, 0f, 0f);
			ThisCamera.transform.rotation = Dad.transform.rotation * quaternion;
			ThisCamera.enabled = false;
			if (Fog.SunAttached)
			{
				Fog.Sun.transform.rotation = Dad.transform.rotation * quaternion;
			}
		}
	}

	private void Update()
	{
		if (Fog.IsVisible && Fog.CastShadows && ExtensionMethods.TimeSnap(Fog.ShadowCameraSkippedFrames))
		{
			RenderShadowMap();
		}
	}

	private void OnDisable()
	{
		RenderTexture.active = null;
		ThisCamera.targetTexture = null;
		if ((bool)RT_Opacity)
		{
			UnityEngine.Object.DestroyImmediate(RT_Opacity);
		}
		if ((bool)RT_OpacityBlur)
		{
			UnityEngine.Object.DestroyImmediate(RT_OpacityBlur);
		}
		if ((bool)RT_PostProcess)
		{
			UnityEngine.Object.DestroyImmediate(RT_PostProcess);
		}
		if ((bool)blurMaterial)
		{
			UnityEngine.Object.DestroyImmediate(blurMaterial);
		}
		if ((bool)postProcessMaterial)
		{
			UnityEngine.Object.DestroyImmediate(postProcessMaterial);
		}
	}
}
