using System.Collections;
using UnityEngine;

public class OpenGift : MonoBehaviour
{
	public GameObject batPrefab;

	private bool done;

	private int numBats = 150;

	private int batsSpawned;

	public AudioSource source;

	public Camera batCam;

	private void Start()
	{
		done = false;
		batsSpawned = 0;
	}

	private void Update()
	{
		if (done)
		{
			Debug.Log(base.transform.childCount);
			if (base.transform.childCount <= 2)
			{
				batCam.gameObject.SetActive(false);
				Object.Destroy(base.gameObject);
			}
		}
	}

	private IEnumerator Spawn()
	{
		while (batsSpawned < numBats)
		{
			for (int i = 0; i < 3; i++)
			{
				GameObject gameObject = Object.Instantiate(batPrefab, base.transform.position + (Vector3)Random.insideUnitCircle, Quaternion.identity, base.transform);
				gameObject.GetComponent<BatControl>().mainCam = batCam;
				batsSpawned++;
			}
			yield return null;
		}
		MeshRenderer[] rens = GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] array = rens;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.enabled = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (!done && (!(coll.collider.name != "Tip") || !(coll.collider.name != "PotCollider")))
		{
			done = true;
			batCam.gameObject.SetActive(true);
			StartCoroutine("Spawn");
			GetComponent<Rigidbody2D>().isKinematic = true;
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			GetComponent<Rigidbody2D>().angularVelocity = 0f;
			source.Play();
		}
	}
}
