using System.Linq;
using UnityEngine;

public class CheatDragRuntime : MonoBehaviour
{
	public PlayerControl player;

	public float Power = 2f;

	public bool isDragging;

	private Vector3 targetPlayerWorldPoint;

	private int? fingerId;

	private void Update()
	{
		if (Input.touchCount > 1)
		{
			if (!isDragging)
			{
				player.GetComponent<Rigidbody2D>().gravityScale = 0f;
				fingerId = Input.GetTouch(1).fingerId;
			}
			isDragging = true;
			Touch touch = Input.touches.FirstOrDefault((Touch x) => x.fingerId == fingerId);
			if (touch.fingerId == fingerId)
			{
				Vector3 position = player.transform.position;
				targetPlayerWorldPoint = Camera.main.ScreenToWorldPoint(touch.position);
				targetPlayerWorldPoint = new Vector3(targetPlayerWorldPoint.x, targetPlayerWorldPoint.y, position.z);
			}
			return;
		}
		if (Input.touchCount == 1)
		{
			int? num = fingerId;
			if (num.HasValue && Input.touches.Any((Touch x) => x.fingerId == fingerId))
			{
				Touch touch2 = Input.touches.FirstOrDefault((Touch x) => x.fingerId == fingerId);
				if (touch2.fingerId == fingerId)
				{
					Vector3 position2 = player.transform.position;
					targetPlayerWorldPoint = Camera.main.ScreenToWorldPoint(touch2.position);
					targetPlayerWorldPoint = new Vector3(targetPlayerWorldPoint.x, targetPlayerWorldPoint.y, position2.z);
				}
				return;
			}
		}
		if (isDragging)
		{
			player.GetComponent<Rigidbody2D>().gravityScale = 1f;
		}
		isDragging = false;
		fingerId = null;
	}

	private void FixedUpdate()
	{
		if (isDragging)
		{
			Vector3 position = player.transform.position;
			Vector3 vector = Vector3.Lerp(position, targetPlayerWorldPoint, Time.fixedDeltaTime * Power);
			player.GetComponent<Rigidbody2D>().MovePosition(vector);
		}
	}
}
