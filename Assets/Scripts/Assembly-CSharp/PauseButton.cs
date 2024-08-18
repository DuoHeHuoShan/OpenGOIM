using UnityEngine;

public class PauseButton : MonoBehaviour
{
	public SettingsManager settingsMan;

	private RaycastHit hit;

	private Ray ray;

	private int layerMask;

	private Vector3 originalButtonScale;

	private bool wasTouched;

	private void Start()
	{
		if (settingsMan == null)
		{
			settingsMan = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
		}
		Vector3 position = Camera.main.ScreenToWorldPoint(default(Vector2));
		position.x += 1f;
		position.y = base.transform.position.y;
		position.z = base.transform.position.z;
		base.transform.position = position;
		layerMask = LayerMask.GetMask("Pause");
		originalButtonScale = base.transform.localScale;
	}

	private void Update()
	{
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit, 100f, layerMask))
			{
				Debug.Log("[debug] OnMouseDown");
				wasTouched = true;
				base.transform.localScale = originalButtonScale * 1.2f;
			}
		}
		else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && wasTouched)
		{
			Debug.Log("[debug] OnMouseUp");
			wasTouched = false;
			base.transform.localScale = originalButtonScale;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit, 100f, layerMask))
			{
				Debug.Log("[debug] ToggleMenu");
				settingsMan.ToggleMenu();
			}
		}
	}
}
