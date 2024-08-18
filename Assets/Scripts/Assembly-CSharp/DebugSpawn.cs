using System.Collections.Generic;
using UnityEngine;

public class DebugSpawn : MonoBehaviour
{
	private List<Transform> spawners;

	private int currentSpawner;

	public GameObject player;

	private int prevSpawner;

	private bool timeToGo;

	private void Start()
	{
		spawners = new List<Transform>(base.transform.GetComponentsInChildren<Transform>());
		spawners.Sort(CompareTransform);
		currentSpawner = 0;
		float num = 9999f;
		for (int i = 0; i < spawners.Count; i++)
		{
			float sqrMagnitude = (player.transform.position - spawners[i].position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				currentSpawner = i;
			}
		}
	}

	public void GoLeft()
	{
		timeToGo = true;
		currentSpawner = (spawners.Count + currentSpawner - 1) % spawners.Count;
	}

	public void GoRight()
	{
		timeToGo = true;
		currentSpawner = (currentSpawner + 1) % spawners.Count;
	}

	private void Update()
	{
		if (!Application.isEditor)
		{
			return;
		}
		if (currentSpawner != prevSpawner && timeToGo)
		{
			player.transform.position = spawners[currentSpawner].position;
			timeToGo = false;
		}
		prevSpawner = currentSpawner;
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				GoLeft();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				GoRight();
			}
		}
	}

	private static int CompareTransform(Transform A, Transform B)
	{
		return A.name.CompareTo(B.name);
	}
}
