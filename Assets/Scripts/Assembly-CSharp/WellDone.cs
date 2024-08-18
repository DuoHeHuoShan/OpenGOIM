using UnityEngine;

public class WellDone : MonoBehaviour
{
	private AudioSource aud;

	private bool done;

	private void Start()
	{
		done = false;
		aud = GetComponent<AudioSource>();
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (!done && !(coll.name != "PotCollider"))
		{
			done = true;
			aud.Play();
		}
	}
}
