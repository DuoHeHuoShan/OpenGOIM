using UnityEngine;

public class Match2DTransform : MonoBehaviour
{
	public Transform target;

	private void LateUpdate()
	{
		base.transform.rotation = target.rotation;
		base.transform.position = target.position;
	}
}
