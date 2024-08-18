using UnityEngine;

[ExecuteInEditMode]
public class FogVolumePriority : MonoBehaviour
{
	public Camera GameCamera;

	public int FogOrderCameraAbove = 1;

	public int FogOrderCameraBelow = -1;

	public float HeightThreshold = 30f;

	public FogVolume thisFog;

	private FogVolumeData _FogVolumeData;

	public float CurrentHeight;

	public GameObject Horizon;

	private void OnEnable()
	{
		thisFog = GetComponent<FogVolume>();
		_FogVolumeData = thisFog._FogVolumeData;
	}

	private void Update()
	{
		if ((bool)Horizon)
		{
			HeightThreshold = Horizon.transform.position.y;
		}
		if ((bool)_FogVolumeData)
		{
			GameCamera = _FogVolumeData.GameCamera;
		}
		else
		{
			GameCamera = Camera.main;
		}
		if (!GameCamera)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			if (Camera.current != null)
			{
				CurrentHeight = Camera.current.gameObject.transform.position.y;
			}
		}
		else
		{
			CurrentHeight = GameCamera.gameObject.transform.position.y;
		}
		if (HeightThreshold > CurrentHeight && Horizon != null)
		{
			thisFog.DrawOrder = FogOrderCameraBelow;
		}
		else
		{
			thisFog.DrawOrder = FogOrderCameraAbove;
		}
	}
}
