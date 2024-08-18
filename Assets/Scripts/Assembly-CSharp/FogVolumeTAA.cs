using FogVolumePlaydeadTAA;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera), typeof(FrustumJitter), typeof(VelocityBuffer))]
public class FogVolumeTAA : EffectBase
{
	public enum Neighborhood
	{
		MinMax3x3 = 0,
		MinMax3x3Rounded = 1,
		MinMax4TapVarying = 2
	}

	private static RenderBuffer[] mrt = new RenderBuffer[2];

	private Camera _camera;

	private FrustumJitter _frustumJitter;

	private VelocityBuffer _velocityBuffer;

	public Shader reprojectionShader;

	private Material reprojectionMaterial;

	private RenderTexture[,] reprojectionBuffer;

	private int[] reprojectionIndex = new int[2] { -1, -1 };

	public Neighborhood neighborhood;

	public bool unjitterColorSamples;

	public bool unjitterNeighborhood;

	public bool unjitterReprojection;

	public bool useYCoCg;

	public bool useClipping;

	public bool useDilation;

	public bool useMotionBlur;

	public bool useOptimizations = true;

	[Range(0f, 1f)]
	public float feedbackMin = 0.88f;

	[Range(0f, 1f)]
	public float feedbackMax = 0.97f;

	public float motionBlurStrength = 1f;

	public bool motionBlurIgnoreFF;

	private FogVolumeCamera _FogVolumeCamera;

	private void Reset()
	{
		_camera = GetComponent<Camera>();
		_FogVolumeCamera = base.gameObject.GetComponent<FogVolumeCamera>();
		reprojectionShader = Shader.Find("Hidden/TAA");
		_camera = GetComponent<Camera>();
		_frustumJitter = GetComponent<FrustumJitter>();
		_frustumJitter.enabled = false;
		_velocityBuffer = GetComponent<VelocityBuffer>();
		_velocityBuffer.velocityShader = Shader.Find("Hidden/VelocityBuffer");
	}

	private void Clear()
	{
		EnsureArray(ref reprojectionIndex, 2, 0);
		reprojectionIndex[0] = -1;
		reprojectionIndex[1] = -1;
	}

	private void Awake()
	{
		Reset();
		Clear();
	}

	private void Resolve(RenderTexture source, RenderTexture destination)
	{
		_velocityBuffer.GenerateVelocityBuffer();
		EnsureArray(ref reprojectionBuffer, 2, 2);
		EnsureArray(ref reprojectionIndex, 2, -1);
		EnsureMaterial(ref reprojectionMaterial, reprojectionShader);
		if (reprojectionMaterial == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		int num = ((_camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) ? 1 : 0);
		int width = source.width;
		int height = source.height;
		ref RenderTexture rt = ref reprojectionBuffer[num, 0];
		int width2 = width;
		int height2 = height;
		RenderTextureFormat rTFormat = _FogVolumeCamera.GetRTFormat();
		FilterMode filterMode = FilterMode.Bilinear;
		int antiAliasing = source.antiAliasing;
		if (EnsureRenderTarget(ref rt, width2, height2, rTFormat, filterMode, 0, antiAliasing))
		{
			Clear();
		}
		rt = ref reprojectionBuffer[num, 1];
		antiAliasing = width;
		height2 = height;
		rTFormat = _FogVolumeCamera.GetRTFormat();
		filterMode = FilterMode.Bilinear;
		width2 = source.antiAliasing;
		if (EnsureRenderTarget(ref rt, antiAliasing, height2, rTFormat, filterMode, 0, width2))
		{
			Clear();
		}
		bool stereoEnabled = _camera.stereoEnabled;
		bool flag = !stereoEnabled;
		EnsureKeyword(reprojectionMaterial, "CAMERA_PERSPECTIVE", !_camera.orthographic);
		EnsureKeyword(reprojectionMaterial, "CAMERA_ORTHOGRAPHIC", _camera.orthographic);
		EnsureKeyword(reprojectionMaterial, "MINMAX_3X3", neighborhood == Neighborhood.MinMax3x3);
		EnsureKeyword(reprojectionMaterial, "MINMAX_3X3_ROUNDED", neighborhood == Neighborhood.MinMax3x3Rounded);
		EnsureKeyword(reprojectionMaterial, "MINMAX_4TAP_VARYING", neighborhood == Neighborhood.MinMax4TapVarying);
		EnsureKeyword(reprojectionMaterial, "UNJITTER_COLORSAMPLES", unjitterColorSamples);
		EnsureKeyword(reprojectionMaterial, "UNJITTER_NEIGHBORHOOD", unjitterNeighborhood);
		EnsureKeyword(reprojectionMaterial, "UNJITTER_REPROJECTION", unjitterReprojection);
		EnsureKeyword(reprojectionMaterial, "USE_YCOCG", useYCoCg);
		EnsureKeyword(reprojectionMaterial, "USE_CLIPPING", useClipping);
		EnsureKeyword(reprojectionMaterial, "USE_DILATION", useDilation);
		EnsureKeyword(reprojectionMaterial, "USE_MOTION_BLUR", useMotionBlur && flag);
		EnsureKeyword(reprojectionMaterial, "USE_MOTION_BLUR_NEIGHBORMAX", _velocityBuffer.activeVelocityNeighborMax != null);
		EnsureKeyword(reprojectionMaterial, "USE_OPTIMIZATIONS", useOptimizations);
		if (reprojectionIndex[num] == -1)
		{
			reprojectionIndex[num] = 0;
			reprojectionBuffer[num, reprojectionIndex[num]].DiscardContents();
			Graphics.Blit(source, reprojectionBuffer[num, reprojectionIndex[num]]);
		}
		int num2 = reprojectionIndex[num];
		int num3 = (reprojectionIndex[num] + 1) % 2;
		Vector4 activeSample = _frustumJitter.activeSample;
		activeSample.x /= width;
		activeSample.y /= height;
		activeSample.z /= width;
		activeSample.w /= height;
		reprojectionMaterial.SetVector("_JitterUV", activeSample);
		reprojectionMaterial.SetTexture("_VelocityBuffer", _velocityBuffer.activeVelocityBuffer);
		reprojectionMaterial.SetTexture("_VelocityNeighborMax", _velocityBuffer.activeVelocityNeighborMax);
		reprojectionMaterial.SetTexture("_MainTex", source);
		reprojectionMaterial.SetTexture("_PrevTex", reprojectionBuffer[num, num2]);
		reprojectionMaterial.SetFloat("_FeedbackMin", feedbackMin);
		reprojectionMaterial.SetFloat("_FeedbackMax", feedbackMax);
		mrt[0] = reprojectionBuffer[num, num3].colorBuffer;
		mrt[1] = destination.colorBuffer;
		Graphics.SetRenderTarget(mrt, source.depthBuffer);
		reprojectionMaterial.SetPass(0);
		reprojectionBuffer[num, num3].DiscardContents();
		DrawFullscreenQuad();
		reprojectionIndex[num] = num3;
	}

	public void TAA(ref RenderTexture source)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, _FogVolumeCamera.GetRTFormat(), RenderTextureReadWrite.Default, source.antiAliasing);
		Resolve(source, temporary);
		Graphics.Blit(temporary, source);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void OnDisable()
	{
		if (reprojectionBuffer != null)
		{
			ReleaseRenderTarget(ref reprojectionBuffer[0, 0]);
			ReleaseRenderTarget(ref reprojectionBuffer[0, 1]);
			ReleaseRenderTarget(ref reprojectionBuffer[1, 0]);
			ReleaseRenderTarget(ref reprojectionBuffer[1, 1]);
		}
	}
}
