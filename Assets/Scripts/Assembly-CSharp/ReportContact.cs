using UnityEngine;

public class ReportContact : MonoBehaviour
{
	public StuckDetector detector;

	public int detectorNum;

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			detector.triggers[detectorNum] = true;
		}
	}

	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			detector.triggers[detectorNum] = false;
		}
	}
}
