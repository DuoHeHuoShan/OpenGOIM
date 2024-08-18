using FluffyUnderware.Curvy;
using UnityEngine;

[ExecuteInEditMode]
public class SplineSnapper : MonoBehaviour
{
	public CurvySpline levelSpline;

	private void Start()
	{
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		float nearestPointTF = levelSpline.GetNearestPointTF(position);
		position.z = levelSpline.Interpolate(nearestPointTF).z;
		base.transform.position = position;
	}
}
