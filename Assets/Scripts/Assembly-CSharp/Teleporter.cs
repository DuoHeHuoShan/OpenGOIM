using UnityEngine;

public class Teleporter : MonoBehaviour
{
	public Transform targetX;

	public GameObject player;

	public bool shouldTeleport;

	public bool shouldEnable;

	public bool enableTeleport;

	public Transform cursor;

	private void Start()
	{
		shouldEnable = false;
		shouldTeleport = false;
		enableTeleport = true;
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (enableTeleport && coll.gameObject == player)
		{
			shouldTeleport = true;
		}
	}

	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject == player)
		{
			shouldEnable = true;
		}
	}

	private void LateUpdate()
	{
		if (shouldEnable)
		{
			enableTeleport = true;
			shouldEnable = false;
		}
		if (shouldTeleport)
		{
			targetX.GetComponent<Teleporter>().enableTeleport = false;
			Vector3 vector = Camera.main.transform.position - player.transform.position;
			Vector3 vector2 = cursor.position - player.transform.position;
			player.transform.position = new Vector3(targetX.position.x, player.transform.position.y, player.transform.position.z);
			Camera.main.transform.position = player.transform.position + vector;
			cursor.position = player.transform.position + vector2;
			shouldTeleport = false;
			enableTeleport = false;
			shouldEnable = true;
		}
	}
}
