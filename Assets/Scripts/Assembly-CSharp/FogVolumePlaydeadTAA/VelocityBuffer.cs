using UnityEngine;

namespace FogVolumePlaydeadTAA
{
	public class VelocityBuffer : EffectBase
	{
		public enum NeighborMaxSupport
		{
			TileSize10 = 0,
			TileSize20 = 1,
			TileSize40 = 2
		}

		private const RenderTextureFormat velocityFormat = RenderTextureFormat.RGFloat;

		private Camera _camera;

		private FrustumJitter _frustumJitter;

		public Shader velocityShader;

		private Material velocityMaterial;

		private RenderTexture[] velocityBuffer;

		private RenderTexture[] velocityNeighborMax;

		private bool[] paramInitialized;

		private Vector4[] paramProjectionExtents;

		private Matrix4x4[] paramCurrV;

		private Matrix4x4[] paramCurrVP;

		private Matrix4x4[] paramPrevVP;

		private Matrix4x4[] paramPrevVP_NoFlip;

		private int activeEyeIndex = -1;

		public bool neighborMaxGen;

		public NeighborMaxSupport neighborMaxSupport = NeighborMaxSupport.TileSize20;

		public RenderTexture activeVelocityBuffer
		{
			get
			{
				return (activeEyeIndex == -1) ? null : velocityBuffer[activeEyeIndex];
			}
		}

		public RenderTexture activeVelocityNeighborMax
		{
			get
			{
				return (activeEyeIndex == -1) ? null : velocityNeighborMax[activeEyeIndex];
			}
		}

		private void Reset()
		{
			_camera = GetComponent<Camera>();
			_frustumJitter = GetComponent<FrustumJitter>();
		}

		private void Clear()
		{
			EnsureArray(ref paramInitialized, 2, false);
			paramInitialized[0] = false;
			paramInitialized[1] = false;
		}

		private void OnEnable()
		{
			velocityShader = Shader.Find("Hidden/VelocityBuffer");
		}

		private void Awake()
		{
			Reset();
			Clear();
		}

		private void OnPreRender()
		{
		}

		public void GenerateVelocityBuffer()
		{
			EnsureArray(ref velocityBuffer, 2);
			EnsureArray(ref velocityNeighborMax, 2);
			EnsureArray(ref paramInitialized, 2, false);
			EnsureArray(ref paramProjectionExtents, 2);
			EnsureArray(ref paramCurrV, 2);
			EnsureArray(ref paramCurrVP, 2);
			EnsureArray(ref paramPrevVP, 2);
			EnsureArray(ref paramPrevVP_NoFlip, 2);
			EnsureMaterial(ref velocityMaterial, velocityShader);
			if (velocityMaterial == null)
			{
				return;
			}
			int num = ((_camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) ? 1 : 0);
			int pixelWidth = _camera.pixelWidth;
			int pixelHeight = _camera.pixelHeight;
			if (EnsureRenderTarget(ref velocityBuffer[num], pixelWidth, pixelHeight, RenderTextureFormat.RGFloat, FilterMode.Point, 16))
			{
				Clear();
			}
			EnsureKeyword(velocityMaterial, "CAMERA_PERSPECTIVE", !_camera.orthographic);
			EnsureKeyword(velocityMaterial, "CAMERA_ORTHOGRAPHIC", _camera.orthographic);
			EnsureKeyword(velocityMaterial, "TILESIZE_10", neighborMaxSupport == NeighborMaxSupport.TileSize10);
			EnsureKeyword(velocityMaterial, "TILESIZE_20", neighborMaxSupport == NeighborMaxSupport.TileSize20);
			EnsureKeyword(velocityMaterial, "TILESIZE_40", neighborMaxSupport == NeighborMaxSupport.TileSize40);
			if (_camera.stereoEnabled)
			{
				for (int i = 0; i != 2; i++)
				{
					Camera.StereoscopicEye eye = (Camera.StereoscopicEye)i;
					Matrix4x4 stereoViewMatrix = _camera.GetStereoViewMatrix(eye);
					Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(_camera.GetStereoProjectionMatrix(eye), true);
					Matrix4x4 gPUProjectionMatrix2 = GL.GetGPUProjectionMatrix(_camera.GetStereoProjectionMatrix(eye), false);
					Matrix4x4 matrix4x = ((!paramInitialized[i]) ? stereoViewMatrix : paramCurrV[i]);
					paramInitialized[i] = true;
					paramProjectionExtents[i] = _camera.GetProjectionExtents(eye);
					paramCurrV[i] = stereoViewMatrix;
					paramCurrVP[i] = gPUProjectionMatrix * stereoViewMatrix;
					paramPrevVP[i] = gPUProjectionMatrix * matrix4x;
					paramPrevVP_NoFlip[i] = gPUProjectionMatrix2 * matrix4x;
				}
			}
			else
			{
				Matrix4x4 worldToCameraMatrix = _camera.worldToCameraMatrix;
				Matrix4x4 gPUProjectionMatrix3 = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true);
				Matrix4x4 gPUProjectionMatrix4 = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, false);
				Matrix4x4 matrix4x2 = ((!paramInitialized[0]) ? worldToCameraMatrix : paramCurrV[0]);
				paramInitialized[0] = true;
				paramProjectionExtents[0] = ((!_frustumJitter.enabled) ? _camera.GetProjectionExtents() : _camera.GetProjectionExtents(_frustumJitter.activeSample.x, _frustumJitter.activeSample.y));
				paramCurrV[0] = worldToCameraMatrix;
				paramCurrVP[0] = gPUProjectionMatrix3 * worldToCameraMatrix;
				paramPrevVP[0] = gPUProjectionMatrix3 * matrix4x2;
				paramPrevVP_NoFlip[0] = gPUProjectionMatrix4 * matrix4x2;
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = velocityBuffer[num];
			GL.Clear(true, true, Color.black);
			velocityMaterial.SetVectorArray("_ProjectionExtents", paramProjectionExtents);
			velocityMaterial.SetMatrixArray("_CurrV", paramCurrV);
			velocityMaterial.SetMatrixArray("_CurrVP", paramCurrVP);
			velocityMaterial.SetMatrixArray("_PrevVP", paramPrevVP);
			velocityMaterial.SetMatrixArray("_PrevVP_NoFlip", paramPrevVP_NoFlip);
			velocityMaterial.SetPass(0);
			DrawFullscreenQuad();
			if (neighborMaxGen)
			{
				int num2 = 1;
				switch (neighborMaxSupport)
				{
				case NeighborMaxSupport.TileSize10:
					num2 = 10;
					break;
				case NeighborMaxSupport.TileSize20:
					num2 = 20;
					break;
				case NeighborMaxSupport.TileSize40:
					num2 = 40;
					break;
				}
				int num3 = pixelWidth / num2;
				int num4 = pixelHeight / num2;
				EnsureRenderTarget(ref velocityNeighborMax[num], num3, num4, RenderTextureFormat.RGFloat, FilterMode.Bilinear);
				RenderTexture renderTexture = (RenderTexture.active = RenderTexture.GetTemporary(num3, num4, 0, RenderTextureFormat.RGFloat));
				velocityMaterial.SetTexture("_VelocityTex", velocityBuffer[num]);
				velocityMaterial.SetVector("_VelocityTex_TexelSize", new Vector4(1f / (float)pixelWidth, 1f / (float)pixelHeight, 0f, 0f));
				velocityMaterial.SetPass(3);
				DrawFullscreenQuad();
				RenderTexture.active = velocityNeighborMax[num];
				velocityMaterial.SetTexture("_VelocityTex", renderTexture);
				velocityMaterial.SetVector("_VelocityTex_TexelSize", new Vector4(1f / (float)num3, 1f / (float)num4, 0f, 0f));
				velocityMaterial.SetPass(4);
				DrawFullscreenQuad();
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			else
			{
				ReleaseRenderTarget(ref velocityNeighborMax[0]);
				ReleaseRenderTarget(ref velocityNeighborMax[1]);
			}
			RenderTexture.active = active;
			activeEyeIndex = num;
		}

		private void OnDisable()
		{
			if (velocityBuffer != null)
			{
				ReleaseRenderTarget(ref velocityBuffer[0]);
				ReleaseRenderTarget(ref velocityBuffer[1]);
			}
			if (velocityNeighborMax != null)
			{
				ReleaseRenderTarget(ref velocityNeighborMax[0]);
				ReleaseRenderTarget(ref velocityNeighborMax[1]);
			}
		}
	}
}
