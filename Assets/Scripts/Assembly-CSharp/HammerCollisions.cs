using System.Collections.Generic;
using UnityEngine;

public class HammerCollisions : MonoBehaviour
{
	public ImpactBlur impactBlur;

	public Dictionary<Collider2D, IndexedCollision> collisionPoints;

	private Collider2D myCollider;

	public PhysicsMaterial2D staticFriction;

	public PhysicsMaterial2D slidingFriction;

	public MeshRenderer headMesh;

	private float moveThreshold = 0.03f;

	private bool slide;

	private ParticleSystem.EmitParams sparkParams;

	private ParticleSystem.EmitParams debrisParams;

	private ParticleSystem.EmitParams dustParams;

	public ParticleSystem debris;

	public ParticleSystem sparks;

	public ParticleSystem dust;

	public AudioClip[] whooshes;

	private AudioSource audioSource;

	public GameObject player;

	private float lastSoundTimer;

	private Vector2 deltaPos;

	private Vector2 oldDeltaPos;

	private float whooshThreshold = 1.5f;

	private float lastWhooshTimer;

	private Rigidbody2D tip;

	public HitSoundProvider hs;

	private float hardHitThreshold = 14f;

	private IndexedCollision indexedCollisions = new IndexedCollision();

	private void Start()
	{
		collisionPoints = new Dictionary<Collider2D, IndexedCollision>();
		myCollider = GetComponent<Collider2D>();
		slide = true;
		sparkParams = default(ParticleSystem.EmitParams);
		debrisParams = default(ParticleSystem.EmitParams);
		dustParams = default(ParticleSystem.EmitParams);
		audioSource = GetComponent<AudioSource>();
		lastSoundTimer = 0f;
		lastWhooshTimer = 0f;
		deltaPos = base.transform.position - player.transform.position;
		oldDeltaPos = deltaPos;
		tip = GetComponent<Rigidbody2D>();
	}

	private void LateUpdate()
	{
		lastSoundTimer += Time.deltaTime;
		lastWhooshTimer += Time.deltaTime;
		deltaPos = base.transform.position - player.transform.position;
		float magnitude = (deltaPos - oldDeltaPos).magnitude;
		if (magnitude > whooshThreshold)
		{
			PlayWhoosh(Mathf.Clamp(deltaPos.magnitude * 0.26f, 0.1f, 1f));
		}
		oldDeltaPos = deltaPos;
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
			ContactPoint2D[] colliders = collisionPoint.Value.Colliders;
			foreach (ContactPoint2D contactPoint2D in colliders)
			{
				Gizmos.color = array[num];
				Gizmos.DrawSphere(contactPoint2D.point, 0.1f);
				num = (num + 1) % array.Length;
			}
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
		if (coll.relativeVelocity.magnitude > 14f)
		{
			player.SendMessage("HammerReturn");
		}
		if (collisionPoints.ContainsKey(coll.collider))
		{
			return;
		}
		collisionPoints[coll.collider] = indexedCollisions;
		GroundCol component = coll.collider.GetComponent<GroundCol>();
		impactBlur.Impact(new Vector2(indexedCollisions.Colliders[0].point.x, indexedCollisions.Colliders[0].point.y), coll.relativeVelocity.magnitude / 12f);
		bool flag = false;
		if (component != null && (component.material == GroundCol.SoundMaterial.rock || component.material == GroundCol.SoundMaterial.metal || component.material == GroundCol.SoundMaterial.solidmetal))
		{
			flag = true;
		}
		if (coll.relativeVelocity.magnitude < hardHitThreshold)
		{
			for (int i = 0; i < indexedCollisions.Count; i++)
			{
				debrisParams.position = indexedCollisions.Colliders[i].point;
				if (component != null)
				{
					debrisParams.startColor = component.groundCol;
				}
				float normalImpulse = indexedCollisions.Colliders[i].normalImpulse;
				float tangentImpulse = indexedCollisions.Colliders[i].tangentImpulse;
				Vector3 vector = indexedCollisions.Colliders[i].normal;
				Vector3 vector2 = new Vector3(vector.y, 0f - vector.x, 0f);
				for (int j = 0; j < (int)Mathf.Min((normalImpulse + tangentImpulse) * 0.1f, 20f); j++)
				{
					debrisParams.velocity = vector * normalImpulse * 0.2f + vector2 * ((j % 2 != 0) ? (-1f) : 1f) * tangentImpulse * 0.1f + Random.insideUnitSphere * 3.4f;
					debris.Emit(debrisParams, 1);
				}
			}
			if (component != null)
			{
				PlayHit(coll.relativeVelocity.magnitude / hardHitThreshold, component.material);
			}
			else
			{
				PlayHit(coll.relativeVelocity.magnitude / hardHitThreshold);
			}
			return;
		}
		if (flag)
		{
			for (int k = 0; k < indexedCollisions.Count; k++)
			{
				sparkParams.position = indexedCollisions.Colliders[k].point;
				float normalImpulse2 = indexedCollisions.Colliders[k].normalImpulse;
				float tangentImpulse2 = indexedCollisions.Colliders[k].tangentImpulse;
				float value = normalImpulse2 + tangentImpulse2;
				Vector3 vector3 = indexedCollisions.Colliders[k].normal;
				Vector3 vector4 = new Vector3(vector3.y, 0f - vector3.x, 0f);
				for (int l = 0; l < (int)Mathf.Min((normalImpulse2 + tangentImpulse2) * 0.1f, 20f); l++)
				{
					sparkParams.velocity = vector3 * normalImpulse2 * 0.3f + vector4 * ((l % 2 != 0) ? (-1f) : 1f) * tangentImpulse2 * 0.3f + Random.insideUnitSphere * Mathf.Clamp(value, 1f, 60f) * 0.1f;
					sparks.Emit(sparkParams, 1);
				}
			}
		}
		else
		{
			for (int m = 0; m < indexedCollisions.Count; m++)
			{
				debrisParams.position = indexedCollisions.Colliders[m].point;
				if (component != null)
				{
					debrisParams.startColor = component.groundCol;
				}
				float normalImpulse3 = indexedCollisions.Colliders[m].normalImpulse;
				float tangentImpulse3 = indexedCollisions.Colliders[m].tangentImpulse;
				float value2 = normalImpulse3 + tangentImpulse3;
				Vector3 vector5 = indexedCollisions.Colliders[m].normal;
				Vector3 vector6 = new Vector3(vector5.y, 0f - vector5.x, 0f);
				for (int n = 0; n < (int)Mathf.Min((normalImpulse3 + tangentImpulse3) * 0.1f, 20f); n++)
				{
					debrisParams.velocity = vector5 * normalImpulse3 * 0.1f + vector6 * ((n % 2 != 0) ? (-1f) : 1f) * tangentImpulse3 * 0.1f + Random.insideUnitSphere * Mathf.Clamp(value2, 1f, 60f) * 0.1f;
					debris.Emit(debrisParams, 1);
				}
			}
		}
		if (component != null)
		{
			PlayHardHit(coll.relativeVelocity.magnitude / 4f, component.material);
		}
		else
		{
			PlayHardHit(coll.relativeVelocity.magnitude / 4f);
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
		if (tip.velocity.magnitude > 0.3f && num2 > 5f)
		{
			flag = true;
		}
		if (flag && coll.rigidbody == null)
		{
			collisionPoints[coll.collider] = indexedCollisions;
			GroundCol component = coll.collider.GetComponent<GroundCol>();
			if (component != null)
			{
				dustParams.startColor = component.groundCol;
			}
			for (int k = 0; k < indexedCollisions.Count; k++)
			{
				dustParams.position = indexedCollisions.Colliders[k].point;
				Vector3 vector = 0.4f * new Vector3(indexedCollisions.Colliders[k].normal.y, 0f - indexedCollisions.Colliders[k].normal.x) * Vector2.Dot(new Vector2(indexedCollisions.Colliders[k].normal.y, 0f - indexedCollisions.Colliders[k].normal.x), coll.relativeVelocity);
				dustParams.velocity = Random.insideUnitSphere * 2f + vector;
				dust.Emit(dustParams, (int)Mathf.Max(num * 50f - 1f, 0f));
				if (!audioSource.isPlaying)
				{
					if (component != null)
					{
						PlayScrape(1f, component.material);
					}
					else
					{
						PlayScrape();
					}
				}
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

	private void FixedUpdate()
	{
		if (slide)
		{
			myCollider.sharedMaterial = slidingFriction;
		}
		else
		{
			myCollider.sharedMaterial = staticFriction;
		}
	}

	private void PlayHit(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (!(lastSoundTimer < 0.1f))
		{
			lastSoundTimer = 0f;
			audioSource.PlayOneShot(hs.GetHit(mat), volume);
		}
	}

	private void PlayHardHit(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (!(lastSoundTimer < 0.1f))
		{
			lastSoundTimer = 0f;
			audioSource.PlayOneShot(hs.GetHardHit(mat), volume);
		}
	}

	private void PlayScrape(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (!(lastSoundTimer < 0.1f))
		{
			lastSoundTimer = 0f;
			audioSource.PlayOneShot(hs.GetScrape(mat), volume);
		}
	}

	private void PlayWhoosh(float volume = 1f)
	{
		if (!(lastWhooshTimer < 1f))
		{
			lastWhooshTimer = 0f;
			audioSource.PlayOneShot(whooshes[Random.Range(0, whooshes.Length)], volume);
		}
	}
}
