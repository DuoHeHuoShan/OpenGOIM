using System.Collections.Generic;
using FxProNS;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/FxPro™")]
public class FxPro : MonoBehaviour
{
	public EffectsQuality Quality = EffectsQuality.Normal;

	private static Material _mat;

	private static Material _tapMat;

	private Camera _effectCamera;

	public Texture2D LensDirtTexture;

	[Range(0f, 2f)]
	public float LensDirtIntensity = 1f;

	public bool ChromaticAberration = true;

	public bool ChromaticAberrationPrecise;

	[Range(1f, 2.5f)]
	public float ChromaticAberrationOffset = 1f;

	[Range(0f, 1f)]
	public float SCurveIntensity = 0.5f;

	public bool LensCurvatureEnabled = true;

	[Range(1f, 2f)]
	public float LensCurvaturePower = 1.1f;

	public bool LensCurvaturePrecise;

	[Range(0f, 1f)]
	public float FilmGrainIntensity = 0.5f;

	[Range(1f, 10f)]
	public float FilmGrainTiling = 4f;

	[Range(0f, 1f)]
	public float VignettingIntensity = 0.5f;

	public bool DOFEnabled = true;

	public bool BlurCOCTexture = true;

	public DOFHelperParams DOFParams = new DOFHelperParams();

	public bool VisualizeCOC;

	private List<Texture2D> _filmGrainTextures;

	public bool ColorEffectsEnabled = true;

	public Color CloseTint = new Color(1f, 0.5f, 0f, 1f);

	public Color FarTint = new Color(0f, 0f, 1f, 1f);

	[Range(0f, 1f)]
	public float CloseTintStrength = 0.5f;

	[Range(0f, 1f)]
	public float FarTintStrength = 0.5f;

	[Range(0f, 2f)]
	public float DesaturateDarksStrength = 0.5f;

	[Range(0f, 1f)]
	public float DesaturateFarObjsStrength = 0.5f;

	public Color FogTint = Color.white;

	[Range(0f, 1f)]
	public float FogStrength = 0.5f;

	public bool HalfResolution;

	private RenderTexture curRenderTex;

	private RenderTexture srcProcessed;

	private RenderTexture cocRenderTex;

	private RenderTexture dofRenderTex;

	public static Material Mat
	{
		get
		{
			if (null == _mat)
			{
				Material material = new Material(Shader.Find("Hidden/FxPro"));
				material.hideFlags = HideFlags.HideAndDontSave;
				_mat = material;
			}
			return _mat;
		}
	}

	private static Material TapMat
	{
		get
		{
			if (null == _tapMat)
			{
				Material material = new Material(Shader.Find("Hidden/FxProTap"));
				material.hideFlags = HideFlags.HideAndDontSave;
				_tapMat = material;
			}
			return _tapMat;
		}
	}

	private Camera EffectCamera
	{
		get
		{
			if (null == _effectCamera)
			{
				_effectCamera = GetComponent<Camera>();
			}
			return _effectCamera;
		}
	}

	public void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.LogError("Image effects are not supported on this platform.");
			base.enabled = false;
			return;
		}
		_filmGrainTextures = new List<Texture2D>();
		for (int i = 1; i <= 4; i++)
		{
			string text = "filmgrain_0" + i;
			Texture2D texture2D = Resources.Load(text) as Texture2D;
			if (null == texture2D)
			{
				Debug.LogError("Unable to load grain texture '" + text + "'");
			}
			else
			{
				_filmGrainTextures.Add(texture2D);
			}
		}
	}

	public void Init(bool searchForNonDepthmapAlphaObjects = false)
	{
		if (HalfResolution)
		{
			Screen.SetResolution(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2, Screen.fullScreen, Screen.currentResolution.refreshRate);
		}
		if ((DOFEnabled || ColorEffectsEnabled) && EffectCamera.depthTextureMode == DepthTextureMode.None)
		{
			EffectCamera.depthTextureMode = DepthTextureMode.Depth;
		}
		if (DOFEnabled)
		{
			if (null == DOFParams.EffectCamera)
			{
				DOFParams.EffectCamera = EffectCamera;
			}
			DOFParams.DepthCompression = Mathf.Clamp(DOFParams.DepthCompression, 2f, 8f);
			Singleton<DOFHelper>.Instance.SetParams(DOFParams);
			Singleton<DOFHelper>.Instance.Init(searchForNonDepthmapAlphaObjects);
			if (!DOFParams.DoubleIntensityBlur)
			{
				Singleton<DOFHelper>.Instance.SetBlurRadius((Quality != EffectsQuality.Fastest && Quality != EffectsQuality.Fast) ? 5 : 3);
			}
			else
			{
				Singleton<DOFHelper>.Instance.SetBlurRadius((Quality != EffectsQuality.Fastest && Quality != EffectsQuality.Fast) ? 10 : 5);
			}
		}
		else
		{
			Mat.EnableKeyword("DOF_DISABLED");
			Mat.DisableKeyword("DOF_ENABLED");
		}
		if (FilmGrainIntensity >= 0.001f)
		{
			Mat.SetFloat("_FilmGrainIntensity", FilmGrainIntensity);
			Mat.SetFloat("_FilmGrainTiling", FilmGrainTiling);
			Mat.EnableKeyword("FILM_GRAIN_ON");
			Mat.DisableKeyword("FILM_GRAIN_OFF");
		}
		else
		{
			Mat.EnableKeyword("FILM_GRAIN_OFF");
			Mat.DisableKeyword("FILM_GRAIN_ON");
		}
		if (VignettingIntensity <= 1f)
		{
			Mat.SetFloat("_VignettingIntensity", VignettingIntensity);
			Mat.EnableKeyword("VIGNETTING_ON");
			Mat.DisableKeyword("VIGNETTING_OFF");
		}
		else
		{
			Mat.EnableKeyword("VIGNETTING_OFF");
			Mat.DisableKeyword("VIGNETTING_ON");
		}
	}

	public void OnEnable()
	{
		Init(true);
	}

	public void OnDisable()
	{
		if (null != Mat)
		{
			Object.DestroyImmediate(Mat);
		}
		RenderTextureManager.Instance.Dispose();
		Singleton<DOFHelper>.Instance.Dispose();
	}

	public void OnValidate()
	{
		Init();
	}

	public static RenderTexture DownsampleTex(RenderTexture input, float downsampleBy)
	{
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(Mathf.RoundToInt((float)input.width / downsampleBy), Mathf.RoundToInt((float)input.height / downsampleBy), input.depth, input.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		Graphics.BlitMultiTap(input, renderTexture, TapMat, new Vector2(-1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(1f, -1f));
		return renderTexture;
	}

	private RenderTexture ApplyColorEffects(RenderTexture input)
	{
		if (!ColorEffectsEnabled)
		{
			return input;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		Graphics.Blit(input, renderTexture, Mat, 5);
		return renderTexture;
	}

	private RenderTexture ApplyLensCurvature(RenderTexture input)
	{
		if (!LensCurvatureEnabled)
		{
			return input;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		Graphics.Blit(input, renderTexture, Mat, (!LensCurvaturePrecise) ? 4 : 3);
		return renderTexture;
	}

	private RenderTexture ApplyChromaticAberration(RenderTexture input)
	{
		if (!ChromaticAberration)
		{
			return null;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		Graphics.Blit(input, renderTexture, Mat, 2);
		Mat.SetTexture("_ChromAberrTex", renderTexture);
		return renderTexture;
	}

	private Vector2 ApplyLensCurvature(Vector2 uv, float barrelPower, bool precise)
	{
		uv = uv * 2f - Vector2.one;
		uv.x *= EffectCamera.aspect * 2f;
		float f = Mathf.Atan2(uv.y, uv.x);
		float magnitude = uv.magnitude;
		magnitude = ((!precise) ? Mathf.Lerp(magnitude, magnitude * magnitude, Mathf.Clamp01(barrelPower - 1f)) : Mathf.Pow(magnitude, barrelPower));
		uv.x = magnitude * Mathf.Cos(f);
		uv.y = magnitude * Mathf.Sin(f);
		uv.x /= EffectCamera.aspect * 2f;
		return 0.5f * (uv + Vector2.one);
	}

	private void UpdateLensCurvatureZoom()
	{
		float value = 1f / ApplyLensCurvature(new Vector2(1f, 1f), LensCurvaturePower, LensCurvaturePrecise).x;
		Mat.SetFloat("_LensCurvatureZoom", value);
	}

	private void UpdateFilmGrain()
	{
		if (FilmGrainIntensity >= 0.001f)
		{
			int index = Random.Range(0, 3);
			Mat.SetTexture("_FilmGrainTex", _filmGrainTextures[index]);
			switch (Random.Range(0, 3))
			{
			case 0:
				Mat.SetVector("_FilmGrainChannel", new Vector4(1f, 0f, 0f, 0f));
				break;
			case 1:
				Mat.SetVector("_FilmGrainChannel", new Vector4(0f, 1f, 0f, 0f));
				break;
			case 2:
				Mat.SetVector("_FilmGrainChannel", new Vector4(0f, 0f, 1f, 0f));
				break;
			case 3:
				Mat.SetVector("_FilmGrainChannel", new Vector4(0f, 0f, 0f, 1f));
				break;
			}
		}
	}

	private void RenderEffects(RenderTexture source, RenderTexture destination)
	{
		source.filterMode = FilterMode.Bilinear;
		UpdateFilmGrain();
		curRenderTex = source;
		srcProcessed = source;
		RenderTextureManager.Instance.SafeAssign(ref curRenderTex, DownsampleTex(srcProcessed, 2f));
		if (Quality == EffectsQuality.Fastest)
		{
			RenderTextureManager.Instance.SafeAssign(ref curRenderTex, DownsampleTex(curRenderTex, 2f));
		}
		if (DOFEnabled)
		{
			if (null == DOFParams.EffectCamera)
			{
				Debug.LogError("null == DOFParams.camera");
				return;
			}
			cocRenderTex = RenderTextureManager.Instance.RequestRenderTexture(curRenderTex.width, curRenderTex.height, curRenderTex.depth, curRenderTex.format);
			Singleton<DOFHelper>.Instance.RenderCOCTexture(curRenderTex, cocRenderTex, (!BlurCOCTexture) ? 0f : 1.5f);
			dofRenderTex = RenderTextureManager.Instance.RequestRenderTexture(curRenderTex.width, curRenderTex.height, curRenderTex.depth, curRenderTex.format);
			Singleton<DOFHelper>.Instance.RenderDOFBlur(curRenderTex, dofRenderTex, cocRenderTex);
			Mat.SetTexture("_DOFTex", dofRenderTex);
			Mat.SetTexture("_COCTex", cocRenderTex);
		}
		Graphics.Blit(srcProcessed, destination, Mat, 0);
		RenderTextureManager.Instance.ReleaseRenderTexture(srcProcessed);
		RenderTextureManager.Instance.ReleaseRenderTexture(cocRenderTex);
		RenderTextureManager.Instance.ReleaseRenderTexture(dofRenderTex);
		RenderTextureManager.Instance.ReleaseRenderTexture(curRenderTex);
	}

	[ImageEffectTransformsToLDR]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderEffects(source, destination);
		RenderTextureManager.Instance.ReleaseAllRenderTextures();
	}
}
