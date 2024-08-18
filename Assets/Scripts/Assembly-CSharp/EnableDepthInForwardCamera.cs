using UnityEngine;

[ExecuteInEditMode]
public class EnableDepthInForwardCamera : MonoBehaviour
{
	private void OnEnable()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	private void Update()
	{
		if (GetComponent<Camera>().depthTextureMode != DepthTextureMode.Depth)
		{
			GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		}
	}
}
