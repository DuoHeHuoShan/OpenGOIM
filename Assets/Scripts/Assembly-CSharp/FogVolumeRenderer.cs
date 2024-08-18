using FogVolumePlaydeadTAA;
using UnityEngine;

[ExecuteInEditMode]
public class FogVolumeRenderer : MonoBehaviour
{
	public enum BlendMode
	{
		PremultipliedTransparency = 1,
		TraditionalTransparency = 5
	}

	public string FogVolumeResolution;

	public BlendMode _BlendMode = BlendMode.PremultipliedTransparency;

	public bool GenerateDepth = true;

	[SerializeField]
	[Range(0f, 8f)]
	public int _Downsample = 1;

	public bool BilateralUpsampling;

	public bool ShowBilateralEdge;

	[SerializeField]
	public FogVolumeCamera.UpsampleMode USMode = FogVolumeCamera.UpsampleMode.DOWNSAMPLE_CHESSBOARD;

	public Camera ThisCamera;

	[HideInInspector]
	public FogVolumeCamera _FogVolumeCamera;

	private GameObject _FogVolumeCameraGO;

	[SerializeField]
	[Range(0f, 0.01f)]
	public float upsampleDepthThreshold = 0.00187f;

	public bool HDR;

	public bool TAA;

	public FogVolumeTAA _TAA;

	private VelocityBuffer _TAAvelocity;

	[SerializeField]
	public int DepthLayer2;

	private Vector4 TexelSize = Vector4.zero;

	private Material SurrogateMaterial;

	public bool SceneBlur = true;

	private int m_screenWidth;

	private int m_screenHeight;

	public void setDownsample(int val)
	{
		_Downsample = val;
	}

	private void TAASetup()
	{
		if (_Downsample > 1 && TAA)
		{
			if (_FogVolumeCameraGO.GetComponent<FogVolumeTAA>() == null)
			{
				_FogVolumeCameraGO.AddComponent<FogVolumeTAA>();
			}
			_TAA = _FogVolumeCameraGO.GetComponent<FogVolumeTAA>();
			_TAAvelocity = _FogVolumeCameraGO.GetComponent<VelocityBuffer>();
		}
	}

	private void CreateFogCamera()
	{
		if (_Downsample > 1)
		{
			_FogVolumeCameraGO = new GameObject();
			_FogVolumeCameraGO.name = "FogVolumeCamera";
			_FogVolumeCamera = _FogVolumeCameraGO.AddComponent<FogVolumeCamera>();
			_FogVolumeCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
			_FogVolumeCamera.GetComponent<Camera>().backgroundColor = new Color(0f, 0f, 0f, 0f);
			_FogVolumeCameraGO.hideFlags = HideFlags.None;
			_FogVolumeCamera.GetComponent<Camera>().renderingPath = RenderingPath.Forward;
			_FogVolumeCamera.GetComponent<Camera>().allowMSAA = false;
		}
	}

	private void FindFogCamera()
	{
		_FogVolumeCameraGO = GameObject.Find("FogVolumeCamera");
		if (_FogVolumeCameraGO != null)
		{
			_FogVolumeCameraGO.SetActive(true);
		}
		else
		{
			CreateFogCamera();
		}
	}

	private void TexelUpdate()
	{
		if ((bool)_FogVolumeCamera.RT_FogVolume)
		{
			TexelSize.x = 1f / (float)_FogVolumeCamera.RT_FogVolume.width;
			TexelSize.y = 1f / (float)_FogVolumeCamera.RT_FogVolume.height;
			TexelSize.z = _FogVolumeCamera.RT_FogVolume.width;
			TexelSize.w = _FogVolumeCamera.RT_FogVolume.height;
			Shader.SetGlobalVector("RT_FogVolume_TexelSize", TexelSize);
		}
	}

	private void OnEnable()
	{
		ThisCamera = base.gameObject.GetComponent<Camera>();
		FindFogCamera();
		if ((bool)GetComponent<FogVolume>())
		{
			MonoBehaviour.print("Don't add FogVolume here. Create a new one using the menu buttons and follow the instructions");
			Object.DestroyImmediate(GetComponent<FogVolume>());
		}
		if ((bool)ThisCamera.GetComponent<MeshFilter>())
		{
			Object.DestroyImmediate(ThisCamera.GetComponent<MeshFilter>());
		}
		if ((bool)ThisCamera.GetComponent<MeshRenderer>())
		{
			Object.DestroyImmediate(ThisCamera.GetComponent<MeshRenderer>());
		}
		SurrogateMaterial = (Material)Resources.Load("Fog Volume Surrogate");
		UpdateParams();
		if (DepthLayer2 == 0)
		{
			DepthLayer2 = 1;
		}
	}

	private void UpdateParams()
	{
		if ((bool)_FogVolumeCamera && _Downsample > 1)
		{
			_FogVolumeCamera.useBilateralUpsampling = BilateralUpsampling;
			if (BilateralUpsampling && GenerateDepth)
			{
				_FogVolumeCamera.upsampleMode = USMode;
				_FogVolumeCamera.showBilateralEdge = ShowBilateralEdge;
				_FogVolumeCamera.upsampleDepthThreshold = upsampleDepthThreshold;
			}
			if (GenerateDepth)
			{
				SurrogateMaterial.SetInt("_ztest", 8);
				DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolume"));
				DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
				DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
				DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
				DepthLayer2 &= ~(1 << LayerMask.NameToLayer("UI"));
				_FogVolumeCamera.DepthMask = DepthLayer2;
			}
			else
			{
				SurrogateMaterial.SetInt("_ztest", 4);
			}
			if (!_TAA)
			{
				TAASetup();
			}
			if ((bool)_TAA && _TAA.enabled != TAA)
			{
				_TAA.enabled = TAA;
				_TAAvelocity.enabled = TAA;
			}
			HDR = ThisCamera.allowHDR;
		}
	}

	private void OnPreRender()
	{
		int pixelLightCount = QualitySettings.pixelLightCount;
		ShadowQuality shadows = QualitySettings.shadows;
		QualitySettings.pixelLightCount = 0;
		QualitySettings.shadows = ShadowQuality.Disable;
		if (_Downsample > 1 && (bool)_FogVolumeCamera)
		{
			SurrogateMaterial.SetInt("_SrcBlend", (int)_BlendMode);
			Shader.EnableKeyword("_FOG_LOWRES_RENDERER");
			_FogVolumeCamera.Render();
			Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		}
		else
		{
			Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		}
		QualitySettings.pixelLightCount = pixelLightCount;
		QualitySettings.shadows = shadows;
	}

	private void Update()
	{
		if (_FogVolumeCamera == null && _Downsample > 1)
		{
			FindFogCamera();
		}
		if (_Downsample > 1 && (bool)_FogVolumeCameraGO && base.isActiveAndEnabled)
		{
			ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeSurrogate");
		}
		else
		{
			ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");
			ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			if (Screen.width != m_screenWidth || Screen.height != m_screenHeight)
			{
				m_screenWidth = Screen.width;
				m_screenHeight = Screen.height;
			}
		}
		if ((bool)_FogVolumeCameraGO)
		{
			_FogVolumeCamera._Downsample = _Downsample;
		}
		else
		{
			CreateFogCamera();
		}
	}

	private void DestroyFogCamera()
	{
		if ((bool)_FogVolumeCameraGO)
		{
			Object.DestroyImmediate(_FogVolumeCameraGO);
		}
	}

	private void OnDisable()
	{
		Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		DestroyFogCamera();
		ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");
		ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
		ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
	}
}
