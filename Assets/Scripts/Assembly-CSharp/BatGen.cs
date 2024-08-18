using System.Collections;
using UnityEngine;

public class BatGen : MonoBehaviour
{
	public GameObject batPrefab;

	private bool done;

	private int numBats = 210;

	private int batsSpawned;

	private AudioSource source;

	public Camera batCam;

	private void Start()
	{
		done = false;
		batsSpawned = 0;
		source = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (done && base.transform.childCount <= 1)
		{
			batCam.gameObject.SetActive(false);
			Object.Destroy(base.gameObject);
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
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (!done && !(coll.name != "PotCollider"))
		{
			done = true;
			batCam.gameObject.SetActive(true);
			StartCoroutine("Spawn");
			int @int = PlayerPrefs.GetInt("NumWins");
			if (@int <= 0)
			{
				source.Play();
			}
		}
	}
}
