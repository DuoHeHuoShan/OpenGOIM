using System.Collections.Generic;
using UnityEngine;

public class PotSounds : MonoBehaviour
{
	public AudioClip[] scrapes;

	public AudioClip[] clunks;

	public AudioClip[] hardclunks;

	public AudioClip[] rolls;

	public AudioClip[] splashes;

	private AudioSource audioSource;

	private float moveThreshold = 0.07f;

	private float rollThreshold = 50f;

	private float hardClunkThreshold = 200f;

	private float lastContactTimer;

	private float lastClunkTimer;

	private float lastRollTimer;

	private float lastSlideTimer;

	private float lastSplashTimer;

	public PhysicsMaterial2D staticFriction;

	public PhysicsMaterial2D slidingFriction;

	public ParticleSystem sparks;

	public ParticleSystem debris;

	public ParticleSystem water;

	private ParticleSystem.EmitParams sparkParams;

	private ParticleSystem.EmitParams debrisParams;

	private ParticleSystem.ShapeModule waterModule;

	private ParticleSystem.EmissionModule waterEmission;

	private ParticleSystem.CollisionModule sparksPC;

	private ParticleSystem.CollisionModule debrisPC;

	private ParticleSystem.CollisionModule waterPC;

	private Vector3 baseVel;

	private float tangent;

	public PolygonCollider2D bottomCol;

	public Rigidbody2D potRB;

	private Dictionary<Collider2D, IndexedCollision> collisionPoints;

	private IndexedCollision indexedCollisions = new IndexedCollision();

	private bool slide;

	private void Start()
	{
		sparkParams = default(ParticleSystem.EmitParams);
		debrisParams = default(ParticleSystem.EmitParams);
		sparksPC = sparks.collision;
		debrisPC = debris.collision;
		waterPC = water.collision;
		sparksPC.enabled = false;
		debrisPC.enabled = false;
		waterPC.enabled = false;
		collisionPoints = new Dictionary<Collider2D, IndexedCollision>();
		waterModule = water.shape;
		waterEmission = water.emission;
		audioSource = GetComponent<AudioSource>();
		lastContactTimer = 0f;
		lastClunkTimer = 0f;
		lastSlideTimer = 0f;
		lastRollTimer = 0f;
		lastSplashTimer = 0f;
		slide = true;
	}

	private void LateUpdate()
	{
		lastContactTimer += Time.deltaTime;
		lastClunkTimer += Time.deltaTime;
		lastSlideTimer += Time.deltaTime;
		lastRollTimer += Time.deltaTime;
		lastSplashTimer += Time.deltaTime;
	}

	private void FixedUpdate()
	{
		if (slide)
		{
			bottomCol.sharedMaterial = slidingFriction;
		}
		else
		{
			bottomCol.sharedMaterial = staticFriction;
		}
	}

	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (collisionPoints == null)
		{
			return;
		}
		indexedCollisions.Count = coll.GetContacts(indexedCollisions.Colliders);
		if (coll.gameObject.layer != LayerMask.NameToLayer("Terrain"))
		{
			return;
		}
		if (collisionPoints.Count > 0 && Mathf.Abs(potRB.angularVelocity) > rollThreshold)
		{
			PlayRoll(Mathf.Clamp01(Mathf.Abs(potRB.angularVelocity) / 8f * rollThreshold));
		}
		if (collisionPoints.ContainsKey(coll.collider) || collisionPoints.Count != 0)
		{
			return;
		}
		collisionPoints[coll.collider] = indexedCollisions;
		if (collisionPoints.Count > 1 && Mathf.Abs(potRB.angularVelocity) > rollThreshold)
		{
			PlayRoll(Mathf.Clamp01(Mathf.Abs(potRB.angularVelocity) / 8f * rollThreshold));
			Splash(coll.relativeVelocity.magnitude);
			return;
		}
		GroundCol component = coll.collider.GetComponent<GroundCol>();
		bool flag = false;
		if (component != null && (component.material == GroundCol.SoundMaterial.rock || component.material == GroundCol.SoundMaterial.metal || component.material == GroundCol.SoundMaterial.solidmetal))
		{
			flag = true;
		}
		float num = 0f;
		for (int i = 0; i < indexedCollisions.Count; i++)
		{
			num += indexedCollisions.Colliders[i].normalImpulse;
		}
		if (num > hardClunkThreshold * 0.3f)
		{
			if (num < hardClunkThreshold)
			{
				for (int j = 0; j < indexedCollisions.Count; j++)
				{
					debrisParams.position = indexedCollisions.Colliders[j].point;
					if (component != null)
					{
						debrisParams.startColor = component.groundCol;
					}
					Vector3 vector = 2f * new Vector3(indexedCollisions.Colliders[j].normal.y, indexedCollisions.Colliders[j].normal.x) * Vector2.Dot(new Vector2(indexedCollisions.Colliders[j].normal.y, indexedCollisions.Colliders[j].normal.x), coll.relativeVelocity);
					debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
					debris.Emit(debrisParams, 1);
					debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
					debris.Emit(debrisParams, 1);
					debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
					debris.Emit(debrisParams, 1);
				}
				PlayClunk((num - hardClunkThreshold * 0.3f) / hardClunkThreshold * 0.7f);
			}
			else if (flag)
			{
				for (int k = 0; k < indexedCollisions.Count; k++)
				{
					sparkParams.position = indexedCollisions.Colliders[k].point;
					Vector3 vector2 = 2f * new Vector3(indexedCollisions.Colliders[k].normal.y, indexedCollisions.Colliders[k].normal.x) * Vector2.Dot(new Vector2(indexedCollisions.Colliders[k].normal.y, indexedCollisions.Colliders[k].normal.x), coll.relativeVelocity);
					sparkParams.velocity = Random.insideUnitSphere * 6f + vector2;
					sparks.Emit(sparkParams, 1);
					sparkParams.velocity = Random.insideUnitSphere * 6f + vector2;
					sparks.Emit(sparkParams, 1);
					sparkParams.velocity = Random.insideUnitSphere * 6f + vector2;
					sparks.Emit(sparkParams, 1);
				}
				PlayHardClunk(num / (2f * hardClunkThreshold));
			}
			Splash(coll.relativeVelocity.magnitude);
		}
		lastContactTimer = 0f;
	}

	private void Splash(float mag)
	{
		water.transform.localEulerAngles = new Vector3(-90f, 0f, Random.Range(-180f, 180f));
		waterModule.arcSpeed = ((!(Random.value < 0.5f)) ? 2f : (-2f));
		waterEmission.rateOverTime = mag * 10f;
		water.Play();
		if (mag > 10f)
		{
			PlaySplash(Mathf.Clamp(mag / 18f - 0.2f, 0f, 0.8f));
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying || collisionPoints == null)
		{
			return;
		}
		int num = 0;
		Color[] array = new Color[6]
		{
			Color.red,
			Color.green,
			Color.blue,
			Color.cyan,
			Color.yellow,
			Color.magenta
		};
		foreach (KeyValuePair<Collider2D, IndexedCollision> collisionPoint in collisionPoints)
		{
			for (int i = 0; i < collisionPoint.Value.Count; i++)
			{
				Gizmos.color = array[num];
				Gizmos.DrawSphere((Vector3)collisionPoint.Value.Colliders[i].point + new Vector3(0f, 0f, -2f), 0.1f);
				num = (num + 1) % array.Length;
			}
		}
		if (slide)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(new Vector3(potRB.position.x, potRB.position.y, -2f), 0.5f);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(new Vector3(potRB.position.x, potRB.position.y, -2f), 0.5f);
		}
	}

	private void OnCollisionStay2D(Collision2D coll)
	{
		if (collisionPoints == null)
		{
			return;
		}
		indexedCollisions.Count = coll.GetContacts(indexedCollisions.Colliders);
		if (coll.gameObject.layer != LayerMask.NameToLayer("Terrain") || !collisionPoints.ContainsKey(coll.collider))
		{
			return;
		}
		bool flag = true;
		float num = 0.3f;
		float num2 = 0f;
		for (int i = 0; i < indexedCollisions.Count; i++)
		{
			for (int j = 0; j < collisionPoints[coll.collider].Count; j++)
			{
				float magnitude = (collisionPoints[coll.collider].Colliders[j].point - indexedCollisions.Colliders[i].point).magnitude;
				num = Mathf.Min(num, magnitude);
				num2 = Mathf.Max((1f + Mathf.Abs(indexedCollisions.Colliders[i].tangentImpulse)) / (1f + Mathf.Abs(indexedCollisions.Colliders[i].normalImpulse)), num2);
				if (magnitude < moveThreshold)
				{
					flag = false;
					break;
				}
			}
		}
		if (num2 > 6f && potRB.velocity.magnitude > 2f)
		{
			flag = true;
		}
		if (flag && coll.rigidbody == null)
		{
			collisionPoints[coll.collider] = indexedCollisions;
			GroundCol component = coll.collider.GetComponent<GroundCol>();
			if (component != null)
			{
				debrisParams.startColor = component.groundCol;
			}
			for (int k = 0; k < indexedCollisions.Count; k++)
			{
				debrisParams.position = indexedCollisions.Colliders[k].point;
				Vector3 vector = 0.4f * new Vector3(indexedCollisions.Colliders[k].normal.y, 0f - indexedCollisions.Colliders[k].normal.x) * Vector2.Dot(new Vector2(indexedCollisions.Colliders[k].normal.y, 0f - indexedCollisions.Colliders[k].normal.x), coll.relativeVelocity);
				debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
				debris.Emit(debrisParams, (int)Mathf.Max(num * 50f - 1f, 0f));
				PlayScrape();
			}
		}
		slide = flag;
	}

	private void OnCollisionExit2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain") && collisionPoints.ContainsKey(coll.collider))
		{
			collisionPoints.Remove(coll.collider);
		}
		if (collisionPoints == null)
		{
			slide = true;
		}
		else if (collisionPoints.Keys.Count == 0)
		{
			slide = true;
		}
	}

	private void PlaySplash(float volume = 1f)
	{
		if (!(lastSplashTimer < 0.1f))
		{
			lastSplashTimer = 0f;
			audioSource.PlayOneShot(splashes[Random.Range(0, splashes.Length)], volume);
		}
	}

	private void PlayClunk(float volume = 1f)
	{
		if (!(lastClunkTimer < 0.1f))
		{
			lastClunkTimer = 0f;
			audioSource.PlayOneShot(clunks[Random.Range(0, clunks.Length)], volume);
		}
	}

	private void PlayHardClunk(float volume = 1f)
	{
		if (!(lastClunkTimer < 0.1f))
		{
			lastClunkTimer = 0f;
			audioSource.PlayOneShot(hardclunks[Random.Range(0, hardclunks.Length)], volume);
		}
	}

	private void PlayScrape(float volume = 1f)
	{
		if (!(lastSlideTimer < 0.1f))
		{
			lastSlideTimer = 0f;
			audioSource.PlayOneShot(scrapes[Random.Range(0, scrapes.Length)], volume);
		}
	}

	private void PlayRoll(float volume = 1f)
	{
		if (!(lastRollTimer < 0.1f))
		{
			lastRollTimer = 0f;
			audioSource.PlayOneShot(rolls[Random.Range(0, rolls.Length)], volume);
		}
	}
}
