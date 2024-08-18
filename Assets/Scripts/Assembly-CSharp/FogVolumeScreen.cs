using FogVolumeUtilities;
using UnityEngine;

[ExecuteInEditMode]
public class FogVolumeScreen : MonoBehaviour
{
	public enum BlurType
	{
		Standard = 0,
		Sgx = 1
	}

	[Header("Scene blur")]
	[Range(1f, 8f)]
	public int Downsample = 8;

	[SerializeField]
	[Range(0.001f, 15f)]
	private float _Falloff = 1f;

	private float FOV_compensation;

	private Shader _BlurShader;

	private Camera UniformFogCamera;

	private GameObject UniformFogCameraGO;

	[HideInInspector]
	public Camera SceneCamera;

	private RenderTexture RT_FogVolumeConvolution;

	private RenderTextureFormat RT_Format;

	[HideInInspector]
	public int FogVolumeLayer = -1;

	[SerializeField]
	[HideInInspector]
	private string _FogVolumeLayerName = "FogVolumeUniform";

	private Material _BlurMaterial;

	[Range(0f, 10f)]
	public int iterations = 3;

	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	[Header("Bloom")]
	[Range(1f, 5f)]
	public int _BloomDowsample = 8;

	[Range(0f, 1.5f)]
	public float threshold = 0.35f;

	[Range(0f, 10f)]
	public float intensity = 2.5f;

	[Range(0f, 1f)]
	public float _Saturation = 1f;

	[Range(0f, 5f)]
	public float blurSize = 1f;

	[Range(1f, 10f)]
	public int blurIterations = 4;

	private BlurType blurType;

	private Shader fastBloomShader;

	private Material _fastBloomMaterial;

	private float initFOV;

	public bool SceneBloom;

	private static FogVolumeScreen _instance;

	private RenderTexture _source;

	public int screenX
	{
		get
		{
			return SceneCamera.pixelWidth;
		}
	}

	public int screenY
	{
		get
		{
			return SceneCamera.pixelHeight;
		}
	}

	public string FogVolumeLayerName
	{
		get
		{
			return _FogVolumeLayerName;
		}
		set
		{
			if (_FogVolumeLayerName != value)
			{
				SetFogVolumeLayer(value);
			}
		}
	}

	private Material BlurMaterial
	{
		get
		{
			if (_BlurMaterial == null)
			{
				_BlurMaterial = new Material(_BlurShader);
				_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _BlurMaterial;
		}
	}

	private Material fastBloomMaterial
	{
		get
		{
			if (_fastBloomMaterial == null)
			{
				_fastBloomMaterial = new Material(fastBloomShader);
				_fastBloomMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _fastBloomMaterial;
		}
	}

	public static FogVolumeScreen instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<FogVolumeScreen>();
			}
			return _instance;
		}
	}

	private void SetFogVolumeLayer(string NewFogVolumeLayerName)
	{
		_FogVolumeLayerName = NewFogVolumeLayerName;
		FogVolumeLayer = LayerMask.NameToLayer(_FogVolumeLayerName);
	}

	private void OnValidate()
	{
		SetFogVolumeLayer(_FogVolumeLayerName);
	}

	private void CreateUniformFogCamera()
	{
		UniformFogCameraGO = GameObject.Find("Uniform Fog Volume Camera");
		if (UniformFogCameraGO == null)
		{
			UniformFogCameraGO = new GameObject();
			UniformFogCameraGO.name = "Uniform Fog Volume Camera";
			if (UniformFogCamera == null)
			{
				UniformFogCamera = UniformFogCameraGO.AddComponent<Camera>();
			}
			UniformFogCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			UniformFogCamera.clearFlags = CameraClearFlags.Color;
			UniformFogCamera.renderingPath = RenderingPath.Forward;
			UniformFogCamera.enabled = false;
			UniformFogCamera.farClipPlane = SceneCamera.farClipPlane;
			UniformFogCamera.GetComponent<Camera>().allowMSAA = false;
		}
		else
		{
			UniformFogCamera = UniformFogCameraGO.GetComponent<Camera>();
		}
		UniformFogCameraGO.hideFlags = HideFlags.None;
		initFOV = SceneCamera.fieldOfView;
	}

	private void OnEnable()
	{
		SceneCamera = base.gameObject.GetComponent<Camera>();
		_BlurShader = Shader.Find("Hidden/FogVolumeDensityFilter");
		if (_BlurShader == null)
		{
			MonoBehaviour.print("Hidden/FogVolumeDensityFilter #SHADER ERROR#");
		}
		fastBloomShader = Shader.Find("Hidden/FogVolumeBloom");
		if (fastBloomShader == null)
		{
			MonoBehaviour.print("Hidden/FogVolumeBloom #SHADER ERROR#");
		}
		CreateUniformFogCamera();
	}

	protected void OnDisable()
	{
		if ((bool)_BlurMaterial)
		{
			Object.DestroyImmediate(_BlurMaterial);
		}
		if ((bool)_fastBloomMaterial)
		{
			Object.DestroyImmediate(_fastBloomMaterial);
		}
		if ((bool)UniformFogCameraGO)
		{
			Object.DestroyImmediate(UniformFogCameraGO);
		}
	}

	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, BlurMaterial, new Vector2(0f - num, 0f - num), new Vector2(0f - num, num), new Vector2(num, num), new Vector2(num, 0f - num));
	}

	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		float num = 1f;
		Graphics.BlitMultiTap(source, dest, BlurMaterial, new Vector2(0f - num, 0f - num), new Vector2(0f - num, num), new Vector2(num, num), new Vector2(num, 0f - num));
	}

	public RenderTextureFormat GetRTFormat()
	{
		RT_Format = ((!SceneCamera.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		return RT_Format;
	}

	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	public RenderTextureReadWrite GetRTReadWrite()
	{
		return (!SceneCamera.allowHDR) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
	}

	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, GetRTFormat(), GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	public void ConvolveFogVolume()
	{
		if (UniformFogCameraGO == null)
		{
			CreateUniformFogCamera();
		}
		int2 size = new int2(screenX, screenY);
		UniformFogCamera.projectionMatrix = SceneCamera.projectionMatrix;
		UniformFogCamera.transform.position = SceneCamera.transform.position;
		UniformFogCamera.transform.rotation = SceneCamera.transform.rotation;
		GetRT(ref RT_FogVolumeConvolution, size, "RT_FogVolumeConvolution");
		UniformFogCamera.targetTexture = RT_FogVolumeConvolution;
		UniformFogCamera.Render();
		Shader.SetGlobalTexture("RT_FogVolumeConvolution", RT_FogVolumeConvolution);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		ConvolveFogVolume();
		GetRT(ref _source, new int2(Screen.width, Screen.height), "_source");
		Graphics.Blit(source, _source);
		fastBloomMaterial.SetTexture("_source", _source);
		BlurMaterial.SetTexture("_source", _source);
		UniformFogCamera.cullingMask = 1 << instance.FogVolumeLayer;
		FOV_compensation = initFOV / SceneCamera.fieldOfView;
		Shader.SetGlobalFloat("FOV_compensation", FOV_compensation);
		fastBloomMaterial.SetFloat("_Falloff", _Falloff);
		RenderTexture renderTexture = RenderTexture.GetTemporary(screenX / Downsample, screenY / Downsample, 0, RT_Format);
		DownSample4x(source, renderTexture);
		for (int i = 0; i < iterations; i++)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(screenX / Downsample, screenY / Downsample, 0, RT_Format);
			FourTapCone(renderTexture, temporary, i);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		if (intensity > 0f)
		{
			Rendering.EnsureKeyword(fastBloomMaterial, "BLOOM", true);
			float num = 2f / (float)_BloomDowsample;
			fastBloomMaterial.SetFloat("_Saturation", _Saturation);
			fastBloomMaterial.SetVector("_Parameter", new Vector4(blurSize * num, 0f, threshold, intensity));
			int width = source.width / _BloomDowsample;
			int height = source.height / _BloomDowsample;
			RenderTexture renderTexture2 = RenderTexture.GetTemporary(width, height, 0, RT_Format);
			renderTexture2.filterMode = FilterMode.Bilinear;
			if (SceneBloom)
			{
				Graphics.Blit(source, renderTexture2, fastBloomMaterial, 1);
			}
			else
			{
				Graphics.Blit(renderTexture, renderTexture2, fastBloomMaterial, 1);
			}
			int num2 = ((blurType != 0) ? 2 : 0);
			for (int j = 1; j < blurIterations; j++)
			{
				fastBloomMaterial.SetVector("_Parameter", new Vector4(blurSize * num + (float)j * 1f, 0f, threshold, intensity));
				RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RT_Format);
				temporary2.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture2, temporary2, fastBloomMaterial, 2 + num2);
				RenderTexture.ReleaseTemporary(renderTexture2);
				renderTexture2 = temporary2;
				temporary2 = RenderTexture.GetTemporary(width, height, 0, RT_Format);
				temporary2.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture2, temporary2, fastBloomMaterial, 3 + num2);
				RenderTexture.ReleaseTemporary(renderTexture2);
				renderTexture2 = temporary2;
			}
			fastBloomMaterial.SetTexture("_Bloom", renderTexture2);
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
		else
		{
			Rendering.EnsureKeyword(fastBloomMaterial, "BLOOM", false);
		}
		Graphics.Blit(renderTexture, destination, fastBloomMaterial, 0);
		RenderTexture.ReleaseTemporary(renderTexture);
	}
}
