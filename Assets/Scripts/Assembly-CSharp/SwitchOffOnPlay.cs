using UnityEngine;

public class SwitchOffOnPlay : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.SetActive(false);
	}
}
