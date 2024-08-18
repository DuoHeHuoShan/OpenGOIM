using UnityEngine;

public class MipmapBias : MonoBehaviour
{
	public Texture[] textures;

	private float bias = -1f;

	private void Start()
	{
		Texture[] array = textures;
		foreach (Texture texture in array)
		{
			texture.mipMapBias = bias;
		}
	}
}
