using UnityEngine;

public class CraneMove : MonoBehaviour
{
	private Vector2 initialPos;

	private Rigidbody2D rb;

	private void Start()
	{
		initialPos = base.transform.position;
		rb = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		rb.MovePosition(new Vector2(initialPos.x + Mathf.Sin(Time.time) * 0.01f, initialPos.y));
	}
}
