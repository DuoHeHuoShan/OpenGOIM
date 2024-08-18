using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.XR;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(FogVolumeRenderManager))]
public class FogVolumeCamera : MonoBehaviour
{
	[Serializable]
	public enum UpsampleMode
	{
		DOWNSAMPLE_MIN = 0,
		DOWNSAMPLE_MAX = 1,
		DOWNSAMPLE_CHESSBOARD = 2
	}

	public enum UpsampleMaterialPass
	{
		DEPTH_DOWNSAMPLE = 0,
		BILATERAL_UPSAMPLE = 1
	}

	[HideInInspector]
	public string FogVolumeResolution;

	public RenderTexture RT_FogVolume;

	public RenderTexture RT_FogVolumeR;

	[HideInInspector]
	public int _Downsample;

	private FogVolumeRenderer _FogVolumeRenderer;

	[HideInInspector]
	public Camera ThisCamera;

	[HideInInspector]
	public Camera SceneCamera;

	public LayerMask DepthMask = 0;

	public RenderTexture RT_Depth;

	public RenderTexture RT_DepthR;

	private Shader depthShader;

	[HideInInspector]
	public float upsampleDepthThreshold = 0.02f;

	private UpsampleMode _upsampleMode;

	public RenderTexture[] lowProfileDepthRT;

	private bool _useBilateralUpsampling;

	private Material bilateralMaterial;

	private bool _showBilateralEdge;

	private FogVolumeRenderManager _CameraRender;

	private RenderTextureFormat rt_DepthFormat;

	private FogVolumeData _FogVolumeData;

	private GameObject _FogVolumeDataGO;

	private Texture2D nullTex;

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

	public UpsampleMode upsampleMode
	{
		get
		{
			return _upsampleMode;
		}
		set
		{
			if (value != _upsampleMode)
			{
				SetUpsampleMode(value);
			}
		}
	}

	public bool useBilateralUpsampling
	{
		get
		{
			return _useBilateralUpsampling;
		}
		set
		{
			if (_useBilateralUpsampling != value)
			{
				SetUseBilateralUpsampling(value);
			}
		}
	}

	public bool showBilateralEdge
	{
		get
		{
			return _showBilateralEdge;
		}
		set
		{
			if (value != _showBilateralEdge)
			{
				ShowBilateralEdge(value);
			}
		}
	}

	private void SetUpsampleMode(UpsampleMode value)
	{
		_upsampleMode = value;
		UpdateBilateralDownsampleModeSwitch();
	}

	public static bool BilateralUpsamplingEnabled()
	{
		return SystemInfo.graphicsShaderLevel >= 40;
	}

	private void ReleaseLowProfileDepthRT()
	{
		if (lowProfileDepthRT != null)
		{
			for (int i = 0; i < lowProfileDepthRT.Length; i++)
			{
				RenderTexture.ReleaseTemporary(lowProfileDepthRT[i]);
			}
			lowProfileDepthRT = null;
		}
	}

	private void SetUseBilateralUpsampling(bool b)
	{
		_useBilateralUpsampling = b;
		if (_useBilateralUpsampling)
		{
			if (bilateralMaterial == null)
			{
				bilateralMaterial = new Material(Shader.Find("Hidden/Upsample"));
				if (bilateralMaterial == null)
				{
					Debug.Log("#ERROR# Hidden/Upsample");
				}
				UpdateBilateralDownsampleModeSwitch();
				ShowBilateralEdge(_showBilateralEdge);
			}
		}
		else
		{
			bilateralMaterial = null;
		}
	}

	private void UpdateBilateralDownsampleModeSwitch()
	{
		if (bilateralMaterial != null)
		{
			switch (_upsampleMode)
			{
			case UpsampleMode.DOWNSAMPLE_MIN:
				bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			case UpsampleMode.DOWNSAMPLE_MAX:
				bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			case UpsampleMode.DOWNSAMPLE_CHESSBOARD:
				bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			}
		}
	}

	public void ShowBilateralEdge(bool b)
	{
		_showBilateralEdge = b;
		if ((bool)bilateralMaterial)
		{
			if (showBilateralEdge)
			{
				bilateralMaterial.EnableKeyword("VISUALIZE_EDGE");
			}
			else
			{
				bilateralMaterial.DisableKeyword("VISUALIZE_EDGE");
			}
		}
	}

	private Texture2D MakeTex(Color col)
	{
		Color[] pixels = new Color[1] { col };
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	private void ShaderLoad()
	{
		depthShader = Shader.Find("Hidden/Fog Volume/Depth");
		if (depthShader == null)
		{
			MonoBehaviour.print("Hidden/Fog Volume/Depth #SHADER ERROR#");
		}
	}

	public RenderTextureReadWrite GetRTReadWrite()
	{
		return (!SceneCamera.allowHDR) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
	}

	public RenderTextureFormat GetRTFormat()
	{
		if (!_FogVolumeRenderer.TAA)
		{
			return (!SceneCamera.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;
		}
		return RenderTextureFormat.ARGBHalf;
	}

	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, GetRTFormat(), GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	private void OnEnable()
	{
		nullTex = MakeTex(Color.black);
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
		{
			rt_DepthFormat = RenderTextureFormat.RFloat;
		}
		else
		{
			rt_DepthFormat = RenderTextureFormat.DefaultHDR;
		}
		_FogVolumeDataGO = GameObject.FindGameObjectWithTag("FogVolumeData");
		if ((bool)_FogVolumeDataGO)
		{
			_FogVolumeData = _FogVolumeDataGO.GetComponent<FogVolumeData>();
		}
		if ((bool)_FogVolumeData)
		{
			SceneCamera = _FogVolumeData.GetFogVolumeCamera;
		}
		if (SceneCamera == null)
		{
			Debug.Log("FogVolumeCamera.cs can't get a valid camera from 'Fog Volume Data'\n Assigning BackgroundCam - M@ Edited this");
			SceneCamera = GameObject.FindGameObjectWithTag("BackgroundCamera").GetComponent<Camera>();
		}
		ShaderLoad();
		SetUpsampleMode(_upsampleMode);
		ShowBilateralEdge(_showBilateralEdge);
		_FogVolumeRenderer = SceneCamera.GetComponent<FogVolumeRenderer>();
		if (_FogVolumeRenderer == null && !_FogVolumeData.ForceNoRenderer)
		{
			FogVolumeRenderer fogVolumeRenderer = SceneCamera.gameObject.AddComponent<FogVolumeRenderer>();
			fogVolumeRenderer.enabled = true;
		}
		ThisCamera = GetComponent<Camera>();
		ThisCamera.enabled = false;
		ThisCamera.clearFlags = CameraClearFlags.Color;
		ThisCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		ThisCamera.farClipPlane = SceneCamera.farClipPlane;
		_CameraRender = GetComponent<FogVolumeRenderManager>();
		if (_CameraRender == null)
		{
			_CameraRender = base.gameObject.AddComponent<FogVolumeRenderManager>();
		}
		_CameraRender.SceneCamera = SceneCamera;
		_CameraRender.SecondaryCamera = ThisCamera;
	}

	private void CameraUpdate()
	{
		if ((bool)SceneCamera)
		{
			ThisCamera.aspect = SceneCamera.aspect;
			ThisCamera.farClipPlane = SceneCamera.farClipPlane;
			ThisCamera.nearClipPlane = SceneCamera.nearClipPlane;
			ThisCamera.allowHDR = SceneCamera.allowHDR;
			ThisCamera.projectionMatrix = SceneCamera.projectionMatrix;
		}
	}

	protected void Get_RT_Depth(ref RenderTexture rt, int2 size, string name)
	{
		ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 16, rt_DepthFormat);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	public void RenderDepth()
	{
		if (_FogVolumeRenderer.GenerateDepth)
		{
			ThisCamera.cullingMask = _FogVolumeRenderer.DepthLayer2;
			if (SceneCamera.stereoEnabled)
			{
				Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
				if (SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || SceneCamera.stereoTargetEye == StereoTargetEyeMask.Left)
				{
					Vector3 camPosition = SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.LeftEye));
					Quaternion rotation = SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix = SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					Get_RT_Depth(ref RT_Depth, new int2(screenX, screenY), "RT_DepthLeft");
					_CameraRender.RenderEye(RT_Depth, camPosition, rotation, stereoProjectionMatrix, depthShader);
					Shader.SetGlobalTexture("RT_Depth", RT_Depth);
				}
				if (SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || SceneCamera.stereoTargetEye == StereoTargetEyeMask.Right)
				{
					Vector3 camPosition2 = SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.RightEye));
					Quaternion rotation2 = SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix2 = SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
					Get_RT_Depth(ref RT_DepthR, new int2(screenX, screenY), "RT_DepthRight");
					_CameraRender.RenderEye(RT_DepthR, camPosition2, rotation2, stereoProjectionMatrix2, depthShader);
					Shader.SetGlobalTexture("RT_DepthR", RT_DepthR);
				}
			}
			else
			{
				Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
				Get_RT_Depth(ref RT_Depth, new int2(screenX, screenY), "RT_Depth");
				_CameraRender.RenderEye(RT_Depth, SceneCamera.transform.position, SceneCamera.transform.rotation, SceneCamera.projectionMatrix, depthShader);
				Shader.SetGlobalTexture("RT_Depth", RT_Depth);
			}
		}
		else
		{
			Shader.SetGlobalTexture("RT_Depth", nullTex);
			Shader.SetGlobalTexture("RT_DepthR", nullTex);
		}
	}

	public void Render()
	{
		CameraUpdate();
		if (_Downsample > 0)
		{
			RenderDepth();
			ThisCamera.cullingMask = 1 << LayerMask.NameToLayer("FogVolume");
			ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			int2 size = new int2(screenX / _Downsample, screenY / _Downsample);
			if (SceneCamera.stereoEnabled)
			{
				Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
				if (SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || SceneCamera.stereoTargetEye == StereoTargetEyeMask.Left)
				{
					Vector3 camPosition = SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.LeftEye));
					Quaternion rotation = SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix = SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					GetRT(ref RT_FogVolume, size, "RT_FogVolumeLeft");
					_CameraRender.RenderEye(RT_FogVolume, camPosition, rotation, stereoProjectionMatrix, null);
				}
				if (SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || SceneCamera.stereoTargetEye == StereoTargetEyeMask.Right)
				{
					Vector3 camPosition2 = SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.RightEye));
					Quaternion rotation2 = SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix2 = SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
					GetRT(ref RT_FogVolumeR, size, "RT_FogVolumeRight");
					_CameraRender.RenderEye(RT_FogVolumeR, camPosition2, rotation2, stereoProjectionMatrix2, null);
				}
			}
			else
			{
				Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
				GetRT(ref RT_FogVolume, size, "RT_FogVolume");
				_CameraRender.RenderEye(RT_FogVolume, SceneCamera.transform.position, SceneCamera.transform.rotation, SceneCamera.projectionMatrix, null);
			}
			if ((bool)RT_FogVolume)
			{
				if (_FogVolumeRenderer.TAA && Application.isPlaying)
				{
					_FogVolumeRenderer._TAA.TAA(ref RT_FogVolume);
				}
				Shader.SetGlobalTexture("RT_FogVolume", RT_FogVolume);
			}
			if ((bool)RT_FogVolumeR)
			{
				if (_FogVolumeRenderer.TAA && Application.isPlaying)
				{
					_FogVolumeRenderer._TAA.TAA(ref RT_FogVolumeR);
				}
				Shader.SetGlobalTexture("RT_FogVolumeR", RT_FogVolumeR);
			}
			if (useBilateralUpsampling && _FogVolumeRenderer.GenerateDepth)
			{
				if ((bool)bilateralMaterial)
				{
					ReleaseLowProfileDepthRT();
					lowProfileDepthRT = new RenderTexture[_Downsample];
					for (int i = 0; i < _Downsample; i++)
					{
						int width = screenX / (i + 1);
						int height = screenY / (i + 1);
						int num = screenX / Mathf.Max(i, 1);
						int num2 = screenY / Mathf.Max(i, 1);
						Vector4 value = new Vector4(1f / (float)num, 1f / (float)num2, 0f, 0f);
						bilateralMaterial.SetFloat("_UpsampleDepthThreshold", upsampleDepthThreshold);
						bilateralMaterial.SetVector("_TexelSize", value);
						bilateralMaterial.SetTexture("_HiResDepthBuffer", RT_Depth);
						lowProfileDepthRT[i] = RenderTexture.GetTemporary(width, height, 0, rt_DepthFormat, GetRTReadWrite());
						lowProfileDepthRT[i].name = "lowProfileDepthRT_" + i;
						Graphics.Blit(null, lowProfileDepthRT[i], bilateralMaterial, 0);
					}
					Shader.SetGlobalTexture("RT_Depth", lowProfileDepthRT[lowProfileDepthRT.Length - 1]);
				}
				if ((bool)bilateralMaterial)
				{
					for (int num3 = _Downsample - 1; num3 >= 0; num3--)
					{
						int width2 = screenX / Mathf.Max(num3, 1);
						int height2 = screenY / Mathf.Max(num3, 1);
						int num4 = screenX / (num3 + 1);
						int num5 = screenY / (num3 + 1);
						Vector4 value2 = new Vector4(1f / (float)num4, 1f / (float)num5, 0f, 0f);
						bilateralMaterial.SetVector("_TexelSize", value2);
						bilateralMaterial.SetVector("_InvdUV", new Vector4(RT_FogVolume.width, RT_FogVolume.height, 0f, 0f));
						bilateralMaterial.SetTexture("_HiResDepthBuffer", RT_Depth);
						bilateralMaterial.SetTexture("_LowResDepthBuffer", lowProfileDepthRT[num3]);
						bilateralMaterial.SetTexture("_LowResColor", RT_FogVolume);
						RenderTexture temporary = RenderTexture.GetTemporary(width2, height2, 0, GetRTFormat(), GetRTReadWrite());
						temporary.filterMode = FilterMode.Bilinear;
						Graphics.Blit(null, temporary, bilateralMaterial, 1);
						RenderTexture rT_FogVolume = RT_FogVolume;
						RT_FogVolume = temporary;
						RenderTexture.ReleaseTemporary(rT_FogVolume);
					}
				}
				ReleaseLowProfileDepthRT();
			}
			Shader.SetGlobalTexture("RT_FogVolume", RT_FogVolume);
		}
		else
		{
			ReleaseRT(RT_FogVolume);
		}
	}

	private void OnDisable()
	{
		if ((bool)ThisCamera)
		{
			ThisCamera.targetTexture = null;
		}
		UnityEngine.Object.DestroyImmediate(RT_FogVolume);
		ReleaseRT(RT_FogVolume);
		UnityEngine.Object.DestroyImmediate(RT_FogVolumeR);
		ReleaseRT(RT_FogVolumeR);
		ReleaseRT(RT_Depth);
		UnityEngine.Object.DestroyImmediate(RT_Depth);
		ReleaseRT(RT_DepthR);
		UnityEngine.Object.DestroyImmediate(RT_DepthR);
	}
}
