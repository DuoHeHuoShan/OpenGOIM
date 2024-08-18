using UnityEngine;

public class StuckDetector : MonoBehaviour
{
	public bool[] triggers;

	public Narrator narrator;

	private float timer;

	private bool done;

	private void Start()
	{
		timer = 0f;
		done = false;
	}

	private void Update()
	{
		if (triggers[0] && triggers[1])
		{
			timer += Time.deltaTime;
		}
		else
		{
			timer = 0f;
		}
		if (timer > 2f && !done)
		{
			narrator.StuckOnAntenna();
			done = true;
		}
	}
}
