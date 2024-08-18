using UnityEngine;

public class YPbPr : MonoBehaviour
{
	private Material material;

	private void Awake()
	{
		material = new Material(Shader.Find("Hidden/YPbPr"));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}
}
