using UnityEngine;

public class FogVolumeRenderManager : MonoBehaviour
{
	public Camera SceneCamera;

	public Camera SecondaryCamera;

	public void RenderEye(RenderTexture targetTexture, Vector3 camPosition, Quaternion camRotation, Matrix4x4 camProjectionMatrix, Shader CameraShader)
	{
		SecondaryCamera.transform.position = camPosition;
		SecondaryCamera.transform.rotation = camRotation;
		SecondaryCamera.projectionMatrix = camProjectionMatrix;
		SecondaryCamera.targetTexture = targetTexture;
		if (CameraShader != null)
		{
			SecondaryCamera.RenderWithShader(CameraShader, "RenderType");
		}
		else
		{
			SecondaryCamera.Render();
		}
	}
}
