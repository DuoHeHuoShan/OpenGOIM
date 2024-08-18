using FluffyUnderware.Curvy;
using UnityEngine;

public class DialogLine : MonoBehaviour
{
	[HideInInspector]
	public float splinePosition;

	[TextArea(8, 12)]
	public string subtitles;

	public AudioClip clip;

	public CurvySpline spline;

	private void Start()
	{
	}
}
