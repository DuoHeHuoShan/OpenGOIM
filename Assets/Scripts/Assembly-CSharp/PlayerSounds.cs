using UnityEngine;
using UnityEngine.UI;

public class PlayerSounds : MonoBehaviour
{
	public AudioClip[] hardStrainSounds;

	public AudioClip[] strainSounds;

	public AudioClip[] releaseSounds;

	public AudioClip[] painSounds;

	public AudioClip[] impactSounds;

	public AudioClip[] sadSounds;

	public AudioClip[] grunts;

	public AudioClip[] jumpSounds;

	public AudioClip[] whuSounds;

	public HingeJoint2D hj;

	public SliderJoint2D sj;

	private Rigidbody2D rb;

	private Vector2 deltaVelocity;

	private Vector2 smoothDeltaVelocity;

	private Vector2 oldVelocity;

	private float lastSoundTimer;

	private AudioSource audioSource;

	private int lastSoundIndex;

	public Transform hammerTransform;

	private Vector2 oldHammerPos;

	private float strain;

	private float oldStrain;

	private float oldDeltaStrain;

	private float deltaStrainAccumulator;

	private float hurtThreshold = 8f;

	private float jumpThreshold = 10f;

	private float releaseThreshold = -2f;

	private float strainThreshold = 0.6f;

	private float hardStrainThreshold = 7f;

	private float hardStrainSpeedThreshold = 0.01f;

	private float impactThreshold = 20f;

	private float fallTime = 0.2f;

	private float fallTimer;

	private bool whuhThisFall;

	private bool sadThisFall;

	public LayerMask fallLayer;

	public Image strainbar;

	public Image deltaStrainbar;

	public Image fallBar;

	public Image jerkBar;

	public bool isTouching;

	public SkinnedMeshRenderer skin;

	public Transform neck;

	public PoseControl pose;

	private Saviour saviour;

	private float blinkTimer;

	private float mouthTimer;

	private ContactPoint2D[] contactPoints = new ContactPoint2D[20];

	private void Start()
	{
		saviour = GetComponent<Saviour>();
		lastSoundTimer = 0f;
		lastSoundIndex = 0;
		deltaVelocity = Vector2.zero;
		smoothDeltaVelocity = Vector2.zero;
		oldVelocity = Vector2.zero;
		audioSource = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody2D>();
		strain = 1f;
		oldStrain = 1f;
		oldDeltaStrain = 0f;
		deltaStrainAccumulator = 0f;
		fallTimer = 0f;
		isTouching = false;
		whuhThisFall = false;
		sadThisFall = false;
		oldHammerPos = hammerTransform.position;
		blinkTimer = 0f;
		mouthTimer = 0f;
	}

	private void Update()
	{
		if (lastSoundTimer > 0f)
		{
			lastSoundTimer -= Time.deltaTime;
		}
		else
		{
			lastSoundTimer = 0f;
		}
		if (!isTouching)
		{
			if (smoothDeltaVelocity.y < Physics2D.gravity.y * Time.fixedDeltaTime * 0.95f && Mathf.Abs(smoothDeltaVelocity.x) < 0.05f)
			{
				fallTimer += Time.deltaTime;
				RaycastHit2D raycastHit2D = Physics2D.CircleCast(rb.position, 0.5f, smoothDeltaVelocity.normalized, 100f, fallLayer);
				fallBar.rectTransform.localScale = new Vector3(fallTimer / fallTime, 0.25f, 1f);
				if (raycastHit2D.collider == null || raycastHit2D.distance > 10f)
				{
					if (fallTimer > fallTime && !sadThisFall)
					{
						PlaySadSound();
						sadThisFall = true;
						Debug.Log("falling over 10m");
					}
				}
				else if (raycastHit2D.distance > 5f && !whuhThisFall)
				{
					PlayWhuh();
					whuhThisFall = true;
				}
			}
			else
			{
				fallTimer = 0f;
			}
		}
		else
		{
			fallTimer = 0f;
			whuhThisFall = false;
			sadThisFall = false;
		}
		blinkTimer -= Time.deltaTime;
		if (blinkTimer < 0f)
		{
			blinkTimer = Random.Range(3f, 4f);
			skin.SetBlendShapeWeight(16, 100f);
			skin.SetBlendShapeWeight(19, 100f);
		}
		else
		{
			skin.SetBlendShapeWeight(16, Mathf.Lerp(skin.GetBlendShapeWeight(16), 0f, 0.5f));
			skin.SetBlendShapeWeight(19, Mathf.Lerp(skin.GetBlendShapeWeight(19), 0f, 0.5f));
		}
		mouthTimer -= Time.deltaTime;
		if (mouthTimer <= 0f)
		{
			mouthTimer = 0f;
		}
		skin.SetBlendShapeWeight(23, Mathf.Min(100f, mouthTimer * 60f));
		skin.SetBlendShapeWeight(38, Mathf.Max(0f, oldDeltaStrain) * 5f);
	}

	private void LateUpdate()
	{
		neck.localRotation = Quaternion.Euler(6.784f, 0f, Random.Range(-0.1f * Mathf.Max(0f, oldDeltaStrain), 0.1f * Mathf.Max(0f, oldDeltaStrain)));
	}

	private void LateFixedUpdate()
	{
	}

	private void FixedUpdate()
	{
		deltaVelocity = rb.velocity - oldVelocity;
		smoothDeltaVelocity = Vector2.Lerp(smoothDeltaVelocity, deltaVelocity, 0.2f);
		float num = Mathf.Abs(hj.GetReactionTorque(Time.fixedDeltaTime)) + sj.GetReactionForce(Time.fixedDeltaTime).magnitude;
		if (num > 7000f)
		{
			strain += num * 0.0009f * Time.fixedDeltaTime;
		}
		else
		{
			strain -= 0.026f;
		}
		strain = Mathf.Clamp(strain, 0f, 20f);
		strainbar.rectTransform.localScale = new Vector3(strain * 2f, 0.25f, 1f);
		float b = 100f * (strain - oldStrain);
		float num2 = Mathf.Lerp(oldDeltaStrain, b, 0.25f);
		deltaStrainAccumulator *= 0.92f;
		deltaStrainAccumulator += num2 * Time.fixedDeltaTime;
		deltaStrainAccumulator = Mathf.Max(0f, deltaStrainAccumulator);
		strainbar.rectTransform.localScale = new Vector3(deltaStrainAccumulator, 0.25f, 1f);
		float magnitude = ((Vector2)hammerTransform.position - oldHammerPos).magnitude;
		oldHammerPos = hammerTransform.position;
		float num3 = num2 - oldDeltaStrain;
		jerkBar.rectTransform.localScale = new Vector3(num3 * 0.1f, 0.25f, 1f);
		deltaStrainbar.rectTransform.localScale = new Vector3(num2, 0.25f, 1f);
		if (num2 > jumpThreshold && num < 0.1f)
		{
			PlayJump();
		}
		else if (num3 < releaseThreshold)
		{
			PlayReleaseSound();
		}
		else if (num2 > hardStrainThreshold && magnitude < hardStrainSpeedThreshold)
		{
			PlayStrainSound(1f, true);
		}
		else if (deltaStrainAccumulator > strainThreshold)
		{
			PlayStrainSound();
			deltaStrainAccumulator = 0f;
		}
		oldStrain = strain;
		oldDeltaStrain = num2;
		oldVelocity = rb.velocity;
		isTouching = false;
	}

	private void OnCollisionEnter2D(Collision2D coll)
	{
		coll.GetContacts(contactPoints);
		if (coll.relativeVelocity.magnitude > impactThreshold && contactPoints[0].normal.y > 0.5f)
		{
			PlayImpactSound();
		}
		else if (rb.GetPoint(contactPoints[0].point).y > 0.4f && coll.relativeVelocity.magnitude > hurtThreshold)
		{
			PlayPainSound();
		}
	}

	private void OnCollisionStay2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			isTouching = true;
		}
	}

	private void PlayStrainSound(float volume = 1f, bool hardStrain = false)
	{
		if (!(lastSoundTimer > 0f))
		{
			if (hardStrain)
			{
				lastSoundIndex = Random.Range(0, hardStrainSounds.Length);
				audioSource.PlayOneShot(hardStrainSounds[lastSoundIndex], volume);
				lastSoundTimer = hardStrainSounds[lastSoundIndex].length * 2.5f;
			}
			else
			{
				lastSoundIndex = Random.Range(0, strainSounds.Length);
				audioSource.PlayOneShot(strainSounds[lastSoundIndex], volume);
				lastSoundTimer = strainSounds[lastSoundIndex].length * 1.5f;
			}
		}
	}

	private void PlayReleaseSound(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			lastSoundIndex = Random.Range(0, releaseSounds.Length);
			audioSource.PlayOneShot(releaseSounds[lastSoundIndex], volume);
			lastSoundTimer = releaseSounds[lastSoundIndex].length;
			mouthTimer = releaseSounds[lastSoundIndex].length;
		}
	}

	private void PlayPainSound(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			lastSoundIndex = Random.Range(0, painSounds.Length);
			audioSource.PlayOneShot(painSounds[lastSoundIndex], volume);
			lastSoundTimer = painSounds[lastSoundIndex].length * 1.5f;
			mouthTimer = painSounds[lastSoundIndex].length;
		}
	}

	private void PlayImpactSound(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			lastSoundIndex = Random.Range(0, impactSounds.Length);
			audioSource.PlayOneShot(impactSounds[lastSoundIndex], volume);
			lastSoundTimer = impactSounds[lastSoundIndex].length * 2.5f;
			mouthTimer = impactSounds[lastSoundIndex].length;
		}
	}

	private void PlaySadSound(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			lastSoundIndex = Random.Range(0, sadSounds.Length);
			audioSource.PlayOneShot(sadSounds[lastSoundIndex], volume);
			lastSoundTimer = sadSounds[lastSoundIndex].length * 2.5f;
			mouthTimer = sadSounds[lastSoundIndex].length;
			saviour.SaveGameNow(false);
		}
	}

	private void PlayWhuh(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			lastSoundIndex = Random.Range(0, whuSounds.Length);
			audioSource.PlayOneShot(whuSounds[lastSoundIndex], volume);
			lastSoundTimer = whuSounds[lastSoundIndex].length * 1.5f;
			mouthTimer = whuSounds[lastSoundIndex].length;
		}
	}

	private void PlayGrunt(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			audioSource.PlayOneShot(grunts[lastSoundIndex], volume);
			lastSoundTimer = grunts[lastSoundIndex].length * 1.5f;
		}
	}

	private void PlayJump(float volume = 1f)
	{
		if (!(lastSoundTimer > 0f))
		{
			lastSoundIndex = Random.Range(0, jumpSounds.Length);
			audioSource.PlayOneShot(jumpSounds[lastSoundIndex], volume);
			lastSoundTimer = jumpSounds[lastSoundIndex].length * 1.5f;
		}
	}
}
