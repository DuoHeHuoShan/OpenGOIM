using UnityEngine;

public class AnimCallbacks : MonoBehaviour
{
	public GameObject loader;

	public ParticleSystem rocks;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void EndIntroAnim()
	{
		loader.SendMessage("EndIntroAnim", SendMessageOptions.DontRequireReceiver);
		rocks.Play();
	}
}
