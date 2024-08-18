using UnityEngine;

public class PropSound : MonoBehaviour
{
	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.loop = false;
	}

	public void OnCollisionEnter2D(Collision2D coll)
	{
		if (!(audioSource == null) && coll.relativeVelocity.magnitude > 0.2f && !audioSource.isPlaying)
		{
			audioSource.volume = Mathf.Log(coll.relativeVelocity.magnitude + 1f);
			audioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);
			audioSource.Play();
		}
	}
}
