using UnityEngine;

public class GiftSpawner : MonoBehaviour
{
	private float timeInTrigger;

	private bool spawned;

	public GameObject gift;

	public Transform player;

	public Transform spawn1;

	public Transform spawn2;

	private void Start()
	{
		spawned = false;
		timeInTrigger = 0f;
	}

	private void Update()
	{
	}

	private void OnTriggerStay2D(Collider2D coll)
	{
		if (coll.name != "PotCollider" || spawned)
		{
			return;
		}
		timeInTrigger += Time.fixedDeltaTime;
		if (timeInTrigger > 480f)
		{
			if (player.position.x < base.transform.position.x)
			{
				gift.transform.position = spawn1.position;
				gift.SetActive(true);
			}
			else
			{
				gift.transform.position = spawn2.position;
				gift.SetActive(true);
			}
			spawned = true;
		}
	}
}
