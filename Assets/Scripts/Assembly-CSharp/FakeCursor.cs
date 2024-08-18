using UnityEngine;

public class FakeCursor : MonoBehaviour
{
	public Rigidbody2D tip;

	public Rigidbody2D rb;

	private void Start()
	{
		rb.MovePosition(tip.position);
	}
}
