using UnityEngine;

public class TitleFade : MonoBehaviour
{
	private SpriteRenderer rend;

	private bool done;

	private GameObject player;

	private Color color;

	private float lerpAmount;

	private float prevLerpAmount = -1f;

	private float pos;

	private Color whiteClear = new Color(255f, 255f, 255f, 0f);

	private void Start()
	{
		rend = GetComponent<SpriteRenderer>();
		player = GameObject.FindWithTag("Player");
	}

	public void Restart()
	{
		rend.enabled = true;
		rend.color = Color.white;
		done = false;
		prevLerpAmount = -1f;
	}

	private void Update()
	{
		if (!done)
		{
			pos = player.transform.position.x;
			if (pos < -42f)
			{
				lerpAmount = 0f;
			}
			else if (pos > -32f)
			{
				lerpAmount = 1f;
			}
			else
			{
				lerpAmount = (10f + (32f + pos)) / 10f;
			}
			if (lerpAmount < prevLerpAmount)
			{
				lerpAmount = prevLerpAmount;
			}
			color = Color.Lerp(Color.white, whiteClear, lerpAmount);
			rend.color = color;
			if (lerpAmount > 1.01f)
			{
				done = true;
				rend.enabled = false;
			}
			prevLerpAmount = lerpAmount;
		}
	}
}
