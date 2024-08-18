using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noodle;
using UnityEngine;
using UnityEngine.Rendering;

public class MobileManager : MonoBehaviour
{
	public enum iOSDeviceNames
	{
		iPhoneX = 0,
		iPhone8 = 1,
		iPhone8Plus = 2,
		iPhone7 = 3,
		iPhone7Plus = 4,
		iPhone6s = 5,
		iPhone6sPlus = 6,
		iPhoneSE = 7,
		iPhone6 = 8,
		iPhone6Plus = 9,
		iPhone5s = 10,
		iPadPro12gen2 = 11,
		iPadPro9 = 12,
		iPadPro10 = 13,
		iPadPro12 = 14,
		iPad5thgen = 15,
		iPadAir2 = 16,
		iPadAir = 17,
		iPadMini4 = 18,
		iPadMini3 = 19,
		iPadMini2 = 20,
		iPod6 = 21,
		AndroidHigh = 22,
		AndroidMedium = 23,
		AndroidLow = 24
	}

	public enum AndroidSpecs
	{
		Low = 0,
		Medium = 1,
		High = 2
	}

	[Serializable]
	public class MobileQualitySettings
	{
		public iOSDeviceNames DeviceName;

		public MobileScale Scale;

		public MobileDOF DOF;

		public bool Grain;

		public bool Vignetting;

		public MobileLOD LOD;

		public MobileShadow ShadowQuality;

		public MobileCloudIterations CloudIterations;

		public MobileRayMCloudDownsample CloudDownsample;

		public MobileMaxParticles MaxParticles;
	}

	public enum MobileScale
	{
		PtFifty = 0,
		PtSeventyFive = 1,
		One = 2,
		None = 3
	}

	public enum MobileDOF
	{
		Off = 0,
		Low = 1,
		Medium = 2,
		High = 3
	}

	public enum MobileLOD
	{
		PtThree = 0,
		PtSix = 1,
		PtSeven = 2,
		One = 3,
		OnePtFive = 4
	}

	public enum MobileShadow
	{
		None = 0,
		Low = 1,
		Medium = 2,
		High = 3
	}

	public enum MobileCloudIterations
	{
		Disabled = 0,
		TenTenTen = 1,
		FifteenFifteenFifteen = 2,
		TwentyTwentyFifteen = 3,
		TwentyFiveTwentyFiveTwenty = 4,
		ThirtyThirtyFiveThirty = 5,
		FourtySixtyFourty = 6
	}

	public enum MobileRayMCloudDownsample
	{
		Twelve = 0,
		Ten = 1,
		Eight = 2,
		Six = 3
	}

	public enum MobileMaxParticles
	{
		FiveHundred = 0,
		OneThousand = 1
	}

	public enum DisplayMode
	{
		Beautiful = 0,
		SixtyFrames = 1,
		BatterySaver = 2,
		Streaming = 3
	}

	public static List<List<string>> DeviceIDsbyDeviceNameList;

	public List<string> ScalePtFifty = new List<string>();

	public List<string> ScalePtSeventyFive = new List<string>();

	public List<string> ScaleOne = new List<string>();

	public bool BuildBool;

	public bool BuildBool1;

	public bool BuildBool2;

	public bool BuildBool3;

	public bool BuildBool4;

	public bool BuildBool5;

	public List<MobileQualitySettings> AllMobileQualitySettings = new List<MobileQualitySettings>();

	public GameObject blendProbe;

	public RectTransform rect;

	public GameObject PrettyButton;

	public GameObject SixtyFPSButton;

	public GameObject BatterySaverButton;

	public GameObject StreamingButton;

	private Camera mainCam;

	private Camera bkgCam;

	private SkinnedMeshRenderer player;

	private MeshRenderer pot;

	private SkinnedMeshRenderer hammer;

	private FogVolume Cumulus;

	private FogVolume Stratus;

	private FogVolume Space;

	private MeshRenderer FogVolumeSurrogate;

	private ParticleSystem SpaceParticles;

	private ParticleSystem SnowParticles;

	private ParticleSystem WaterParticles;

	private ParticleSystem DebrisParticles;

	private ParticleSystem SparkParticles;

	private Canvas canvas;

	private MobileQualitySettings qs = new MobileQualitySettings();

	private MobileQualitySettings origSettings = new MobileQualitySettings();

	private FogVolumeRenderer fvr;

	private const float fpsMeasurePeriod = 0.5f;

	private float m_FpsAccumulator;

	private float m_FpsNextPeriod;

	public float m_CurrentFps;

	public int hysteresis;

	public Camera BatCam;

	public Camera TreeCam;

	public Camera UICam;

	private FxPro fxpro;

	public DisplayMode myDisplayMode;

	private ScreenFader sf;

	public GamecenterManager gcMan;

	public bool Ready;

	public GameObject[] Trees = new GameObject[0];

	private Vector3[] TreesOrigPos = new Vector3[0];

	private bool maxedOut;

	private bool minedOut;

	private int minedCount;

	private int maxedCount;

	private int thresholdMax;

	private int thresholdMin;

	private bool switchCloud;

	private DisplayMode lastDisplayMode;

	private AndroidSpecs AndroidSpec
	{
		get
		{
			if (SystemInfo.systemMemorySize > 3100 && NoodleManager.GetAndroidApiVersion() >= 26)
			{
				return AndroidSpecs.High;
			}
			if (SystemInfo.systemMemorySize > 3100)
			{
				return AndroidSpecs.Medium;
			}
			return AndroidSpecs.Low;
		}
	}

	public MobileManager(MobileQualitySettings qs)
	{
		this.qs = qs;
	}

	public MobileManager()
	{
	}

	private IEnumerator Start()
	{
		m_FpsNextPeriod = Time.timeSinceLevelLoad + 0.5f;
		thresholdMax = 8;
		thresholdMin = -30;
		BatCam = GameObject.FindGameObjectWithTag("BatCam").GetComponent<Camera>();
		TreeCam = GameObject.FindGameObjectWithTag("TreeCam").GetComponent<Camera>();
		UICam = GameObject.FindGameObjectWithTag("UICam").GetComponent<Camera>();
		mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		bkgCam = GameObject.FindGameObjectWithTag("BackgroundCamera").GetComponent<Camera>();
		Cumulus = GameObject.FindGameObjectWithTag("Cumulus").GetComponent<FogVolume>();
		Stratus = GameObject.FindGameObjectWithTag("Stratus").GetComponent<FogVolume>();
		Space = GameObject.FindGameObjectWithTag("Space").GetComponent<FogVolume>();
		FogVolumeSurrogate = GameObject.FindGameObjectWithTag("FogVolumeSurrogate").GetComponent<MeshRenderer>();
		DisableClouds();
		sf = mainCam.GetComponent<ScreenFader>();
		canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		player = GameObject.FindGameObjectWithTag("PlayerMesh").GetComponent<SkinnedMeshRenderer>();
		pot = GameObject.FindGameObjectWithTag("PotMesh").GetComponent<MeshRenderer>();
		hammer = GameObject.FindGameObjectWithTag("HammerMesh").GetComponent<SkinnedMeshRenderer>();
		gcMan = GetComponent<GamecenterManager>();
		fxpro = bkgCam.GetComponent<FxPro>();
		base.gameObject.transform.SetParent(canvas.gameObject.transform, false);
		rect = base.gameObject.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		RectTransform rectTransform = rect;
		Vector2 zero = Vector2.zero;
		rect.offsetMax = zero;
		rectTransform.offsetMin = zero;
		qs = getMobileQualitySettings();
		origSettings = qs;
		LODGroup[] array6 = new LODGroup[0];
		LODGroup[] array = UnityEngine.Object.FindObjectsOfType<LODGroup>();
		foreach (LODGroup lODGroup in array)
		{
			if (lODGroup.gameObject.name == "Broadleaf_Desktop")
			{
				lODGroup.ForceLOD(2);
				continue;
			}
			if (lODGroup.gameObject.name == "Deadtree")
			{
				lODGroup.ForceLOD(0);
				continue;
			}
			switch (qs.DOF)
			{
			case MobileDOF.Off:
				lODGroup.ForceLOD(2);
				break;
			case MobileDOF.Low:
				lODGroup.ForceLOD(2);
				break;
			case MobileDOF.Medium:
				lODGroup.ForceLOD(1);
				break;
			case MobileDOF.High:
				lODGroup.ForceLOD(1);
				break;
			}
		}
		fvr = bkgCam.GetComponent<FogVolumeRenderer>();
		switch (qs.CloudDownsample)
		{
		case MobileRayMCloudDownsample.Twelve:
			fvr._Downsample = 12;
			break;
		case MobileRayMCloudDownsample.Ten:
			fvr._Downsample = 10;
			break;
		case MobileRayMCloudDownsample.Eight:
			fvr._Downsample = 8;
			break;
		case MobileRayMCloudDownsample.Six:
			fvr._Downsample = 6;
			break;
		}
		switch (qs.CloudIterations)
		{
		case MobileCloudIterations.Disabled:
			Cumulus.VolumeFogInscattering = false;
			Cumulus._DirectionalLighting = false;
			Stratus.VolumeFogInscattering = false;
			Stratus._DirectionalLighting = false;
			Space.VolumeFogInscattering = false;
			Space._DirectionalLighting = false;
			break;
		case MobileCloudIterations.TenTenTen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 10;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 10;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 10;
			break;
		case MobileCloudIterations.FifteenFifteenFifteen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 15;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 15;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 15;
			break;
		case MobileCloudIterations.TwentyTwentyFifteen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 20;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 20;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 15;
			break;
		case MobileCloudIterations.TwentyFiveTwentyFiveTwenty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 25;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 25;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 20;
			break;
		case MobileCloudIterations.ThirtyThirtyFiveThirty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 30;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 35;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 30;
			break;
		case MobileCloudIterations.FourtySixtyFourty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 40;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 60;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 40;
			break;
		}
		MeshRenderer[] array2 = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		SkinnedMeshRenderer[] array3 = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
		MeshRenderer[] array4 = array2;
		for (int j = 0; j < array4.Length; j++)
		{
			array4[j].reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		SkinnedMeshRenderer[] array5 = array3;
		for (int j = 0; j < array5.Length; j++)
		{
			array5[j].reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		player.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		pot.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		hammer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		switch (qs.Scale)
		{
		case MobileScale.PtFifty:
			fxpro.DOFParams.DOFBlurSize = 0.318f;
			break;
		case MobileScale.PtSeventyFive:
			fxpro.DOFParams.DOFBlurSize = 0.477f;
			break;
		case MobileScale.One:
			fxpro.DOFParams.DOFBlurSize = 0.636f;
			break;
		case MobileScale.None:
			fxpro.DOFParams.DOFBlurSize = 1.272f;
			break;
		}
		fxpro.FilmGrainIntensity = ((!qs.Grain) ? 0f : 0.12f);
		fxpro.VignettingIntensity = ((!qs.Vignetting) ? 0f : 0.671f);
		switch (qs.DOF)
		{
		case MobileDOF.Off:
			fxpro.enabled = false;
			fxpro.DOFEnabled = false;
			break;
		case MobileDOF.Low:
			fxpro.enabled = true;
			fxpro.DOFEnabled = true;
			break;
		case MobileDOF.Medium:
			fxpro.enabled = true;
			fxpro.DOFEnabled = true;
			break;
		case MobileDOF.High:
			fxpro.enabled = true;
			fxpro.DOFEnabled = true;
			break;
		}
		fxpro.Init();
		yield return null;
		TreesOrigPos = new Vector3[Trees.Length];
		for (int k = 0; k < Trees.Length; k++)
		{
			TreesOrigPos[k] = Trees[k].transform.position;
			if (Trees[k].name.Contains("(1)"))
			{
				Trees[k].layer = 0;
				foreach (Transform item in Trees[k].transform)
				{
					item.gameObject.layer = 0;
				}
			}
			Trees[k].transform.position = player.transform.position;
			Trees[k].SetActive(false);
		}
		Trees[0].SetActive(true);
		yield return null;
		for (int i = 1; i < Trees.Length; i++)
		{
			Trees[i - 1].SetActive(false);
			Trees[i].SetActive(true);
			yield return null;
			if (Trees[i].name.Contains("(1)"))
			{
				Trees[i].layer = 20;
				foreach (Transform item2 in Trees[i].transform)
				{
					item2.gameObject.layer = 20;
				}
			}
			Trees[i].transform.position = TreesOrigPos[i];
		}
		Trees[0].transform.position = TreesOrigPos[0];
		yield return null;
		BatCam.clearFlags = CameraClearFlags.Depth;
		TreeCam.clearFlags = CameraClearFlags.Depth;
		UICam.clearFlags = CameraClearFlags.Depth;
		mainCam.clearFlags = CameraClearFlags.Depth;
		yield return null;
		if (qs.CloudIterations != 0)
		{
			ToggleClouds(true);
		}
		yield return null;
		if (PlayerPrefs.HasKey("DisplayMode"))
		{
			switch (PlayerPrefs.GetInt("DisplayMode"))
			{
			case 0:
				BeautifulOn();
				break;
			case 1:
				SixtyFramesOn();
				break;
			case 2:
				BatterySaverOn();
				break;
			case 3:
				Debug.LogWarning("Streaming mode at start");
				BeautifulOn();
				break;
			}
		}
		else
		{
			Debug.Log("[MobileManager] systemMemorySize = " + SystemInfo.systemMemorySize);
			if (AndroidSpec == AndroidSpecs.High)
			{
				BeautifulOn();
			}
			else if (AndroidSpec == AndroidSpecs.Medium)
			{
				SixtyFramesOn();
			}
			else
			{
				BatterySaverOn();
			}
		}
		GameObject[] trees = Trees;
		foreach (GameObject gameObject in trees)
		{
			while (!gameObject.activeInHierarchy)
			{
				gameObject.SetActive(true);
			}
		}
		sf.StartScene();
		Ready = true;
		if (!Application.isEditor)
		{
			gcMan.InitializeGameCenter();
		}
		Debug.unityLogger.logEnabled = false;
	}

	public MobileQualitySettings getMobileQualitySettings()
	{
		MobileQualitySettings mobileQualitySettings = new MobileQualitySettings();
		switch (AndroidSpec)
		{
		case AndroidSpecs.Low:
			return AllMobileQualitySettings[24];
		case AndroidSpecs.Medium:
			return AllMobileQualitySettings[23];
		case AndroidSpecs.High:
			return AllMobileQualitySettings[22];
		default:
			return AllMobileQualitySettings[24];
		}
	}

	public MobileScale getDeviceScale()
	{
		if (qs == null)
		{
			Debug.LogError("MobileManager qs was null in getDeviceScale. Filling defaults.");
			qs = getMobileQualitySettings();
		}
		return qs.Scale;
	}

	public void ToggleClouds(bool enabled)
	{
		if (origSettings.CloudIterations == MobileCloudIterations.Disabled || !enabled)
		{
			DisableClouds();
		}
		else
		{
			StartCoroutine(EnableClouds());
		}
	}

	public IEnumerator EnableClouds()
	{
		FogVolumeSurrogate.enabled = true;
		yield return new WaitForEndOfFrame();
		Cumulus.gameObject.SetActive(true);
		Stratus.gameObject.SetActive(true);
		Space.gameObject.SetActive(true);
		yield return new WaitForEndOfFrame();
		bkgCam.GetComponent<FogVolumeRenderer>().enabled = true;
	}

	private void DisableClouds()
	{
		Cumulus.gameObject.SetActive(false);
		Stratus.gameObject.SetActive(false);
		Space.gameObject.SetActive(false);
		FogVolumeSurrogate.enabled = false;
		bkgCam.GetComponent<FogVolumeRenderer>().enabled = false;
	}

	public void TogglePostProc(bool enabled)
	{
		if (enabled)
		{
			fxpro.enabled = true;
			fxpro.DOFEnabled = origSettings.DOF != MobileDOF.Off;
			fxpro.FilmGrainIntensity = ((!origSettings.Grain) ? 0f : 0.012f);
			fxpro.VignettingIntensity = ((!origSettings.Vignetting) ? 0f : 0.671f);
			fxpro.Init();
		}
		else
		{
			fxpro.enabled = false;
		}
	}

	private void Update()
	{
		m_FpsAccumulator += 1f;
		if (!(Time.timeSinceLevelLoad > m_FpsNextPeriod))
		{
			return;
		}
		m_CurrentFps = m_FpsAccumulator / 0.5f;
		m_FpsAccumulator = 0f;
		m_FpsNextPeriod += 0.5f;
		if (Time.timeSinceLevelLoad < 5f || myDisplayMode != 0 || origSettings == null || origSettings.CloudIterations == MobileCloudIterations.Disabled)
		{
			return;
		}
		if (m_CurrentFps <= 29f && !minedOut)
		{
			if (hysteresis > 0)
			{
				hysteresis = 0;
			}
			hysteresis--;
			if (hysteresis < thresholdMin)
			{
				if (qs.CloudIterations != 0)
				{
					qs.CloudIterations--;
				}
				if (qs.CloudDownsample != 0)
				{
					qs.CloudDownsample--;
				}
				if (qs.CloudIterations == MobileCloudIterations.Disabled && qs.CloudDownsample == MobileRayMCloudDownsample.Twelve)
				{
					minedOut = true;
				}
				RefreshCloudSettings();
				maxedOut = false;
				hysteresis = 0;
			}
		}
		else
		{
			if (!(m_CurrentFps > 29f) || maxedOut)
			{
				return;
			}
			hysteresis++;
			if (hysteresis > thresholdMax && (thresholdMax < 15360 || qs.CloudIterations != 0))
			{
				thresholdMax *= 2;
				if (qs.CloudIterations != origSettings.CloudIterations)
				{
					qs.CloudIterations++;
				}
				if (qs.CloudDownsample != origSettings.CloudDownsample)
				{
					qs.CloudDownsample++;
				}
				if (qs.CloudIterations == origSettings.CloudIterations && qs.CloudDownsample == origSettings.CloudDownsample)
				{
					maxedOut = true;
				}
				RefreshCloudSettings();
				minedOut = false;
				hysteresis = 0;
			}
		}
	}

	public void RefreshCloudSettings()
	{
		switch (qs.CloudIterations)
		{
		case MobileCloudIterations.Disabled:
			Cumulus.VolumeFogInscattering = false;
			Cumulus._DirectionalLighting = false;
			Stratus.VolumeFogInscattering = false;
			Stratus._DirectionalLighting = false;
			Space.VolumeFogInscattering = false;
			Space._DirectionalLighting = false;
			break;
		case MobileCloudIterations.TenTenTen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 10;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 10;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 10;
			break;
		case MobileCloudIterations.FifteenFifteenFifteen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 15;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 15;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 15;
			break;
		case MobileCloudIterations.TwentyTwentyFifteen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 20;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 20;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 15;
			break;
		case MobileCloudIterations.TwentyFiveTwentyFiveTwenty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 25;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 25;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 20;
			break;
		case MobileCloudIterations.ThirtyThirtyFiveThirty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 30;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 35;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 30;
			break;
		case MobileCloudIterations.FourtySixtyFourty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 40;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 60;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 40;
			break;
		}
		if (fvr == null)
		{
			fvr = bkgCam.GetComponent<FogVolumeRenderer>();
		}
		switch (qs.CloudDownsample)
		{
		case MobileRayMCloudDownsample.Twelve:
			fvr._Downsample = 12;
			break;
		case MobileRayMCloudDownsample.Ten:
			fvr._Downsample = 10;
			break;
		case MobileRayMCloudDownsample.Eight:
			fvr._Downsample = 8;
			break;
		case MobileRayMCloudDownsample.Six:
			fvr._Downsample = 6;
			break;
		}
		ToggleClouds(true);
	}

	public void BeautifulOn()
	{
		if (myDisplayMode != DisplayMode.Streaming)
		{
			myDisplayMode = DisplayMode.Beautiful;
			ToggleClouds(true);
			TogglePostProc(true);
			Application.targetFrameRate = 60;
			StartCoroutine(PlayerControl.AdjustScreenResolution(1f));
			QualitySettings.SetQualityLevel(4);
			QualitySettings.vSyncCount = 0;
			PlayerPrefs.SetInt("DisplayMode", 0);
			PlayerPrefs.Save();
			PrettyButton.SetActive(true);
			SixtyFPSButton.SetActive(false);
			BatterySaverButton.SetActive(false);
			StreamingButton.SetActive(false);
		}
	}

	public void SixtyFramesOn()
	{
		if (myDisplayMode != DisplayMode.Streaming)
		{
			myDisplayMode = DisplayMode.SixtyFrames;
			ToggleClouds(false);
			TogglePostProc(false);
			Application.targetFrameRate = 60;
			StartCoroutine(PlayerControl.AdjustScreenResolution(1f));
			QualitySettings.SetQualityLevel(2);
			QualitySettings.vSyncCount = 0;
			PlayerPrefs.SetInt("DisplayMode", 1);
			PlayerPrefs.Save();
			PrettyButton.SetActive(false);
			SixtyFPSButton.SetActive(true);
			BatterySaverButton.SetActive(false);
			StreamingButton.SetActive(false);
		}
	}

	public void BatterySaverOn()
	{
		if (myDisplayMode != DisplayMode.Streaming)
		{
			myDisplayMode = DisplayMode.BatterySaver;
			ToggleClouds(false);
			TogglePostProc(false);
			Application.targetFrameRate = 60;
			StartCoroutine(PlayerControl.AdjustScreenResolution(0.5f));
			QualitySettings.SetQualityLevel(0);
			QualitySettings.vSyncCount = 0;
			PlayerPrefs.SetInt("DisplayMode", 2);
			PlayerPrefs.Save();
			PrettyButton.SetActive(false);
			SixtyFPSButton.SetActive(false);
			BatterySaverButton.SetActive(true);
			StreamingButton.SetActive(false);
		}
	}

	public void StreamingOn()
	{
		if (myDisplayMode != DisplayMode.Streaming)
		{
			lastDisplayMode = myDisplayMode;
			myDisplayMode = DisplayMode.Streaming;
			ToggleClouds(false);
			TogglePostProc(false);
			Application.targetFrameRate = 60;
			PlayerPrefs.SetInt("DisplayMode", 3);
			PlayerPrefs.Save();
			PrettyButton.SetActive(false);
			SixtyFPSButton.SetActive(false);
			BatterySaverButton.SetActive(false);
			StreamingButton.SetActive(true);
		}
	}

	public void StreamingOff()
	{
		if (myDisplayMode != DisplayMode.Streaming)
		{
			Debug.LogWarning("Attempting to turn of streaming before turning it on.");
			if (lastDisplayMode == DisplayMode.Streaming)
			{
				myDisplayMode = DisplayMode.Beautiful;
			}
		}
		else
		{
			myDisplayMode = lastDisplayMode;
		}
		switch (myDisplayMode)
		{
		case DisplayMode.Beautiful:
			BeautifulOn();
			break;
		case DisplayMode.SixtyFrames:
			SixtyFramesOn();
			break;
		case DisplayMode.BatterySaver:
			BatterySaverOn();
			break;
		case DisplayMode.Streaming:
			Debug.LogWarning("Streaming Display was last mode while Streaming");
			BeautifulOn();
			break;
		}
	}

	public void SetNewQualitySettings(int SettingsInt)
	{
		MobileQualitySettings mobileQualitySettings = getMobileQualitySettings();
		switch (mobileQualitySettings.DOF)
		{
		case MobileDOF.Off:
			fxpro.DOFEnabled = false;
			break;
		case MobileDOF.Low:
			fxpro.DOFEnabled = true;
			break;
		case MobileDOF.Medium:
			fxpro.DOFEnabled = true;
			break;
		case MobileDOF.High:
			fxpro.DOFEnabled = true;
			break;
		}
		fxpro.FilmGrainIntensity = ((!mobileQualitySettings.Grain) ? 0f : 0.099f);
		fxpro.VignettingIntensity = ((!mobileQualitySettings.Vignetting) ? 0f : 0.671f);
		LODGroup[] array6 = new LODGroup[0];
		LODGroup[] array = UnityEngine.Object.FindObjectsOfType<LODGroup>();
		foreach (LODGroup lODGroup in array)
		{
			if (lODGroup.gameObject.name == "Broadleaf_Desktop")
			{
				lODGroup.ForceLOD(2);
				continue;
			}
			if (lODGroup.gameObject.name == "Deadtree")
			{
				lODGroup.ForceLOD(0);
				continue;
			}
			switch (mobileQualitySettings.DOF)
			{
			case MobileDOF.Off:
				lODGroup.ForceLOD(2);
				break;
			case MobileDOF.Low:
				lODGroup.ForceLOD(2);
				break;
			case MobileDOF.Medium:
				lODGroup.ForceLOD(1);
				break;
			case MobileDOF.High:
				lODGroup.ForceLOD(1);
				break;
			}
		}
		switch (mobileQualitySettings.CloudIterations)
		{
		case MobileCloudIterations.Disabled:
			Cumulus.VolumeFogInscattering = false;
			Cumulus._DirectionalLighting = false;
			Stratus.VolumeFogInscattering = false;
			Stratus._DirectionalLighting = false;
			Space.VolumeFogInscattering = false;
			Space._DirectionalLighting = false;
			break;
		case MobileCloudIterations.TenTenTen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 10;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 10;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 10;
			break;
		case MobileCloudIterations.FifteenFifteenFifteen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 15;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 15;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 15;
			break;
		case MobileCloudIterations.TwentyTwentyFifteen:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 20;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 20;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 15;
			break;
		case MobileCloudIterations.TwentyFiveTwentyFiveTwenty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 25;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 25;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 20;
			break;
		case MobileCloudIterations.ThirtyThirtyFiveThirty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 30;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 35;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 30;
			break;
		case MobileCloudIterations.FourtySixtyFourty:
			Cumulus.VolumeFogInscattering = true;
			Cumulus._DirectionalLighting = true;
			Cumulus.Iterations = 40;
			Stratus.VolumeFogInscattering = true;
			Stratus._DirectionalLighting = true;
			Stratus.Iterations = 60;
			Space.VolumeFogInscattering = true;
			Space._DirectionalLighting = true;
			Space.Iterations = 40;
			break;
		}
		QualitySettings.SetQualityLevel((int)mobileQualitySettings.LOD);
		fvr = bkgCam.GetComponent<FogVolumeRenderer>();
		switch (mobileQualitySettings.CloudDownsample)
		{
		case MobileRayMCloudDownsample.Twelve:
			fvr._Downsample = 12;
			break;
		case MobileRayMCloudDownsample.Ten:
			fvr._Downsample = 10;
			break;
		case MobileRayMCloudDownsample.Eight:
			fvr._Downsample = 8;
			break;
		case MobileRayMCloudDownsample.Six:
			fvr._Downsample = 6;
			break;
		}
		
		MeshRenderer[] array2 = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		SkinnedMeshRenderer[] array3 = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
		MeshRenderer[] array4 = array2;
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		SkinnedMeshRenderer[] array5 = array3;
		for (int i = 0; i < array5.Length; i++)
		{
			array5[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		player.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		pot.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		hammer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		fxpro.Init();
	}

	private void PopulateDeviceScaling()
	{
		for (int i = 0; i < Enum.GetNames(typeof(iOSDeviceNames)).Length; i++)
		{
			switch (AllMobileQualitySettings[i].Scale)
			{
			case MobileScale.PtFifty:
				ScalePtFifty.AddRange(DeviceIDsbyDeviceNameList[i]);
				break;
			case MobileScale.PtSeventyFive:
				ScalePtSeventyFive.AddRange(DeviceIDsbyDeviceNameList[i]);
				break;
			case MobileScale.One:
				ScaleOne.AddRange(DeviceIDsbyDeviceNameList[i]);
				break;
			}
		}
		using (FileStream stream = new FileStream("Assets/Mobile/DeviceScaling1.txt", FileMode.Truncate))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream))
			{
				foreach (string item in ScaleOne)
				{
					streamWriter.Write(item);
					streamWriter.WriteLine();
				}
			}
		}
		using (FileStream stream2 = new FileStream("Assets/Mobile/DeviceScaling75.txt", FileMode.Truncate))
		{
			using (StreamWriter streamWriter2 = new StreamWriter(stream2))
			{
				foreach (string item2 in ScalePtSeventyFive)
				{
					streamWriter2.Write(item2);
					streamWriter2.WriteLine();
				}
			}
		}
		using (FileStream stream3 = new FileStream("Assets/Mobile/DeviceScaling50.txt", FileMode.Truncate))
		{
			using (StreamWriter streamWriter3 = new StreamWriter(stream3))
			{
				foreach (string item3 in ScalePtFifty)
				{
					streamWriter3.Write(item3);
					streamWriter3.WriteLine();
				}
			}
		}
	}

	static MobileManager()
	{
		DeviceIDsbyDeviceNameList = new List<List<string>>
		{
			new List<string> { "iPhone10,3", "iPhone10,6" },
			new List<string> { "iPhone10,1", "iPhone10,4" },
			new List<string> { "iPhone10,2", "iPhone10,5" },
			new List<string> { "iPhone9,1", "iPhone9,3" },
			new List<string> { "iPhone9,2", "iPhone9,4" },
			new List<string> { "iPhone8,1" },
			new List<string> { "iPhone8,2" },
			new List<string> { "iPhone8,4" },
			new List<string> { "iPhone7,2" },
			new List<string> { "iPhone7,1" },
			new List<string> { "iPhone6,1", "iPhone6,2" },
			new List<string> { "iPad7,1", "iPad7,2" },
			new List<string> { "iPad6,3", "iPad6,4" },
			new List<string> { "iPad7,3", "iPad7,4" },
			new List<string> { "iPad6,7", "iPad6,8" },
			new List<string> { "iPad6,11", "iPad6,12" },
			new List<string> { "iPad5,3", "iPad5,4" },
			new List<string> { "iPad4,1", "iPad4,2", "iPad4,3" },
			new List<string> { "iPad5,1", "iPad5,2" },
			new List<string> { "iPad4,7", "iPad4,8", "iPad4,9" },
			new List<string> { "iPad4,4", "iPad4,5", "iPad4,6" },
			new List<string> { "iPod7,1" }
		};
	}
}
