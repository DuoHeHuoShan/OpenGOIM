using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FogVolume : MonoBehaviour
{
	public enum FogType
	{
		Uniform = 0,
		Textured = 1
	}

	public enum SamplingMethod
	{
		Eye2Box = 0,
		ViewAligned = 1
	}

	public enum DebugMode
	{
		Lit = 0,
		Iterations = 1,
		Inscattering = 2,
		VolumetricShadows = 3,
		VolumeFogInscatterClamp = 4,
		VolumeFogPhase = 5
	}

	public enum VortexAxis
	{
		X = 0,
		Y = 1,
		Z = 2
	}

	public enum LightScatterMethod
	{
		BeersLaw = 0,
		InverseSquare = 1,
		Linear = 2
	}

	public FogType _FogType;

	public bool RenderableInSceneView = true;

	private GameObject FogVolumeGameObject;

	public FogVolume ShadowCaster;

	public Texture2D _PerformanceLUT;

	public Vector3 fogVolumeScale = new Vector3(20f, 20f, 20f);

	[SerializeField]
	public Color FogMainColor = Color.white;

	public Color _FogColor = Color.white;

	public bool _AmbientAffectsFogColor;

	[Range(1f, 5f)]
	public float Exposure = 2.5f;

	[Range(-0.5f, 0.5f)]
	public float Offset;

	[Range(0.01f, 3f)]
	public float Gamma = 1f;

	public bool Tonemap;

	public float Visibility = 25f;

	[HideInInspector]
	public bool ColorAdjust;

	[Header("Lighting")]
	public int _SelfShadowSteps = 3;

	public int ShadowCameraSkippedFrames;

	public Color _SelfShadowColor = Color.black;

	public float VolumeFogInscatteringAnisotropy = 0.5f;

	public float VolumeFogInscatteringIntensity = 0.1f;

	public float VolumeFogInscatteringStartDistance = 10f;

	public float VolumeFogInscatteringTransitionWideness = 1f;

	public Color VolumeFogInscatteringColor = Color.white;

	public bool VolumeFogInscattering;

	public float HeightAbsorption;

	public float HeightAbsorptionMin = -1f;

	public float HeightAbsorptionMax = -1f;

	public bool SunAttached;

	[SerializeField]
	public Light Sun;

	[SerializeField]
	public bool _ShadeNoise;

	public bool _DirectionalLighting;

	public float _DirectionalLightingDistance = 0.01f;

	public float DirectLightingShadowDensity = 1f;

	public Color LightExtinctionColor = new Color(27f / 85f, 0.1764706f, 0.101960786f, 0f);

	public int DirectLightingShadowSteps = 1;

	public bool bAbsorption = true;

	public float Absorption = 0.5f;

	public float _LightExposure = 1f;

	public bool Lambert;

	public float LambertianBias = 1f;

	public float NormalDistance = 0.01f;

	public float DirectLightingAmount = 1f;

	public float DirectLightingDistance = 10f;

	[Range(1f, 3f)]
	public float ShadowBrightness = 1f;

	public Color _AmbientColor = Color.gray;

	private bool _ProxyVolume;

	[SerializeField]
	[Range(0f, 0.15f)]
	public float ShadowShift = 0.0094f;

	[Range(0f, 5f)]
	public float PointLightsIntensity = 1f;

	public float PointLightingDistance = 1000f;

	public float PointLightingDistance2Camera = 50f;

	public float PointLightCullSizeMultiplier = 1f;

	public bool PointLightsActive;

	public bool EnableInscattering;

	public Color InscatteringColor = Color.white;

	public Texture2D _LightHaloTexture;

	public Texture2D CoverageTex;

	public bool LightHalo;

	public float _HaloWidth = 0.75f;

	public float _HaloOpticalDispersion = 1f;

	public float _HaloRadius = 1f;

	public float _HaloIntensity = 1f;

	public float _HaloAbsorption = 0.5f;

	[Range(-1f, 1f)]
	public float InscatteringShape;

	public float InscatteringIntensity = 0.2f;

	public float InscatteringStartDistance;

	public float InscatteringTransitionWideness = 1f;

	[Header("Noise")]
	public bool EnableNoise;

	public int Octaves = 1;

	public float _DetailMaskingThreshold = 18f;

	public bool bSphericalFade;

	public float SphericalFadeDistance = 10f;

	public float DetailDistance = 500f;

	public float DetailTiling = 1f;

	public float _DetailRelativeSpeed = 10f;

	public float _BaseRelativeSpeed = 1f;

	public float _NoiseDetailRange = 0.5f;

	public float _Curl = 0.5f;

	public float BaseTiling = 8f;

	public float Coverage = 1.5f;

	public float NoiseDensity = 1f;

	public float _3DNoiseScale = 80f;

	[Range(10f, 300f)]
	public int Iterations = 85;

	public FogVolumeRenderer.BlendMode _BlendMode = FogVolumeRenderer.BlendMode.TraditionalTransparency;

	public float IterationStep = 500f;

	public float _OptimizationFactor;

	public SamplingMethod _SamplingMethod;

	public bool _VisibleByReflectionProbeStatic = true;

	private FogVolumeRenderer _FogVolumeRenderer;

	public GameObject GameCameraGO;

	public DebugMode _DebugMode;

	public bool useHeightGradient;

	public float GradMin = 1f;

	public float GradMax = -1f;

	public float GradMin2 = -1f;

	public float GradMax2 = -1f;

	public Texture3D _NoiseVolume2;

	public Texture3D _NoiseVolume;

	[Range(0f, 10f)]
	public float NoiseIntensity = 0.3f;

	[Range(0f, 5f)]
	public float NoiseContrast = 12f;

	public float FadeDistance = 5000f;

	[Range(0f, 20f)]
	public float Vortex;

	public VortexAxis _VortexAxis = VortexAxis.Z;

	public bool bVolumeFog;

	public bool VolumeFogInscatterColorAffectedWithFogColor = true;

	public LightScatterMethod _LightScatterMethod = LightScatterMethod.InverseSquare;

	[Range(0f, 360f)]
	public float rotation;

	[Range(0f, 10f)]
	public float RotationSpeed;

	public Vector4 Speed = new Vector4(0f, 0f, 0f, 0f);

	public Vector4 Stretch = new Vector4(0f, 0f, 0f, 0f);

	[SerializeField]
	[Header("Collision")]
	public bool SceneCollision = true;

	public bool ShowPrimitives;

	public bool EnableDistanceFields;

	public float _PrimitiveEdgeSoftener = 1f;

	public float _PrimitiveCutout;

	public float Constrain;

	[SerializeField]
	[Range(1f, 200f)]
	public float _SceneIntersectionSoftness = 1f;

	[SerializeField]
	[Range(0f, 0.1f)]
	public float _jitter = 0.0045f;

	public MeshRenderer FogRenderer;

	public Texture2D[] _InspectorBackground;

	public int _InspectorBackgroundIndex;

	public string Description = string.Empty;

	[Header("Gradient")]
	public bool EnableGradient;

	public Texture2D Gradient;

	[SerializeField]
	public bool HideWireframe = true;

	[SerializeField]
	public bool SaveMaterials;

	[SerializeField]
	public bool RequestSavingMaterials;

	[SerializeField]
	public int DrawOrder;

	[SerializeField]
	public bool ShowDebugGizmos;

	private MeshFilter filter;

	private Mesh mesh;

	public RenderTexture RT_Opacity;

	public RenderTexture RT_OpacityBlur;

	public float ShadowCutoff = 1f;

	private Vector3 currentFogVolume = Vector3.one;

	public bool CastShadows;

	public GameObject ShadowCameraGO;

	public int shadowCameraPosition = 20;

	public ShadowCamera _ShadowCamera;

	public float _PushAlpha = 1f;

	[SerializeField]
	private Material fogMaterial;

	public Shader FogVolumeShader;

	[SerializeField]
	private GameObject ShadowProjector;

	private MeshRenderer ShadowProjectorRenderer;

	private MeshFilter ShadowProjectorMeshFilter;

	private Material ShadowProjectorMaterial;

	public Mesh ShadowProjectorMesh;

	public Color ShadowColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

	public CompareFunction _ztest;

	private Plane[] FrustumPlanes;

	private Camera GameCamera;

	private Material SurrogateMaterial;

	private BoxCollider _BoxCollider;

	public FogVolumeData _FogVolumeData;

	private GameObject _FogVolumeDataGO;

	public bool ExcludeFromLowRes;

	public float PointLightCPUMaxDistance = 1f;

	private GameObject PointLightsCameraGO;

	private Camera PointLightsCamera;

	public float PointLightScreenMargin = 1f;

	public bool PointLightsRealTimeUpdate = true;

	public bool PointLightBoxCheck = true;

	public bool PrimitivesRealTimeUpdate = true;

	public bool IsVisible;

	public bool CreateSurrogate = true;

	public bool InjectCustomDepthBuffer;

	public bool UseConvolvedLightshafts;

	private bool m_hasUpdatedBoxMesh;

	private FogVolumeLightManager m_lightManager;

	private FogVolumePrimitiveManager m_primitiveManager;

	public int ShadowCameraPosition
	{
		get
		{
			return shadowCameraPosition;
		}
		set
		{
			if (value != shadowCameraPosition)
			{
				shadowCameraPosition = value;
				_ShadowCamera.CameraTransform();
			}
		}
	}

	public Material FogMaterial
	{
		get
		{
			if (fogMaterial == null)
			{
				CreateMaterial();
			}
			return fogMaterial;
		}
	}

	public bool HasUpdatedBoxMesh
	{
		get
		{
			bool hasUpdatedBoxMesh = m_hasUpdatedBoxMesh;
			m_hasUpdatedBoxMesh = false;
			return hasUpdatedBoxMesh;
		}
	}

	public void setNoiseIntensity(float value)
	{
		NoiseIntensity = value;
	}

	public float GetVisibility()
	{
		return Visibility;
	}

	private void RemoveMaterial()
	{
	}

	private void CreateMaterial()
	{
		if (SaveMaterials)
		{
			FogVolumeShader = Shader.Find("Hidden/FogVolume");
			fogMaterial = new Material(FogVolumeShader);
			return;
		}
		UnityEngine.Object.DestroyImmediate(fogMaterial);
		FogVolumeShader = Shader.Find("Hidden/FogVolume");
		fogMaterial = new Material(FogVolumeShader);
		try
		{
			fogMaterial.name = FogVolumeGameObject.name + " Material";
		}
		catch
		{
			MonoBehaviour.print(base.name);
		}
		fogMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	private void ShadowProjectorLock()
	{
		if ((bool)ShadowProjector)
		{
			ShadowProjector.transform.position = new Vector3(FogVolumeGameObject.transform.position.x, ShadowProjector.transform.position.y, FogVolumeGameObject.transform.position.z);
			Vector3 localScale = new Vector3(fogVolumeScale.x, ShadowProjector.transform.localScale.y, fogVolumeScale.z);
			ShadowProjector.transform.localScale = localScale;
			ShadowProjector.transform.localRotation = default(Quaternion);
		}
	}

	private void ShadowMapSetup()
	{
		if ((bool)ShadowCameraGO)
		{
			ShadowCameraGO.GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			ShadowCameraGO.GetComponent<Camera>().renderingPath = RenderingPath.Forward;
		}
		if (CastShadows)
		{
			fogVolumeScale.z = fogVolumeScale.x;
		}
		_ShadowCamera.CameraTransform();
		if (CastShadows)
		{
			ShadowProjector = GameObject.Find(FogVolumeGameObject.name + " Shadow Projector");
			if (ShadowProjector == null)
			{
				ShadowProjector = new GameObject();
				ShadowProjector.AddComponent<MeshFilter>();
				ShadowProjector.AddComponent<MeshRenderer>();
				ShadowProjector.transform.parent = FogVolumeGameObject.transform;
				ShadowProjector.transform.position = FogVolumeGameObject.transform.position;
				ShadowProjector.transform.rotation = FogVolumeGameObject.transform.rotation;
				ShadowProjector.transform.localScale = fogVolumeScale;
				ShadowProjector.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
				ShadowProjector.GetComponent<MeshRenderer>().receiveShadows = false;
				ShadowProjector.GetComponent<MeshRenderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
				ShadowProjector.GetComponent<MeshRenderer>().lightProbeUsage = LightProbeUsage.Off;
			}
			ShadowProjectorMeshFilter = ShadowProjector.GetComponent<MeshFilter>();
			ShadowProjectorMeshFilter.mesh = ShadowProjectorMesh;
			if (ShadowProjectorMesh == null)
			{
				GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
				ShadowProjectorMesh = gameObject.GetComponent<MeshFilter>().mesh;
				UnityEngine.Object.DestroyImmediate(gameObject, true);
			}
			if (ShadowProjector.GetComponent<MeshFilter>().sharedMesh == null)
			{
				ShadowProjector.GetComponent<MeshFilter>().mesh = ShadowProjectorMesh;
			}
			if (ShadowProjectorMesh == null)
			{
				MonoBehaviour.print("Missing mesh");
			}
			ShadowProjectorRenderer = ShadowProjector.GetComponent<MeshRenderer>();
			ShadowProjectorRenderer.lightProbeUsage = LightProbeUsage.Off;
			ShadowProjectorRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			ShadowProjectorRenderer.shadowCastingMode = ShadowCastingMode.Off;
			ShadowProjectorRenderer.receiveShadows = false;
			ShadowProjectorMaterial = ShadowProjectorRenderer.sharedMaterial;
			ShadowProjector.name = FogVolumeGameObject.name + " Shadow Projector";
			if (ShadowProjectorMaterial == null)
			{
				ShadowProjectorMaterial = new Material(Shader.Find("Fog Volume/Shadow Projector"));
				ShadowProjectorMaterial.name = "Shadow Projector Material";
			}
			ShadowProjectorRenderer.sharedMaterial = ShadowProjectorMaterial;
		}
	}

	private void SetShadowProyectorLayer()
	{
		if (!ShadowProjector)
		{
			return;
		}
		if (!RenderableInSceneView)
		{
			if (ShadowProjector.layer == LayerMask.NameToLayer("Default"))
			{
				ShadowProjector.layer = LayerMask.NameToLayer("UI");
			}
		}
		else if (ShadowProjector.layer == LayerMask.NameToLayer("UI"))
		{
			ShadowProjector.layer = LayerMask.NameToLayer("Default");
		}
	}

	private void FindDirectionalLight()
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
		if (!Sun)
		{
			Sun = RenderSettings.sun;
		}
		if (!Sun)
		{
			Light[] array2 = array;
			foreach (Light light in array2)
			{
				if (light.type == LightType.Directional)
				{
					Sun = light;
					break;
				}
			}
		}
		if (Sun == null)
		{
			Debug.LogError("Fog Volume: No valid light found\nDirectional light is required. Light component can be disabled.");
			return;
		}
		Sun.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
		Sun.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
		Sun.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
	}

	public void FindFogVolumeData()
	{
		if (_FogVolumeDataGO == null)
		{
			FogVolumeData[] array = UnityEngine.Object.FindObjectsOfType<FogVolumeData>();
			if (array.Length == 0)
			{
				_FogVolumeDataGO = new GameObject();
				_FogVolumeData = _FogVolumeDataGO.AddComponent<FogVolumeData>();
				_FogVolumeDataGO.name = "Fog Volume Data";
			}
			else
			{
				_FogVolumeDataGO = array[0].gameObject;
				_FogVolumeData = array[0];
			}
		}
		else
		{
			_FogVolumeData = _FogVolumeDataGO.GetComponent<FogVolumeData>();
		}
	}

	private void MoveToLayer()
	{
		if (CastShadows || ExcludeFromLowRes)
		{
			return;
		}
		if (_FogType == FogType.Textured)
		{
			if (FogVolumeGameObject.layer != LayerMask.NameToLayer("FogVolume"))
			{
				FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolume");
			}
		}
		else if (FogVolumeGameObject.layer != LayerMask.NameToLayer("FogVolumeUniform"))
		{
			FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolumeUniform");
		}
	}

	public void AssignCamera()
	{
		FindFogVolumeData();
		if (_FogVolumeDataGO == null)
		{
			return;
		}
		if (_FogVolumeData.GetFogVolumeCamera != null)
		{
			GameCameraGO = _FogVolumeData.GetFogVolumeCamera.gameObject;
		}
		if (GameCameraGO == null)
		{
			base.enabled = false;
			return;
		}
		base.enabled = true;
		GameCamera = GameCameraGO.GetComponent<Camera>();
		_FogVolumeRenderer = GameCameraGO.GetComponent<FogVolumeRenderer>();
		if (_FogVolumeRenderer == null)
		{
			if (!_FogVolumeData.ForceNoRenderer)
			{
				_FogVolumeRenderer = GameCameraGO.AddComponent<FogVolumeRenderer>();
				_FogVolumeRenderer.enabled = true;
			}
		}
		else if (!_FogVolumeData.ForceNoRenderer)
		{
			_FogVolumeRenderer.enabled = true;
		}
	}

	private void SetIcon()
	{
	}

	private void OnEnable()
	{
		m_lightManager = null;
		m_primitiveManager = null;
		SetIcon();
		AssignCamera();
		SurrogateMaterial = Resources.Load("Fog Volume Surrogate", typeof(Material)) as Material;
		FindDirectionalLight();
		_BoxCollider = GetComponent<BoxCollider>();
		if (_BoxCollider == null)
		{
			_BoxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		_BoxCollider.hideFlags = HideFlags.HideAndDontSave;
		_BoxCollider.isTrigger = true;
		FogVolumeGameObject = base.gameObject;
		FogRenderer = FogVolumeGameObject.GetComponent<MeshRenderer>();
		if (FogRenderer == null)
		{
			FogRenderer = FogVolumeGameObject.AddComponent<MeshRenderer>();
		}
		FogRenderer.sharedMaterial = FogMaterial;
		ToggleKeyword();
		ShadowCameraGO = new GameObject();
		ShadowCameraGO.transform.parent = FogVolumeGameObject.transform;
		ShadowCameraGO.AddComponent<Camera>();
		Camera component = ShadowCameraGO.GetComponent<Camera>();
		component.orthographic = true;
		component.clearFlags = CameraClearFlags.Color;
		component.backgroundColor = new Color(0f, 0f, 0f, 0f);
		_ShadowCamera = ShadowCameraGO.AddComponent<ShadowCamera>();
		ShadowCameraGO.name = FogVolumeGameObject.name + " Shadow Camera";
		ShadowCameraGO.hideFlags = HideFlags.HideAndDontSave;
		ShadowMapSetup();
		if (filter == null)
		{
			filter = base.gameObject.GetComponent<MeshFilter>();
			CreateBoxMesh(base.transform.localScale);
			base.transform.localScale = Vector3.one;
		}
		filter.hideFlags = HideFlags.HideInInspector;
		UpdateBoxMesh();
		_PerformanceLUT = Resources.Load("PerformanceLUT") as Texture2D;
		GetShadowMap();
		_FogVolumeData.FindFogVolumes();
		if (FrustumPlanes == null)
		{
			FrustumPlanes = new Plane[6];
		}
		_InitializeLightManagerIfNeccessary();
		_InitializePrimitiveManagerIfNeccessary();
	}

	private float GetPointLightDistance2Camera(Vector3 lightPosition)
	{
		PointLightsCameraGO = Camera.current.gameObject;
		return (lightPosition - PointLightsCameraGO.transform.position).magnitude;
	}

	private bool PointIsVisible(Vector3 point)
	{
		PointLightsCamera = PointLightsCameraGO.GetComponent<Camera>();
		float num = 0f - PointLightScreenMargin;
		float num2 = 1f + PointLightScreenMargin;
		bool flag = true;
		Vector3 vector = PointLightsCamera.WorldToViewportPoint(point);
		if (vector.z > num && vector.x > num && vector.x < num2 && vector.y > num && vector.y < num2)
		{
			return true;
		}
		return false;
	}

	public bool PointIsInsideVolume(Vector3 PointPosition)
	{
		bool result = false;
		float num = base.gameObject.transform.position.x + fogVolumeScale.x / 2f;
		float num2 = base.gameObject.transform.position.x - fogVolumeScale.x / 2f;
		float num3 = base.gameObject.transform.position.y + fogVolumeScale.y / 2f;
		float num4 = base.gameObject.transform.position.y - fogVolumeScale.y / 2f;
		float num5 = base.gameObject.transform.position.z + fogVolumeScale.z / 2f;
		float num6 = base.gameObject.transform.position.z - fogVolumeScale.z / 2f;
		if (num > PointPosition.x && num2 < PointPosition.x && num3 > PointPosition.y && num4 < PointPosition.y && num5 > PointPosition.z && num6 < PointPosition.z)
		{
			result = true;
		}
		return result;
	}

	private Vector3 LocalDirectionalSunLight(Light Sun)
	{
		return base.transform.InverseTransformVector(-Sun.transform.forward);
	}

	private void OnDisable()
	{
		m_lightManager = null;
		m_primitiveManager = null;
	}

	public static void Wireframe(GameObject obj, bool Enable)
	{
	}

	private void OnBecameVisible()
	{
	}

	private void GetShadowMap()
	{
		if (!FogRenderer)
		{
			FogRenderer = FogVolumeGameObject.GetComponent<MeshRenderer>();
		}
		if (!IsVisible)
		{
			return;
		}
		if (FogRenderer != null && FogRenderer.shadowCastingMode == ShadowCastingMode.Off)
		{
			CastShadows = false;
		}
		if (FogRenderer.shadowCastingMode == ShadowCastingMode.On)
		{
			if (_FogType == FogType.Textured)
			{
				CastShadows = true;
			}
			else
			{
				FogRenderer.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
		if (CastShadows && _ShadowCamera == null)
		{
			ShadowMapSetup();
			RT_OpacityBlur = _ShadowCamera.GetOpacityBlurRT();
		}
		if ((bool)_ShadowCamera)
		{
			_ShadowCamera.enabled = CastShadows;
		}
		if (CastShadows)
		{
			RT_Opacity = _ShadowCamera.GetOpacityRT();
		}
		else if ((bool)ShadowCaster && FogRenderer.receiveShadows)
		{
			RT_Opacity = ShadowCaster.RT_Opacity;
			RT_OpacityBlur = ShadowCaster.RT_OpacityBlur;
			fogVolumeScale.x = ShadowCaster.fogVolumeScale.x;
			fogVolumeScale.z = fogVolumeScale.x;
		}
		if (CastShadows)
		{
			FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolumeShadowCaster");
		}
		if (CastShadows && (bool)ShadowCaster)
		{
			FogMaterial.DisableKeyword("VOLUME_SHADOWS");
		}
		if (((bool)ShadowCaster && ShadowCaster.CastShadows) || FogRenderer.receiveShadows)
		{
			FogMaterial.EnableKeyword("VOLUME_SHADOWS");
		}
		if (((bool)ShadowCaster && !ShadowCaster.CastShadows) || !FogRenderer.receiveShadows)
		{
			FogMaterial.DisableKeyword("VOLUME_SHADOWS");
		}
	}

	private void Update()
	{
		if (InjectCustomDepthBuffer && !FogMaterial.IsKeywordEnabled("ExternalDepth") && ExcludeFromLowRes)
		{
			FogMaterial.EnableKeyword("ExternalDepth");
		}
		if (FogMaterial.IsKeywordEnabled("ExternalDepth") && ExcludeFromLowRes && !InjectCustomDepthBuffer)
		{
			FogMaterial.DisableKeyword("ExternalDepth");
		}
		if (GameCamera == null)
		{
			AssignCamera();
		}
		if (PointLightCullSizeMultiplier < 1f)
		{
			PointLightCullSizeMultiplier = 1f;
		}
		if (m_lightManager != null)
		{
			m_lightManager.DrawDebugData = ShowDebugGizmos;
			m_lightManager.SetPointLightCullSizeMultiplier(PointLightCullSizeMultiplier);
		}
		if (m_primitiveManager != null)
		{
			m_primitiveManager.SetVisibility(ShowPrimitives);
		}
		if (GameCamera != null)
		{
			FrustumPlanes = GeometryUtility.CalculateFrustumPlanes(GameCamera);
			if (Application.isPlaying)
			{
				if (GeometryUtility.TestPlanesAABB(FrustumPlanes, _BoxCollider.bounds))
				{
					IsVisible = true;
				}
				else
				{
					IsVisible = false;
				}
			}
			else
			{
				IsVisible = true;
			}
		}
		if (EnableNoise || EnableGradient)
		{
			_FogType = FogType.Textured;
		}
		else
		{
			_FogType = FogType.Uniform;
		}
		RenderSurrogate();
		if (ShadowProjector != null && CastShadows)
		{
			ShadowProjectorMaterial.SetColor("_ShadowColor", ShadowColor);
		}
		if ((FogMaterial.GetTexture("_NoiseVolume") == null && _NoiseVolume != null) || FogMaterial.GetTexture("_NoiseVolume") != _NoiseVolume)
		{
			FogMaterial.SetTexture("_NoiseVolume", _NoiseVolume);
		}
		if ((FogMaterial.GetTexture("_NoiseVolume2") == null && _NoiseVolume2 != null) || FogMaterial.GetTexture("_NoiseVolume2") != _NoiseVolume2)
		{
			FogMaterial.SetTexture("_NoiseVolume2", _NoiseVolume2);
		}
		if ((FogMaterial.GetTexture("CoverageTex") == null && CoverageTex != null) || FogMaterial.GetTexture("CoverageTex") != CoverageTex)
		{
			FogMaterial.SetTexture("CoverageTex", CoverageTex);
		}
	}

	public void RenderSurrogate()
	{
		if (IsVisible)
		{
			if (GameCameraGO == null)
			{
				AssignCamera();
			}
			FogVolumeRenderer fogVolumeRenderer = GameCameraGO.GetComponent<FogVolumeRenderer>();
			if (fogVolumeRenderer == null && !_FogVolumeData.ForceNoRenderer)
			{
				fogVolumeRenderer = GameCameraGO.AddComponent<FogVolumeRenderer>();
				fogVolumeRenderer.enabled = true;
			}
			if (!_FogVolumeData.ForceNoRenderer && fogVolumeRenderer._Downsample > 0 && CreateSurrogate && mesh != null && _FogType == FogType.Textured)
			{
				Graphics.DrawMesh(mesh, base.transform.position, base.transform.rotation, SurrogateMaterial, LayerMask.NameToLayer("FogVolumeSurrogate"), null, 0, null, false, false, false);
			}
		}
	}

	private void UpdateParams()
	{
		if (!IsVisible)
		{
			return;
		}
		if ((bool)_PerformanceLUT && _DebugMode == DebugMode.Iterations)
		{
			FogMaterial.SetTexture("_PerformanceLUT", _PerformanceLUT);
		}
		if (_DebugMode != 0)
		{
			FogMaterial.SetInt("_DebugMode", (int)_DebugMode);
		}
		if (_FogType == FogType.Textured)
		{
			FogMaterial.SetInt("_SrcBlend", (int)_BlendMode);
		}
		else
		{
			FogMaterial.SetInt("_SrcBlend", 5);
		}
		FogMaterial.SetInt("_ztest", (int)_ztest);
		if (m_lightManager != null && m_lightManager.CurrentLightCount > 0 && PointLightsActive && SystemInfo.graphicsShaderLevel > 30)
		{
			FogMaterial.SetVectorArray("_LightPositions", m_lightManager.GetLightPositionArray());
			FogMaterial.SetVectorArray("_LightRotations", m_lightManager.GetLightRotationArray());
			FogMaterial.SetColorArray("_LightColors", m_lightManager.GetLightColorArray());
			FogMaterial.SetVectorArray("_LightData", m_lightManager.GetLightData());
			FogMaterial.SetFloat("PointLightingDistance", 1f / PointLightingDistance);
			FogMaterial.SetFloat("PointLightingDistance2Camera", 1f / PointLightingDistance2Camera);
		}
		if (m_primitiveManager != null && m_primitiveManager.CurrentPrimitiveCount > 0 && EnableDistanceFields)
		{
			FogMaterial.SetFloat("Constrain", Constrain);
			FogMaterial.SetVectorArray("_PrimitivePosition", m_primitiveManager.GetPrimitivePositionArray());
			FogMaterial.SetVectorArray("_PrimitiveScale", m_primitiveManager.GetPrimitiveScaleArray());
			FogMaterial.SetInt("_PrimitiveCount", m_primitiveManager.VisiblePrimitiveCount);
			FogMaterial.SetMatrixArray("_PrimitivesTransform", m_primitiveManager.GetPrimitiveTransformArray());
			FogMaterial.SetFloat("_PrimitiveEdgeSoftener", 1f / _PrimitiveEdgeSoftener);
			FogMaterial.SetFloat("_PrimitiveCutout", _PrimitiveCutout);
			FogMaterial.SetVectorArray("_PrimitiveData", m_primitiveManager.GetPrimitiveDataArray());
		}
		if ((bool)Sun && Sun.enabled)
		{
			FogMaterial.SetFloat("_LightExposure", _LightExposure);
			if (LightHalo && (bool)_LightHaloTexture)
			{
				FogMaterial.SetTexture("_LightHaloTexture", _LightHaloTexture);
				FogMaterial.SetFloat("_HaloOpticalDispersion", _HaloOpticalDispersion);
				FogMaterial.SetFloat("_HaloWidth", _HaloWidth.Remap(0f, 1f, 10f, 1f));
				FogMaterial.SetFloat("_HaloIntensity", _HaloIntensity * 2000f);
				FogMaterial.SetFloat("_HaloRadius", _HaloRadius.Remap(0f, 1f, 2f, 0.1f));
				FogMaterial.SetFloat("_HaloAbsorption", _HaloAbsorption.Remap(0f, 1f, 0f, 16f));
			}
			FogMaterial.SetVector("L", -Sun.transform.forward);
			Shader.SetGlobalVector("L", -Sun.transform.forward);
			FogMaterial.SetVector("_LightLocalDirection", LocalDirectionalSunLight(Sun));
			if (EnableInscattering)
			{
				FogMaterial.SetFloat("_InscatteringIntensity", InscatteringIntensity * 50f);
				FogMaterial.SetFloat("InscatteringShape", InscatteringShape);
				FogMaterial.SetFloat("InscatteringTransitionWideness", InscatteringTransitionWideness);
				FogMaterial.SetColor("_InscatteringColor", InscatteringColor);
			}
			if (VolumeFogInscattering)
			{
				FogMaterial.SetFloat("VolumeFogInscatteringIntensity", VolumeFogInscatteringIntensity * 50f);
				FogMaterial.SetFloat("VolumeFogInscatteringAnisotropy", VolumeFogInscatteringAnisotropy);
				FogMaterial.SetColor("VolumeFogInscatteringColor", VolumeFogInscatteringColor);
				VolumeFogInscatteringStartDistance = Mathf.Max(0f, VolumeFogInscatteringStartDistance);
				FogMaterial.SetFloat("VolumeFogInscatteringStartDistance", VolumeFogInscatteringStartDistance);
				VolumeFogInscatteringTransitionWideness = Mathf.Max(0.01f, VolumeFogInscatteringTransitionWideness);
				FogMaterial.SetFloat("VolumeFogInscatteringTransitionWideness", VolumeFogInscatteringTransitionWideness);
			}
			FogMaterial.SetColor("_LightColor", Sun.color * Sun.intensity);
			if (EnableNoise && _NoiseVolume != null)
			{
				FogMaterial.SetFloat("_NoiseDetailRange", _NoiseDetailRange * 1f);
				FogMaterial.SetFloat("_Curl", _Curl);
				if (_DirectionalLighting)
				{
					FogMaterial.SetFloat("_DirectionalLightingDistance", _DirectionalLightingDistance);
					FogMaterial.SetInt("DirectLightingShadowSteps", DirectLightingShadowSteps);
					FogMaterial.SetFloat("DirectLightingShadowDensity", DirectLightingShadowDensity);
				}
			}
		}
		FogMaterial.SetFloat("_Cutoff", ShadowCutoff);
		FogMaterial.SetFloat("Absorption", Absorption);
		LightExtinctionColor.r = Mathf.Max(0.1f, LightExtinctionColor.r);
		LightExtinctionColor.g = Mathf.Max(0.1f, LightExtinctionColor.g);
		LightExtinctionColor.b = Mathf.Max(0.1f, LightExtinctionColor.b);
		FogMaterial.SetColor("LightExtinctionColor", LightExtinctionColor);
		if (Vortex > 0f)
		{
			FogMaterial.SetFloat("_Vortex", Vortex);
			FogMaterial.SetFloat("_Rotation", (float)Math.PI / 180f * rotation);
			FogMaterial.SetFloat("_RotationSpeed", 0f - RotationSpeed);
		}
		DetailDistance = Mathf.Max(1f, DetailDistance);
		FogMaterial.SetFloat("DetailDistance", DetailDistance);
		FogMaterial.SetFloat("Octaves", Octaves);
		FogMaterial.SetFloat("_DetailMaskingThreshold", _DetailMaskingThreshold);
		FogMaterial.SetVector("_VolumePosition", base.gameObject.transform.position);
		FogMaterial.SetFloat("gain", NoiseIntensity);
		FogMaterial.SetFloat("threshold", NoiseContrast * 0.5f - 5f);
		FogMaterial.SetFloat("_3DNoiseScale", _3DNoiseScale * 0.001f);
		FadeDistance = Mathf.Max(1f, FadeDistance);
		FogMaterial.SetFloat("FadeDistance", FadeDistance);
		FogMaterial.SetFloat("STEP_COUNT", Iterations);
		FogMaterial.SetFloat("DetailTiling", DetailTiling);
		FogMaterial.SetFloat("BaseTiling", BaseTiling * 0.1f);
		FogMaterial.SetFloat("Coverage", Coverage);
		FogMaterial.SetFloat("NoiseDensity", NoiseDensity);
		FogMaterial.SetVector("Speed", Speed * 0.1f);
		FogMaterial.SetVector("Stretch", new Vector4(1f, 1f, 1f, 1f) + Stretch * 0.01f);
		if (useHeightGradient)
		{
			FogMaterial.SetVector("_VerticalGradientParams", new Vector4(GradMin, GradMax, GradMin2, GradMax2));
		}
		FogMaterial.SetColor("_AmbientColor", _AmbientColor);
		if (FogRenderer.lightProbeUsage == LightProbeUsage.UseProxyVolume)
		{
			_ProxyVolume = true;
		}
		else
		{
			_ProxyVolume = false;
		}
		FogMaterial.SetFloat("_ProxyVolume", _ProxyVolume ? 1 : 0);
		FogMaterial.SetFloat("ShadowBrightness", ShadowBrightness);
		FogMaterial.SetFloat("_DetailRelativeSpeed", _DetailRelativeSpeed);
		FogMaterial.SetFloat("_BaseRelativeSpeed", _BaseRelativeSpeed);
		if (bSphericalFade)
		{
			SphericalFadeDistance = Mathf.Max(1f, SphericalFadeDistance);
			FogMaterial.SetFloat("SphericalFadeDistance", SphericalFadeDistance);
		}
		FogMaterial.SetVector("VolumeSize", new Vector4(fogVolumeScale.x, fogVolumeScale.y, fogVolumeScale.z, 0f));
		FogMaterial.SetFloat("Exposure", Mathf.Max(0f, Exposure));
		FogMaterial.SetFloat("Offset", Offset);
		FogMaterial.SetFloat("Gamma", Gamma);
		if (Gradient != null)
		{
			FogMaterial.SetTexture("_Gradient", Gradient);
		}
		FogMaterial.SetFloat("InscatteringStartDistance", InscatteringStartDistance);
		Vector3 vector = currentFogVolume;
		FogMaterial.SetVector("_BoxMin", vector * -0.5f);
		FogMaterial.SetVector("_BoxMax", vector * 0.5f);
		FogMaterial.SetColor("_Color", FogMainColor);
		FogMaterial.SetColor("_FogColor", _FogColor);
		FogMaterial.SetInt("_AmbientAffectsFogColor", _AmbientAffectsFogColor ? 1 : 0);
		FogMaterial.SetFloat("_SceneIntersectionSoftness", _SceneIntersectionSoftness);
		FogMaterial.SetFloat("_RayStep", IterationStep * 0.001f);
		FogMaterial.SetFloat("_OptimizationFactor", _OptimizationFactor);
		FogMaterial.SetFloat("_Visibility", ((!bVolumeFog || !EnableNoise || !_NoiseVolume) && !EnableGradient) ? Visibility : (Visibility * 100f));
		FogRenderer.sortingOrder = DrawOrder;
		FogMaterial.SetInt("VolumeFogInscatterColorAffectedWithFogColor", VolumeFogInscatterColorAffectedWithFogColor ? 1 : 0);
		FogMaterial.SetFloat("_FOV", GameCamera.fieldOfView);
		FogMaterial.SetFloat("HeightAbsorption", HeightAbsorption);
		FogMaterial.SetVector("_AmbientHeightAbsorption", new Vector4(HeightAbsorptionMin, HeightAbsorptionMax, HeightAbsorption, 0f));
		FogMaterial.SetFloat("NormalDistance", NormalDistance);
		FogMaterial.SetFloat("DirectLightingAmount", DirectLightingAmount);
		DirectLightingDistance = Mathf.Max(1f, DirectLightingDistance);
		FogMaterial.SetFloat("DirectLightingDistance", DirectLightingDistance);
		FogMaterial.SetFloat("LambertianBias", LambertianBias);
		FogMaterial.SetFloat("_jitter", _jitter);
		FogMaterial.SetInt("SamplingMethod", (int)_SamplingMethod);
		FogMaterial.SetFloat("_PushAlpha", _PushAlpha);
		if (_ShadeNoise && EnableNoise)
		{
			FogMaterial.SetColor("_SelfShadowColor", _SelfShadowColor);
			FogMaterial.SetInt("_SelfShadowSteps", _SelfShadowSteps);
			FogMaterial.SetFloat("ShadowShift", ShadowShift);
		}
	}

	private void LateUpdate()
	{
	}

	private void OnWillRenderObject()
	{
		GetShadowMap();
		if (PointLightsActive && SystemInfo.graphicsShaderLevel > 30)
		{
			_InitializeLightManagerIfNeccessary();
			if (m_lightManager != null)
			{
				if (PointLightsRealTimeUpdate)
				{
					m_lightManager.Deinitialize();
					if (PointLightBoxCheck)
					{
						m_lightManager.FindLightsInFogVolume();
					}
					else
					{
						m_lightManager.FindLightsInScene();
					}
				}
				if (!m_lightManager.AlreadyUsesTransformForPoI)
				{
					m_lightManager.SetPointOfInterest(_FogVolumeData.GameCamera.transform);
				}
				m_lightManager.ManualUpdate(ref FrustumPlanes);
			}
			FogMaterial.SetInt("_LightsCount", GetVisibleLightCount());
		}
		else
		{
			_DeinitializeLightManagerIfNeccessary();
		}
		if (EnableDistanceFields)
		{
			_InitializePrimitiveManagerIfNeccessary();
			if (m_primitiveManager != null)
			{
				if (PrimitivesRealTimeUpdate)
				{
					m_primitiveManager.Deinitialize();
					m_primitiveManager.FindPrimitivesInFogVolume();
				}
				if (!m_primitiveManager.AlreadyUsesTransformForPoI)
				{
					m_primitiveManager.SetPointOfInterest(_FogVolumeData.GameCamera.transform);
				}
				m_primitiveManager.ManualUpdate(ref FrustumPlanes);
			}
			FogMaterial.SetInt("_PrimitivesCount", GetVisiblePrimitiveCount());
		}
		else
		{
			_DeinitializePrimitiveManagerIfNeccessary();
		}
		if (RT_Opacity != null)
		{
			Shader.SetGlobalTexture("RT_Opacity", RT_Opacity);
			if (UseConvolvedLightshafts)
			{
				FogMaterial.SetTexture("LightshaftTex", RT_OpacityBlur);
			}
			else
			{
				FogMaterial.SetTexture("LightshaftTex", RT_Opacity);
			}
		}
		UpdateParams();
		if (!RenderableInSceneView && Camera.current.name == "SceneCamera")
		{
			FogMaterial.SetVector("_BoxMin", new Vector3(0f, 0f, 0f));
			FogMaterial.SetVector("_BoxMax", new Vector3(0f, 0f, 0f));
		}
		else
		{
			FogMaterial.SetVector("_BoxMin", currentFogVolume * -0.5f);
			FogMaterial.SetVector("_BoxMax", currentFogVolume * 0.5f);
		}
	}

	private void ToggleKeyword()
	{
		if (!IsVisible)
		{
			return;
		}
		if (_jitter > 0f)
		{
			FogMaterial.EnableKeyword("JITTER");
		}
		else
		{
			FogMaterial.DisableKeyword("JITTER");
		}
		if (_DebugMode != 0)
		{
			FogMaterial.EnableKeyword("DEBUG");
		}
		else
		{
			FogMaterial.DisableKeyword("DEBUG");
		}
		switch (_SamplingMethod)
		{
		case SamplingMethod.Eye2Box:
			FogMaterial.DisableKeyword("SAMPLING_METHOD_ViewAligned");
			FogMaterial.EnableKeyword("SAMPLING_METHOD_Eye2Box");
			break;
		case SamplingMethod.ViewAligned:
			FogMaterial.EnableKeyword("SAMPLING_METHOD_ViewAligned");
			FogMaterial.DisableKeyword("SAMPLING_METHOD_Eye2Box");
			break;
		}
		if (LightHalo && (bool)_LightHaloTexture)
		{
			FogMaterial.EnableKeyword("HALO");
		}
		else
		{
			FogMaterial.DisableKeyword("HALO");
		}
		if (ShadowCaster != null)
		{
			if (ShadowCaster.SunAttached)
			{
				FogMaterial.EnableKeyword("LIGHT_ATTACHED");
			}
			if (!ShadowCaster.SunAttached)
			{
				FogMaterial.DisableKeyword("LIGHT_ATTACHED");
			}
		}
		if (Vortex > 0f)
		{
			FogMaterial.EnableKeyword("Twirl");
			switch (_VortexAxis)
			{
			case VortexAxis.X:
				FogMaterial.EnableKeyword("Twirl_X");
				FogMaterial.DisableKeyword("Twirl_Y");
				FogMaterial.DisableKeyword("Twirl_Z");
				break;
			case VortexAxis.Y:
				FogMaterial.DisableKeyword("Twirl_X");
				FogMaterial.EnableKeyword("Twirl_Y");
				FogMaterial.DisableKeyword("Twirl_Z");
				break;
			case VortexAxis.Z:
				FogMaterial.DisableKeyword("Twirl_X");
				FogMaterial.DisableKeyword("Twirl_Y");
				FogMaterial.EnableKeyword("Twirl_Z");
				break;
			}
		}
		else
		{
			FogMaterial.DisableKeyword("Twirl_X");
			FogMaterial.DisableKeyword("Twirl_Y");
			FogMaterial.DisableKeyword("Twirl_Z");
		}
		if (Lambert && (bool)Sun && EnableNoise)
		{
			FogMaterial.EnableKeyword("_LAMBERT_SHADING");
		}
		else
		{
			FogMaterial.DisableKeyword("_LAMBERT_SHADING");
		}
		if (PointLightsActive && SystemInfo.graphicsShaderLevel > 30)
		{
			switch (_LightScatterMethod)
			{
			case LightScatterMethod.BeersLaw:
				FogMaterial.EnableKeyword("ATTEN_METHOD_1");
				FogMaterial.DisableKeyword("ATTEN_METHOD_2");
				FogMaterial.DisableKeyword("ATTEN_METHOD_3");
				break;
			case LightScatterMethod.InverseSquare:
				FogMaterial.DisableKeyword("ATTEN_METHOD_1");
				FogMaterial.EnableKeyword("ATTEN_METHOD_2");
				FogMaterial.DisableKeyword("ATTEN_METHOD_3");
				break;
			case LightScatterMethod.Linear:
				FogMaterial.DisableKeyword("ATTEN_METHOD_1");
				FogMaterial.DisableKeyword("ATTEN_METHOD_2");
				FogMaterial.EnableKeyword("ATTEN_METHOD_3");
				break;
			}
		}
		else
		{
			FogMaterial.DisableKeyword("ATTEN_METHOD_1");
			FogMaterial.DisableKeyword("ATTEN_METHOD_2");
			FogMaterial.DisableKeyword("ATTEN_METHOD_3");
		}
		if (EnableNoise && (bool)_NoiseVolume && useHeightGradient)
		{
			FogMaterial.EnableKeyword("HEIGHT_GRAD");
		}
		else
		{
			FogMaterial.DisableKeyword("HEIGHT_GRAD");
		}
		if (ColorAdjust)
		{
			FogMaterial.EnableKeyword("ColorAdjust");
		}
		else
		{
			FogMaterial.DisableKeyword("ColorAdjust");
		}
		if (bVolumeFog)
		{
			FogMaterial.EnableKeyword("VOLUME_FOG");
		}
		else
		{
			FogMaterial.DisableKeyword("VOLUME_FOG");
		}
		if ((bool)FogMaterial)
		{
			if (EnableGradient && Gradient != null)
			{
				FogMaterial.EnableKeyword("_FOG_GRADIENT");
			}
			else
			{
				FogMaterial.DisableKeyword("_FOG_GRADIENT");
			}
			if (EnableNoise && !SystemInfo.supports3DTextures)
			{
				Debug.Log("Noise not supported. SM level found: " + SystemInfo.graphicsShaderLevel / 10);
			}
			if (EnableNoise && SystemInfo.supports3DTextures)
			{
				FogMaterial.EnableKeyword("_FOG_VOLUME_NOISE");
			}
			else
			{
				FogMaterial.DisableKeyword("_FOG_VOLUME_NOISE");
			}
			if (EnableInscattering && (bool)Sun && Sun.enabled && Sun.isActiveAndEnabled)
			{
				FogMaterial.EnableKeyword("_INSCATTERING");
			}
			else
			{
				FogMaterial.DisableKeyword("_INSCATTERING");
			}
			if (VolumeFogInscattering && (bool)Sun && Sun.enabled && bVolumeFog)
			{
				FogMaterial.EnableKeyword("_VOLUME_FOG_INSCATTERING");
			}
			else
			{
				FogMaterial.DisableKeyword("_VOLUME_FOG_INSCATTERING");
			}
			FogMaterial.SetFloat("Collisions", SceneCollision ? 1 : 0);
			if (ShadowShift > 0f && EnableNoise && (bool)Sun && _ShadeNoise)
			{
				FogMaterial.EnableKeyword("_SHADE");
			}
			else
			{
				FogMaterial.DisableKeyword("_SHADE");
			}
			if (Tonemap)
			{
				FogMaterial.EnableKeyword("_TONEMAP");
			}
			else
			{
				FogMaterial.DisableKeyword("_TONEMAP");
			}
			if (bSphericalFade)
			{
				FogMaterial.EnableKeyword("SPHERICAL_FADE");
			}
			else
			{
				FogMaterial.DisableKeyword("SPHERICAL_FADE");
			}
			if (EnableDistanceFields)
			{
				FogMaterial.EnableKeyword("DF");
			}
			else
			{
				FogMaterial.DisableKeyword("DF");
			}
			if (bAbsorption)
			{
				FogMaterial.EnableKeyword("ABSORPTION");
			}
			else
			{
				FogMaterial.DisableKeyword("ABSORPTION");
			}
			if (_DirectionalLighting && EnableNoise && _NoiseVolume != null && _DirectionalLightingDistance > 0f && DirectLightingShadowDensity > 0f)
			{
				FogMaterial.EnableKeyword("DIRECTIONAL_LIGHTING");
			}
			else
			{
				FogMaterial.DisableKeyword("DIRECTIONAL_LIGHTING");
			}
		}
	}

	public void UpdateBoxMesh()
	{
		if (currentFogVolume != fogVolumeScale || filter == null)
		{
			CreateBoxMesh(fogVolumeScale);
			ShadowMapSetup();
			_BoxCollider.size = fogVolumeScale;
			m_hasUpdatedBoxMesh = true;
		}
		base.transform.localScale = Vector3.one;
	}

	private void CreateBoxMesh(Vector3 scale)
	{
		currentFogVolume = scale;
		if (filter == null)
		{
			filter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.name = base.gameObject.name;
			filter.sharedMesh = mesh;
		}
		mesh.Clear();
		float y = scale.y;
		float z = scale.z;
		float x = scale.x;
		Vector3 vector = new Vector3((0f - x) * 0.5f, (0f - y) * 0.5f, z * 0.5f);
		Vector3 vector2 = new Vector3(x * 0.5f, (0f - y) * 0.5f, z * 0.5f);
		Vector3 vector3 = new Vector3(x * 0.5f, (0f - y) * 0.5f, (0f - z) * 0.5f);
		Vector3 vector4 = new Vector3((0f - x) * 0.5f, (0f - y) * 0.5f, (0f - z) * 0.5f);
		Vector3 vector5 = new Vector3((0f - x) * 0.5f, y * 0.5f, z * 0.5f);
		Vector3 vector6 = new Vector3(x * 0.5f, y * 0.5f, z * 0.5f);
		Vector3 vector7 = new Vector3(x * 0.5f, y * 0.5f, (0f - z) * 0.5f);
		Vector3 vector8 = new Vector3((0f - x) * 0.5f, y * 0.5f, (0f - z) * 0.5f);
		Vector3[] vertices = new Vector3[24]
		{
			vector, vector2, vector3, vector4, vector8, vector5, vector, vector4, vector5, vector6,
			vector2, vector, vector7, vector8, vector4, vector3, vector6, vector7, vector3, vector2,
			vector8, vector7, vector6, vector5
		};
		Vector3 up = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 forward = Vector3.forward;
		Vector3 back = Vector3.back;
		Vector3 left = Vector3.left;
		Vector3 right = Vector3.right;
		Vector3[] normals = new Vector3[24]
		{
			down, down, down, down, left, left, left, left, forward, forward,
			forward, forward, back, back, back, back, right, right, right, right,
			up, up, up, up
		};
		Vector2 vector9 = new Vector2(0f, 0f);
		Vector2 vector10 = new Vector2(1f, 0f);
		Vector2 vector11 = new Vector2(0f, 1f);
		Vector2 vector12 = new Vector2(1f, 1f);
		Vector2[] uv = new Vector2[24]
		{
			vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11,
			vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10,
			vector12, vector11, vector9, vector10
		};
		int[] triangles = new int[36]
		{
			3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
			6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
			12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
			23, 21, 20, 23, 22, 21
		};
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.RecalculateBounds();
	}

	private void _InitializeLightManagerIfNeccessary()
	{
		if (m_lightManager == null)
		{
			m_lightManager = GetComponent<FogVolumeLightManager>();
			if (m_lightManager == null)
			{
				m_lightManager = base.gameObject.AddComponent<FogVolumeLightManager>();
			}
			m_lightManager.Initialize();
			if (PointLightBoxCheck)
			{
				m_lightManager.FindLightsInFogVolume();
			}
			else
			{
				m_lightManager.FindLightsInScene();
			}
		}
	}

	private void _DeinitializeLightManagerIfNeccessary()
	{
		if (m_lightManager != null)
		{
			m_lightManager.Deinitialize();
		}
	}

	public int GetVisibleLightCount()
	{
		if (m_lightManager != null)
		{
			return m_lightManager.VisibleLightCount;
		}
		return 0;
	}

	public int GetTotalLightCount()
	{
		if (m_lightManager != null)
		{
			return m_lightManager.CurrentLightCount;
		}
		return 0;
	}

	private void _InitializePrimitiveManagerIfNeccessary()
	{
		if (m_primitiveManager == null)
		{
			m_primitiveManager = GetComponent<FogVolumePrimitiveManager>();
			if (m_primitiveManager == null)
			{
				m_primitiveManager = base.gameObject.AddComponent<FogVolumePrimitiveManager>();
			}
			m_primitiveManager.Initialize();
			m_primitiveManager.FindPrimitivesInFogVolume();
		}
	}

	private void _DeinitializePrimitiveManagerIfNeccessary()
	{
		if (m_primitiveManager != null)
		{
			m_primitiveManager.Deinitialize();
		}
	}

	public int GetVisiblePrimitiveCount()
	{
		if (m_primitiveManager != null)
		{
			return m_primitiveManager.VisiblePrimitiveCount;
		}
		return 0;
	}

	public int GetTotalPrimitiveCount()
	{
		if (m_primitiveManager != null)
		{
			return m_primitiveManager.CurrentPrimitiveCount;
		}
		return 0;
	}
}
