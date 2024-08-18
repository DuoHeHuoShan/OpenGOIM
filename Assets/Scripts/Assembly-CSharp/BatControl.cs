using UnityEngine;

public class BatControl : MonoBehaviour
{
	public Camera mainCam;

	private float speed = 3f;

	private void Start()
	{
		base.transform.rotation = Quaternion.Euler(Random.Range(-25f, 25f), Random.Range(-25f, 25f), Random.Range(-25f, 25f));
	}

	private void Update()
	{
		base.transform.position += base.transform.forward * (0f - speed);
		if (base.transform.position.z < mainCam.transform.position.z)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
