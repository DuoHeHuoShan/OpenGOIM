using UnityEngine;

public class SplashControl : MonoBehaviour
{
	public ParticleSystem smallSplash;

	public ParticleSystem largeSplash;

	private ParticleSystem.EmitParams largeParams;

	private ParticleSystem.EmitParams smallParams;

	public AudioClip smallSplashSound;

	public AudioClip largeSplashSound;

	public AudioSource aud;

	private BoxCollider2D myCol;

	private float groundHeight;

	private void Start()
	{
		myCol = GetComponent<BoxCollider2D>();
		groundHeight = myCol.bounds.max.y;
		largeParams = default(ParticleSystem.EmitParams);
		smallParams = default(ParticleSystem.EmitParams);
		smallParams.velocity = Vector3.zero;
		largeParams.velocity = Vector3.zero;
	}

	private void OnTriggerEnter2D(Collider2D otherCol)
	{
		Vector2 velocity = otherCol.attachedRigidbody.velocity;
		if (!(otherCol.name == "PotCollider") && !(otherCol.name == "Player"))
		{
			return;
		}
		if (velocity.y < -2f)
		{
			for (int i = 0; i < (int)(-0.5 * (double)velocity.y); i++)
			{
				smallSplash.transform.position = new Vector3(otherCol.bounds.center.x + Random.Range(-0.5f, 0.5f), groundHeight, 0f);
				smallSplash.Emit(1);
			}
			aud.PlayOneShot(smallSplashSound, 0.3f);
		}
		if (velocity.y < -8f)
		{
			largeSplash.transform.position = new Vector3(otherCol.bounds.center.x, groundHeight, 0f);
			largeSplash.Emit(1);
			aud.PlayOneShot(largeSplashSound, 0.3f);
		}
	}
}
