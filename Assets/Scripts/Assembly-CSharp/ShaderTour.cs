using System.Collections;
using UnityEngine;

public class ShaderTour : MonoBehaviour
{
	public bool doTour;

	private DebugSpawn debugSpawner;

	private MobileManager mobileMan;

	private int i;

	private int j;

	private void Awake()
	{
		if (Application.isEditor)
		{
			Application.runInBackground = true;
		}
	}

	private IEnumerator Start()
	{
		debugSpawner = GameObject.FindGameObjectWithTag("DebugSpawner").GetComponent<DebugSpawn>();
		mobileMan = GameObject.FindGameObjectWithTag("MobileManager").GetComponent<MobileManager>();
		yield return new WaitForSeconds(2f);
		if (doTour)
		{
			StartCoroutine("Tour");
		}
	}

	private IEnumerator Tour()
	{
		while (j < 22)
		{
			Debug.Log("Next Quality Settings: " + j);
			mobileMan.SetNewQualitySettings(j);
			while (i < 24)
			{
				yield return new WaitForSeconds(1f);
				debugSpawner.GoRight();
				i++;
				Debug.Log("Spawn " + i + " of 24");
			}
			j++;
			i = 0;
		}
		Debug.Log("Tour Complete");
		yield return null;
	}

	private void Update()
	{
	}
}
