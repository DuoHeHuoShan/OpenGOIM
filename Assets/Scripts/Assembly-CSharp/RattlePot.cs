using UnityEngine;

public class RattlePot : MonoBehaviour
{
	public AudioClip rattle1;

	public AudioClip rattle2;

	public AudioClip groan;

	private AudioSource aud;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	public void Rattle1()
	{
		aud.PlayOneShot(rattle1);
	}

	public void Rattle2()
	{
		aud.PlayOneShot(rattle2);
	}

	public void Groan()
	{
		aud.PlayOneShot(groan);
	}
}
