using UnityEngine;

[ExecuteInEditMode]
public class ToggleChildren : MonoBehaviour
{
	public KeyCode Key = KeyCode.F;

	private GameObject[] Children;

	private bool active = true;

	private void OnEnable()
	{
		Children = new GameObject[base.gameObject.transform.childCount];
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			Children[i] = base.gameObject.transform.GetChild(i).gameObject;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(Key))
		{
			active = !active;
			for (int i = 0; i < base.gameObject.transform.childCount; i++)
			{
				Children[i].SetActive(active);
			}
		}
	}
}
