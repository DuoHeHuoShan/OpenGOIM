using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FogVolumeData : MonoBehaviour
{
	[SerializeField]
	private bool _ForceNoRenderer;

	[SerializeField]
	private Camera _GameCamera;

	[SerializeField]
	private List<Camera> FoundCameras;

	[SerializeField]
	private FogVolume[] SceneFogVolumes;

	public bool ForceNoRenderer
	{
		get
		{
			return _ForceNoRenderer;
		}
		set
		{
			if (_ForceNoRenderer != value)
			{
				_ForceNoRenderer = value;
				ToggleFogVolumeRenderers();
			}
		}
	}

	public Camera GameCamera
	{
		get
		{
			return _GameCamera;
		}
		set
		{
			if (_GameCamera != value)
			{
				_GameCamera = value;
				RefreshCamera();
			}
		}
	}

	public Camera GetFogVolumeCamera
	{
		get
		{
			return GameCamera;
		}
	}

	public void setDownsample(int val)
	{
		if ((bool)_GameCamera.GetComponent<FogVolumeRenderer>())
		{
			_GameCamera.GetComponent<FogVolumeRenderer>()._Downsample = val;
		}
	}

	private void RefreshCamera()
	{
		FindFogVolumes();
		FogVolume[] sceneFogVolumes = SceneFogVolumes;
		foreach (FogVolume fogVolume in sceneFogVolumes)
		{
			fogVolume.AssignCamera();
		}
		ToggleFogVolumeRenderers();
	}

	private void OnEnable()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (FoundCameras == null)
		{
			FoundCameras = new List<Camera>();
		}
		FindCamera();
		RefreshCamera();
		if (FoundCameras.Count == 0)
		{
			Debug.Log("Definetly, no camera available for Fog Volume");
		}
	}

	public void FindFogVolumes()
	{
		SceneFogVolumes = (FogVolume[])Object.FindObjectsOfType(typeof(FogVolume));
	}

	private void Update()
	{
		if (GameCamera == null)
		{
			Debug.Log("No Camera available for Fog Volume. Trying to find another one");
			Initialize();
		}
	}

	private void ToggleFogVolumeRenderers()
	{
		if (FoundCameras == null)
		{
			return;
		}
		for (int i = 0; i < FoundCameras.Count; i++)
		{
			if (FoundCameras[i] != _GameCamera)
			{
				if ((bool)FoundCameras[i].GetComponent<FogVolumeRenderer>())
				{
					FoundCameras[i].GetComponent<FogVolumeRenderer>().enabled = false;
				}
				continue;
			}
			if ((bool)FoundCameras[i].GetComponent<FogVolumeRenderer>() && !_ForceNoRenderer)
			{
				FoundCameras[i].GetComponent<FogVolumeRenderer>().enabled = true;
				continue;
			}
			FogVolumeRenderer fogVolumeRenderer = FoundCameras[i].GetComponent<FogVolumeRenderer>();
			if (fogVolumeRenderer == null)
			{
				if (ForceNoRenderer)
				{
					continue;
				}
				fogVolumeRenderer = FoundCameras[i].gameObject.AddComponent<FogVolumeRenderer>();
			}
			if (ForceNoRenderer)
			{
				fogVolumeRenderer.enabled = false;
			}
		}
	}

	public void FindCamera()
	{
		if (FoundCameras != null && FoundCameras.Count > 0)
		{
			FoundCameras.Clear();
		}
		Camera[] array = (Camera[])Object.FindObjectsOfType(typeof(Camera));
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].name.Contains("FogVolumeCamera") && !array[i].name.Contains("Shadow Camera") && array[i].gameObject.hideFlags == HideFlags.None)
			{
				FoundCameras.Add(array[i]);
			}
		}
		if (GameCamera == null)
		{
			GameCamera = Camera.main;
		}
		if (GameCamera == null)
		{
			foreach (Camera foundCamera in FoundCameras)
			{
				if (foundCamera.isActiveAndEnabled && foundCamera.gameObject.activeInHierarchy && foundCamera.gameObject.hideFlags == HideFlags.None)
				{
					GameCamera = foundCamera;
					break;
				}
			}
		}
		if (GameCamera != null)
		{
			if ((bool)Object.FindObjectOfType<FogVolumeCamera>())
			{
				Object.FindObjectOfType<FogVolumeCamera>().SceneCamera = GameCamera;
			}
			GameCamera.depthTextureMode = DepthTextureMode.Depth;
		}
	}

	private void OnDisable()
	{
		FoundCameras.Clear();
		SceneFogVolumes = null;
	}
}
