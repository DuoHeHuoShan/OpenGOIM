using System.Collections;
using UnityEngine;

public class GravityControl : MonoBehaviour
{
	public Transform[] gravityWells;

	private Vector2 gvec;

	public GameObject creditsPrefab;

	public Transform creditsParent;

	public bool creditsUp;

	public ProgressMeter progressMeter;

	public GameObject starNest;

	public Narrator narrator;

	public SettingsManager settingsMenu;

	public Camera fgCam;

	private void Start()
	{
		Physics2D.gravity = new Vector2(0f, -30f);
		creditsUp = false;
		GameObject[] array = GameObject.FindGameObjectsWithTag("GravityWell");
		gravityWells = new Transform[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			gravityWells[i] = array[i].transform;
		}
		if (narrator == null)
		{
			narrator = GameObject.FindGameObjectWithTag("Narrator").GetComponent<Narrator>();
		}
	}

	private void FixedUpdate()
	{
		if (Physics2D.gravity.y == 0f)
		{
		}
	}

	private void OnTriggerStay2D(Collider2D coll)
	{
		if (creditsUp)
		{
			return;
		}
		Transform[] array = gravityWells;
		foreach (Transform transform in array)
		{
			if (coll.attachedRigidbody != null)
			{
				gvec = (Vector2)transform.position - coll.attachedRigidbody.position;
				coll.attachedRigidbody.AddForce(2500f / gvec.sqrMagnitude * gvec.normalized);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		Physics2D.gravity = new Vector2(0f, 0f);
	}

	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.attachedRigidbody.position.y > GetComponent<BoxCollider2D>().bounds.max.y - 5f)
		{
			if (creditsUp)
			{
				return;
			}
			Physics2D.gravity = new Vector2(0f, 1.2f);
			GameObject gameObject = Object.Instantiate(creditsPrefab, creditsParent);
			gameObject.transform.SetAsFirstSibling();
			starNest.SetActive(true);
			starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", 0f);
			StartCoroutine("FadeUpStarNest");
			creditsUp = true;
			progressMeter.Pause(true);
			PlayerPrefs.DeleteKey("NumSaves");
			PlayerPrefs.DeleteKey("SaveGame0");
			PlayerPrefs.DeleteKey("SaveGame1");
			PlayerPrefs.SetFloat("LastTime", narrator.timePlayedThisGame);
			int @int = PlayerPrefs.GetInt("NumWins");
			@int++;
			PlayerPrefs.SetInt("NumWins", @int);
			if (PlayerPrefs.HasKey("BestTime"))
			{
				float @float = PlayerPrefs.GetFloat("BestTime");
				if (narrator.timePlayedThisGame < @float)
				{
					PlayerPrefs.SetFloat("BestTime", narrator.timePlayedThisGame);
				}
			}
			else
			{
				PlayerPrefs.SetFloat("BestTime", narrator.timePlayedThisGame);
			}
			PlayerPrefs.Save();
			settingsMenu.EnableSkipCredits();
		}
		else
		{
			Physics2D.gravity = new Vector2(0f, -30f);
		}
	}

	private IEnumerator FadeUpStarNest()
	{
		float step = 2.0000001E-05f;
		for (float f = 0f; f <= 0.01f; f += step)
		{
			starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", f);
			yield return null;
		}
	}
}
