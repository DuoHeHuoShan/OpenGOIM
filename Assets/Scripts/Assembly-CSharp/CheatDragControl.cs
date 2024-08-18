using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CheatDragControl : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public CheatDragRuntime cheatDragRuntime;

	private int tapCount;

	private void OnEnable()
	{
		tapCount = 0;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (base.isActiveAndEnabled)
		{
			tapCount++;
			if (tapCount >= 10)
			{
				tapCount = 0;
				cheatDragRuntime.gameObject.SetActive(!cheatDragRuntime.gameObject.activeSelf);
				Debug.LogFormat("[NoodleDebug] Drag cheat is {0}", (!cheatDragRuntime.gameObject.activeSelf) ? "DEACTIVATED" : "ACTIVATED");
				StartCoroutine(IndicateActivated());
			}
		}
	}

	private IEnumerator IndicateActivated()
	{
		Vector3 s = base.transform.localScale;
		base.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
		yield return new WaitForSecondsRealtime(0.5f);
		base.transform.localScale = s;
	}
}
