using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FogVolumeDirectionalLight : MonoBehaviour
{
	public enum Resolution
	{
		_256 = 0x100,
		_512 = 0x200,
		_1024 = 0x400,
		_2048 = 0x800,
		_4096 = 0x1000
	}

	public enum Antialiasing
	{
		_1 = 1,
		_2 = 2,
		_4 = 4,
		_8 = 8
	}

	public enum ScaleMode
	{
		VolumeMaxAxis = 0,
		Manual = 1
	}

	public enum UpdateMode
	{
		OnStart = 0,
		Interleaved = 1
	}

	public enum FocusMode
	{
		VolumeCenter = 0,
		GameCameraPosition = 1,
		GameObject = 2
	}

	public FogVolume[] _TargetFogVolumes;

	public Vector2 MiniaturePosition = new Vector2(110f, 320f);

	public FogVolume _ProminentFogVolume;

	public Material FogVolumeMaterial;

	public float _CameraVerticalPosition = 500f;

	private RenderTexture depthRT;

	public Antialiasing _Antialiasing = Antialiasing._1;

	public Resolution Size = Resolution._512;

	public Camera ShadowCamera;

	public float _FogVolumeShadowMapEdgeSoftness = 0.001f;

	public ScaleMode _ScaleMode;

	public LayerMask LayersToRender;

	[HideInInspector]
	public Shader outputDepth;

	[HideInInspector]
	public GameObject GOShadowCamera;

	public bool CameraVisible;

	private Image _CanvasImage;

	public UpdateMode _UpdateMode = UpdateMode.Interleaved;

	public float Scale = 50f;

	[Range(0f, 100f)]
	public int SkipFrames = 2;

	public bool ShowMiniature;

	private GameObject _GO_Canvas;

	private GameObject _GO_Image;

	private Canvas _Canvas;

	public Material DebugViewMaterial;

	private GameObject Quad;

	private Vector3 FocusPosition;

	private FogVolumeData _FogVolumeData;

	private Camera _GameCamera;

	public Transform _GameObjectFocus;

	public FocusMode _FocusMode;

	private Material quadMaterial;

	public Shader quadShader;

	private RenderTextureFormat rt_DepthFormat;

	public Material QuadMaterial
	{
		get
		{
			if (quadMaterial == null)
			{
				CreateMaterial();
			}
			return quadMaterial;
		}
	}

	private void OnEnable()
	{
		_GO_Canvas = GameObject.Find("FogVolume Debug Canvas");
		if (!_GO_Canvas)
		{
			_GO_Canvas = new GameObject("FogVolume Debug Canvas");
		}
		_GO_Image = GameObject.Find("FogVolume Image");
		if (!_GO_Image)
		{
			_GO_Image = new GameObject("FogVolume Image");
			_CanvasImage = _GO_Image.AddComponent<Image>();
			_CanvasImage.material = DebugViewMaterial;
			_CanvasImage.rectTransform.position = new Vector3(MiniaturePosition.x, MiniaturePosition.y, 0f);
			_CanvasImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			_CanvasImage.rectTransform.anchorMax = new Vector2(0f, 0f);
			_CanvasImage.rectTransform.anchorMin = new Vector2(0f, 0f);
			_CanvasImage.rectTransform.localScale = new Vector3(2f, 2f, 2f);
		}
		if (!_CanvasImage)
		{
			_CanvasImage = _GO_Image.GetComponent<Image>();
		}
		_CanvasImage.material = DebugViewMaterial;
		_GO_Image.transform.SetParent(_GO_Canvas.transform);
		_GO_Canvas.AddComponent<CanvasScaler>();
		_GO_Canvas.GetComponent<CanvasScaler>().scaleFactor = 1f;
		_GO_Canvas.GetComponent<CanvasScaler>().referencePixelsPerUnit = 100f;
		_Canvas = _GO_Canvas.GetComponent<Canvas>();
		_GO_Canvas.hideFlags = HideFlags.None;
		_GO_Canvas.layer = LayerMask.NameToLayer("UI");
		_GO_Image.layer = LayerMask.NameToLayer("UI");
		_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		Initialize();
		if (_UpdateMode == UpdateMode.OnStart)
		{
			Render();
		}
	}

	private void CreateMaterial()
	{
		Object.DestroyImmediate(quadMaterial);
		quadShader = Shader.Find("Hidden/DepthMapQuad");
		quadMaterial = new Material(quadShader);
		quadMaterial.name = "Depth camera quad material";
		quadMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	private void Initialize()
	{
		CreateMaterial();
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
		{
			rt_DepthFormat = RenderTextureFormat.RGFloat;
		}
		else
		{
			rt_DepthFormat = RenderTextureFormat.DefaultHDR;
		}
		GameObject gameObject = GameObject.Find("Fog Volume Data");
		if ((bool)gameObject)
		{
			_FogVolumeData = gameObject.GetComponent<FogVolumeData>();
			_GameCamera = _FogVolumeData.GameCamera;
			GOShadowCamera = GameObject.Find("FogVolumeShadowCamera");
			if (!GOShadowCamera)
			{
				GOShadowCamera = new GameObject();
				GOShadowCamera.name = "FogVolumeShadowCamera";
			}
			if (!GOShadowCamera)
			{
				MonoBehaviour.print("Shadow camera is lost");
			}
			else
			{
				ShadowCamera = GOShadowCamera.GetComponent<Camera>();
			}
			if (!depthRT)
			{
				depthRT = new RenderTexture((int)Size, (int)Size, 16, rt_DepthFormat);
				depthRT.antiAliasing = (int)_Antialiasing;
				depthRT.filterMode = FilterMode.Bilinear;
				depthRT.name = "FogVolumeShadowMap";
				depthRT.wrapMode = TextureWrapMode.Clamp;
			}
			if (!ShadowCamera)
			{
				ShadowCamera = GOShadowCamera.AddComponent<Camera>();
			}
			else
			{
				ShadowCamera = GOShadowCamera.GetComponent<Camera>();
			}
			ShadowCamera.clearFlags = CameraClearFlags.Color;
			ShadowCamera.backgroundColor = Color.black;
			ShadowCamera.orthographic = true;
			ShadowCamera.farClipPlane = 10000f;
			ShadowCamera.enabled = false;
			ShadowCamera.stereoTargetEye = StereoTargetEyeMask.None;
			ShadowCamera.targetTexture = depthRT;
			ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
			ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			ShadowCamera.transform.parent = base.gameObject.transform;
			Quad = GameObject.Find("Depth map background");
			if (!Quad)
			{
				Quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			}
			Quad.name = "Depth map background";
			Quad.GetComponent<MeshRenderer>().sharedMaterial = QuadMaterial;
			Quad.transform.parent = ShadowCamera.transform;
			Object.DestroyImmediate(Quad.GetComponent<MeshCollider>());
			Quad.hideFlags = HideFlags.None;
		}
	}

	private void EnableVolumetricShadow(bool b)
	{
		if (_TargetFogVolumes == null || _TargetFogVolumes.Length <= 0)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < _TargetFogVolumes.Length; i++)
		{
			FogVolume fogVolume = _TargetFogVolumes[i];
			if (fogVolume != null && fogVolume._FogType == FogVolume.FogType.Textured)
			{
				if (fogVolume.enabled)
				{
					FogVolumeMaterial = fogVolume.FogMaterial;
					FogVolumeMaterial.SetInt("_VolumetricShadowsEnabled", b ? 1 : 0);
				}
				float num3 = _MaxOf(fogVolume.fogVolumeScale.x, fogVolume.fogVolumeScale.y, fogVolume.fogVolumeScale.z);
				if (num3 > num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		_ProminentFogVolume = _TargetFogVolumes[num2];
	}

	private void Update()
	{
		if (_CanvasImage.material != null)
		{
			_CanvasImage.material = DebugViewMaterial;
		}
		if (!ShadowCamera)
		{
			Initialize();
		}
		if (_TargetFogVolumes != null && _AtLeastOneFogVolumeInArray())
		{
			EnableVolumetricShadow(depthRT);
			LayersToRender = (int)LayersToRender & ~(1 << LayerMask.NameToLayer("FogVolume"));
			LayersToRender = (int)LayersToRender & ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
			LayersToRender = (int)LayersToRender & ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			LayersToRender = (int)LayersToRender & ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			ShadowCamera.cullingMask = LayersToRender;
			Refresh();
			if (_ScaleMode == ScaleMode.VolumeMaxAxis)
			{
				if (_ProminentFogVolume != null)
				{
					ShadowCamera.orthographicSize = _MaxOf(_ProminentFogVolume.fogVolumeScale.x, _ProminentFogVolume.fogVolumeScale.y, _ProminentFogVolume.fogVolumeScale.z) * 0.5f;
				}
			}
			else
			{
				ShadowCamera.orthographicSize = Scale;
			}
			if (ShadowCamera.cullingMask != 0 && _ProminentFogVolume != null && _UpdateMode == UpdateMode.Interleaved && ExtensionMethods.TimeSnap(SkipFrames))
			{
				Render();
			}
		}
		else if ((bool)depthRT)
		{
			Object.DestroyImmediate(depthRT);
			Object.DestroyImmediate(GOShadowCamera);
		}
		if (!ShowMiniature && _GO_Canvas.activeInHierarchy)
		{
			_GO_Canvas.SetActive(ShowMiniature);
		}
		if (ShowMiniature && !_GO_Canvas.activeInHierarchy)
		{
			_GO_Canvas.SetActive(ShowMiniature);
		}
	}

	public void Refresh()
	{
		if (_TargetFogVolumes == null)
		{
			_ProminentFogVolume = null;
			return;
		}
		for (int i = 0; i < _TargetFogVolumes.Length; i++)
		{
			FogVolume fogVolume = _TargetFogVolumes[i];
			if (fogVolume != null && fogVolume._FogType == FogVolume.FogType.Textured && fogVolume.HasUpdatedBoxMesh)
			{
				float num = ((!(_ProminentFogVolume != null)) ? 0f : _MaxOf(_ProminentFogVolume.fogVolumeScale.x, _ProminentFogVolume.fogVolumeScale.y, _ProminentFogVolume.fogVolumeScale.z));
				float num2 = _MaxOf(fogVolume.fogVolumeScale.x, fogVolume.fogVolumeScale.y, fogVolume.fogVolumeScale.z);
				if (num2 > num)
				{
					_ProminentFogVolume = fogVolume;
				}
			}
		}
	}

	public void Render()
	{
		if (!depthRT)
		{
			Initialize();
		}
		if (depthRT.height != (int)Size)
		{
			Object.DestroyImmediate(depthRT);
			Initialize();
		}
		if (_Antialiasing != (Antialiasing)depthRT.antiAliasing)
		{
			Object.DestroyImmediate(depthRT);
			Initialize();
		}
		if (!ShadowCamera)
		{
			Initialize();
		}
		switch (_FocusMode)
		{
		case FocusMode.GameCameraPosition:
			FocusPosition = _GameCamera.transform.position;
			break;
		case FocusMode.VolumeCenter:
			if (_ProminentFogVolume != null)
			{
				FocusPosition = _ProminentFogVolume.transform.position;
			}
			else
			{
				FocusPosition = Vector3.zero;
			}
			break;
		case FocusMode.GameObject:
			if ((bool)_GameObjectFocus)
			{
				FocusPosition = _GameObjectFocus.transform.position;
			}
			break;
		}
		Vector3 translation = new Vector3(0f, 0f, FocusPosition.y - _CameraVerticalPosition);
		ShadowCamera.transform.position = FocusPosition;
		ShadowCamera.transform.Translate(translation, Space.Self);
		Vector3 localScale = new Vector3(ShadowCamera.orthographicSize * 2f, ShadowCamera.orthographicSize * 2f, ShadowCamera.orthographicSize * 2f);
		Quad.transform.localScale = localScale;
		Quad.transform.position = ShadowCamera.transform.position;
		Vector3 translation2 = new Vector3(0f, 0f, ShadowCamera.farClipPlane - 50f);
		Quad.transform.Translate(translation2, Space.Self);
		ShadowCamera.transform.rotation = Quaternion.LookRotation(base.transform.forward);
		Shader.SetGlobalVector("_ShadowCameraPosition", ShadowCamera.transform.position);
		Shader.SetGlobalMatrix("_ShadowCameraProjection", ShadowCamera.worldToCameraMatrix);
		Shader.SetGlobalFloat("_ShadowCameraSize", ShadowCamera.orthographicSize);
		Shader.SetGlobalVector("_ShadowLightDir", ShadowCamera.transform.forward);
		quadShader.maximumLOD = 1;
		Shader.SetGlobalFloat("_FogVolumeShadowMapEdgeSoftness", 20f / _FogVolumeShadowMapEdgeSoftness);
		ShadowCamera.RenderWithShader(outputDepth, "RenderType");
		quadShader.maximumLOD = 100;
		Shader.SetGlobalTexture("_ShadowTexture", depthRT);
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(depthRT);
		if ((bool)_GO_Canvas)
		{
			_GO_Canvas.SetActive(false);
		}
		EnableVolumetricShadow(false);
	}

	private void OnDestroy()
	{
		Object.DestroyImmediate(GOShadowCamera);
		Object.DestroyImmediate(_GO_Canvas);
		Object.DestroyImmediate(Quad);
	}

	private bool _AtLeastOneFogVolumeInArray()
	{
		if (_TargetFogVolumes != null)
		{
			for (int i = 0; i < _TargetFogVolumes.Length; i++)
			{
				if (_TargetFogVolumes[i] != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddAllFogVolumesToThisLight()
	{
		_ProminentFogVolume = null;
		FogVolume[] array = Object.FindObjectsOfType<FogVolume>();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i]._FogType == FogVolume.FogType.Textured)
			{
				num++;
			}
		}
		_TargetFogVolumes = new FogVolume[num];
		int num2 = 0;
		for (int j = 0; j < array.Length; j++)
		{
			FogVolume fogVolume = array[j];
			if (fogVolume != null && fogVolume._FogType == FogVolume.FogType.Textured)
			{
				_TargetFogVolumes[num2++] = array[j];
			}
		}
	}

	public void RemoveAllFogVolumesFromThisLight()
	{
		_ProminentFogVolume = null;
		_TargetFogVolumes = null;
	}

	private float _MaxOf(float _a, float _b)
	{
		return (!(_a >= _b)) ? _b : _a;
	}

	private float _MaxOf(float _a, float _b, float _c)
	{
		return _MaxOf(_MaxOf(_a, _b), _c);
	}
}
