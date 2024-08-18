using System.Collections.Generic;
using UnityEngine;

public class FogVolumeLightManager : MonoBehaviour
{
	protected class LightData
	{
		public EFogVolumeLightType LightType { get; set; }

		public Light Light { get; set; }

		public FogVolumeLight FogVolumeLight { get; set; }

		public Transform Transform { get; set; }

		public float SqDistance { get; set; }

		public float Distance2Camera { get; set; }

		public Bounds Bounds { get; set; }

		public LightData()
		{
			LightType = EFogVolumeLightType.None;
			Light = null;
			FogVolumeLight = null;
			Transform = null;
			SqDistance = 0f;
			Distance2Camera = 0f;
			Bounds = default(Bounds);
		}
	}

	private float m_pointLightCullSizeMultiplier = 1f;

	private FogVolume m_fogVolume;

	private FogVolumeData m_fogVolumeData;

	private Camera m_camera;

	private BoxCollider m_boxCollider;

	private Transform m_pointOfInterestTf;

	private Vector3 m_pointOfInterest = Vector3.zero;

	private readonly Vector4[] m_lightPos = new Vector4[64];

	private readonly Vector4[] m_lightRot = new Vector4[64];

	private readonly Color[] m_lightColor = new Color[64];

	private readonly Vector4[] m_lightData = new Vector4[64];

	private List<LightData> m_lights;

	private List<LightData> m_lightsInFrustum;

	private int m_inFrustumCount;

	private Plane[] FrustumPlanes;

	private const int InvalidIndex = -1;

	private const int MaxVisibleLights = 64;

	private const float InvalidSpotLightAngle = -1f;

	private const float NoData = 0f;

	private const float PointLightRangeDivider = 5f;

	private const float SpotLightRangeDivider = 5f;

	private const int MaxLightCount = 1000;

	private const float LightInVolumeBoundsSize = 5f;

	public int CurrentLightCount { get; private set; }

	public int VisibleLightCount { get; private set; }

	public bool DrawDebugData { get; set; }

	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return m_pointOfInterestTf != null;
		}
	}

	public void FindLightsInScene()
	{
		CurrentLightCount = 0;
		VisibleLightCount = 0;
		m_lights.Clear();
		m_lightsInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			m_lights.Add(new LightData());
			m_lightsInFrustum.Add(new LightData());
		}
		FogVolumeLight[] array = Object.FindObjectsOfType<FogVolumeLight>();
		for (int j = 0; j < array.Length; j++)
		{
			Light component = array[j].GetComponent<Light>();
			if (component != null)
			{
				switch (component.type)
				{
				case LightType.Point:
					AddPointLight(component);
					array[j].IsAddedToNormalLight = true;
					break;
				case LightType.Spot:
					AddSpotLight(component);
					array[j].IsAddedToNormalLight = true;
					break;
				}
			}
			else if (array[j].IsPointLight)
			{
				AddSimulatedPointLight(array[j]);
				array[j].IsAddedToNormalLight = false;
			}
			else
			{
				AddSimulatedSpotLight(array[j]);
				array[j].IsAddedToNormalLight = false;
			}
		}
	}

	public void FindLightsInFogVolume()
	{
		CurrentLightCount = 0;
		VisibleLightCount = 0;
		m_lights.Clear();
		m_lightsInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			m_lights.Add(new LightData());
			m_lightsInFrustum.Add(new LightData());
		}
		if (m_boxCollider == null)
		{
			m_boxCollider = base.gameObject.GetComponent<BoxCollider>();
		}
		Bounds bounds = m_boxCollider.bounds;
		FogVolumeLight[] array = Object.FindObjectsOfType<FogVolumeLight>();
		for (int j = 0; j < array.Length; j++)
		{
			if (!bounds.Intersects(new Bounds(array[j].gameObject.transform.position, Vector3.one * 5f)))
			{
				continue;
			}
			Light component = array[j].GetComponent<Light>();
			if (component != null)
			{
				switch (component.type)
				{
				case LightType.Point:
					AddPointLight(component);
					array[j].IsAddedToNormalLight = true;
					break;
				case LightType.Spot:
					AddSpotLight(component);
					array[j].IsAddedToNormalLight = true;
					break;
				}
			}
			else if (array[j].IsPointLight)
			{
				AddSimulatedPointLight(array[j]);
				array[j].IsAddedToNormalLight = false;
			}
			else
			{
				AddSimulatedSpotLight(array[j]);
				array[j].IsAddedToNormalLight = false;
			}
		}
	}

	public bool AddSimulatedPointLight(FogVolumeLight _light)
	{
		int num = _FindFirstFreeLight();
		if (num != -1)
		{
			LightData lightData = m_lights[num];
			CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.FogVolumePointLight;
			lightData.Transform = _light.transform;
			lightData.Light = null;
			lightData.FogVolumeLight = _light;
			lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.FogVolumeLight.Range * 2.5f);
			return true;
		}
		return false;
	}

	public bool AddSimulatedSpotLight(FogVolumeLight _light)
	{
		int num = _FindFirstFreeLight();
		if (num != -1)
		{
			LightData lightData = m_lights[num];
			CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.FogVolumeSpotLight;
			lightData.Transform = _light.transform;
			lightData.Light = null;
			lightData.FogVolumeLight = _light;
			Vector3 center = lightData.Transform.position + lightData.Transform.forward * lightData.FogVolumeLight.Range * 0.5f;
			lightData.Bounds = new Bounds(center, Vector3.one * lightData.FogVolumeLight.Range * (0.75f + lightData.FogVolumeLight.Angle * 0.03f));
			return true;
		}
		return false;
	}

	public bool AddPointLight(Light _light)
	{
		int num = _FindFirstFreeLight();
		if (num != -1)
		{
			LightData lightData = m_lights[num];
			CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.PointLight;
			lightData.Transform = _light.transform;
			lightData.Light = _light;
			lightData.FogVolumeLight = null;
			lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.Light.range * 2.5f);
			return true;
		}
		return false;
	}

	public bool AddSpotLight(Light _light)
	{
		int num = _FindFirstFreeLight();
		if (num != -1)
		{
			LightData lightData = m_lights[num];
			CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.SpotLight;
			lightData.Transform = _light.transform;
			lightData.Light = _light;
			lightData.FogVolumeLight = null;
			Vector3 center = lightData.Transform.position + lightData.Transform.forward * lightData.Light.range * 0.5f;
			lightData.Bounds = new Bounds(center, Vector3.one * lightData.Light.range * (0.75f + lightData.Light.spotAngle * 0.03f));
			return true;
		}
		return false;
	}

	public bool RemoveLight(Transform _lightToRemove)
	{
		int count = m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (object.ReferenceEquals(m_lights[i].Transform, _lightToRemove))
			{
				m_lights[i].LightType = EFogVolumeLightType.None;
				CurrentLightCount--;
				return true;
			}
		}
		return false;
	}

	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		FrustumPlanes = _frustumPlanes;
		m_camera = ((!(m_fogVolumeData != null)) ? null : m_fogVolumeData.GameCamera);
		if (!(m_camera == null))
		{
			if (m_boxCollider == null)
			{
				m_boxCollider = m_fogVolume.GetComponent<BoxCollider>();
			}
			if (m_pointOfInterestTf != null)
			{
				m_pointOfInterest = m_pointOfInterestTf.position;
			}
			_UpdateBounds();
			_FindLightsInFrustum();
			if (m_lightsInFrustum.Count > 64)
			{
				_SortLightsInFrustum();
			}
			_PrepareShaderArrays();
		}
	}

	public void OnDrawGizmos()
	{
		base.hideFlags = HideFlags.HideInInspector;
		if (!(m_camera == null) && DrawDebugData)
		{
			Color color = Gizmos.color;
			Gizmos.color = Color.green;
			for (int i = 0; i < VisibleLightCount; i++)
			{
				Gizmos.DrawWireCube(m_lightsInFrustum[i].Bounds.center, m_lightsInFrustum[i].Bounds.size);
			}
			Gizmos.color = Color.magenta;
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(m_camera.transform.position, m_camera.transform.rotation, Vector3.one);
			Gizmos.DrawFrustum(m_camera.transform.position, m_camera.fieldOfView, m_camera.nearClipPlane, m_fogVolume.PointLightingDistance2Camera, m_camera.aspect);
			Gizmos.color = color;
			Gizmos.matrix = matrix;
		}
	}

	public void SetPointLightCullSizeMultiplier(float _cullSizeMultiplier)
	{
		m_pointLightCullSizeMultiplier = _cullSizeMultiplier;
	}

	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		m_pointOfInterestTf = null;
		m_pointOfInterest = _pointOfInterest;
	}

	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		m_pointOfInterestTf = _pointOfInterest;
	}

	public Vector4[] GetLightPositionArray()
	{
		return m_lightPos;
	}

	public Vector4[] GetLightRotationArray()
	{
		return m_lightRot;
	}

	public Color[] GetLightColorArray()
	{
		return m_lightColor;
	}

	public Vector4[] GetLightData()
	{
		return m_lightData;
	}

	public void Initialize()
	{
		m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		m_fogVolumeData = Object.FindObjectOfType<FogVolumeData>();
		m_camera = null;
		m_boxCollider = null;
		CurrentLightCount = 0;
		DrawDebugData = false;
		if (m_lights == null)
		{
			m_lights = new List<LightData>(1000);
			m_lightsInFrustum = new List<LightData>(1000);
			for (int i = 0; i < 1000; i++)
			{
				m_lights.Add(new LightData());
				m_lightsInFrustum.Add(new LightData());
			}
		}
	}

	public void Deinitialize()
	{
		VisibleLightCount = 0;
		DrawDebugData = false;
	}

	public void SetFrustumPlanes(ref Plane[] _frustumPlanes)
	{
		FrustumPlanes = _frustumPlanes;
	}

	private void _UpdateBounds()
	{
		int count = m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			LightData lightData = m_lights[i];
			if (lightData.LightType != 0)
			{
				switch (lightData.LightType)
				{
				case EFogVolumeLightType.PointLight:
					lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.Light.range * m_pointLightCullSizeMultiplier);
					break;
				case EFogVolumeLightType.SpotLight:
				{
					Vector3 center2 = lightData.Transform.position + lightData.Transform.forward * lightData.Light.range * 0.5f;
					lightData.Bounds = new Bounds(center2, Vector3.one * lightData.Light.range * m_pointLightCullSizeMultiplier * 1.25f);
					break;
				}
				case EFogVolumeLightType.FogVolumePointLight:
					lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.FogVolumeLight.Range * m_pointLightCullSizeMultiplier);
					break;
				case EFogVolumeLightType.FogVolumeSpotLight:
				{
					Vector3 center = lightData.Transform.position + lightData.Transform.forward * lightData.FogVolumeLight.Range * 0.5f;
					lightData.Bounds = new Bounds(center, Vector3.one * lightData.FogVolumeLight.Range * m_pointLightCullSizeMultiplier * 1.25f);
					break;
				}
				}
			}
		}
	}

	private int _FindFirstFreeLight()
	{
		if (CurrentLightCount < 1000)
		{
			int count = m_lights.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_lights[i].LightType == EFogVolumeLightType.None)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void _FindLightsInFrustum()
	{
		m_inFrustumCount = 0;
		Vector3 position = m_camera.gameObject.transform.position;
		int count = m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_lights[i].Transform == null)
			{
				m_lights[i].LightType = EFogVolumeLightType.None;
			}
			if (m_lights[i].LightType == EFogVolumeLightType.None)
			{
				continue;
			}
			float magnitude = (m_lights[i].Transform.position - position).magnitude;
			if (magnitude > m_fogVolume.PointLightingDistance2Camera)
			{
				continue;
			}
			switch (m_lights[i].LightType)
			{
			case EFogVolumeLightType.PointLight:
			case EFogVolumeLightType.SpotLight:
				if (!m_lights[i].Light.enabled)
				{
					continue;
				}
				break;
			case EFogVolumeLightType.FogVolumePointLight:
				if (!m_lights[i].FogVolumeLight.Enabled)
				{
					continue;
				}
				break;
			case EFogVolumeLightType.FogVolumeSpotLight:
				if (!m_lights[i].FogVolumeLight.Enabled)
				{
					continue;
				}
				break;
			case EFogVolumeLightType.None:
				continue;
			}
			if (!GeometryUtility.TestPlanesAABB(FrustumPlanes, m_lights[i].Bounds))
			{
				continue;
			}
			LightData lightData = m_lights[i];
			Vector3 position2 = lightData.Transform.position;
			lightData.SqDistance = (position2 - m_pointOfInterest).sqrMagnitude;
			lightData.Distance2Camera = (position2 - position).magnitude;
			m_lightsInFrustum[m_inFrustumCount++] = lightData;
			if (lightData.FogVolumeLight != null)
			{
				if (lightData.LightType == EFogVolumeLightType.FogVolumePointLight && !lightData.FogVolumeLight.IsPointLight)
				{
					lightData.LightType = EFogVolumeLightType.FogVolumeSpotLight;
				}
				else if (lightData.LightType == EFogVolumeLightType.FogVolumeSpotLight && lightData.FogVolumeLight.IsPointLight)
				{
					lightData.LightType = EFogVolumeLightType.FogVolumePointLight;
				}
			}
		}
	}

	private void _SortLightsInFrustum()
	{
		bool flag = false;
		do
		{
			flag = true;
			for (int i = 0; i < m_inFrustumCount - 1; i++)
			{
				if (m_lightsInFrustum[i].SqDistance > m_lightsInFrustum[i + 1].SqDistance)
				{
					LightData value = m_lightsInFrustum[i];
					m_lightsInFrustum[i] = m_lightsInFrustum[i + 1];
					m_lightsInFrustum[i + 1] = value;
					flag = false;
				}
			}
		}
		while (!flag);
	}

	private void _PrepareShaderArrays()
	{
		VisibleLightCount = 0;
		for (int i = 0; i < 64 && i < m_inFrustumCount; i++)
		{
			LightData lightData = m_lightsInFrustum[i];
			switch (lightData.LightType)
			{
			case EFogVolumeLightType.FogVolumePointLight:
			{
				FogVolumeLight fogVolumeLight2 = lightData.FogVolumeLight;
				m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				m_lightColor[i] = fogVolumeLight2.Color;
				m_lightData[i] = new Vector4(fogVolumeLight2.Intensity * m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / m_fogVolume.PointLightingDistance2Camera)), fogVolumeLight2.Range / 5f, -1f, 0f);
				VisibleLightCount++;
				break;
			}
			case EFogVolumeLightType.FogVolumeSpotLight:
			{
				FogVolumeLight fogVolumeLight = lightData.FogVolumeLight;
				m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				m_lightColor[i] = fogVolumeLight.Color;
				m_lightData[i] = new Vector4(fogVolumeLight.Intensity * m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / m_fogVolume.PointLightingDistance2Camera)), fogVolumeLight.Range / 5f, fogVolumeLight.Angle, 0f);
				VisibleLightCount++;
				break;
			}
			case EFogVolumeLightType.PointLight:
			{
				Light light2 = lightData.Light;
				m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				m_lightColor[i] = light2.color;
				m_lightData[i] = new Vector4(light2.intensity * m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / m_fogVolume.PointLightingDistance2Camera)), light2.range / 5f, -1f, 0f);
				VisibleLightCount++;
				break;
			}
			case EFogVolumeLightType.SpotLight:
			{
				Light light = lightData.Light;
				m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				m_lightColor[i] = light.color;
				m_lightData[i] = new Vector4(light.intensity * m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / m_fogVolume.PointLightingDistance2Camera)), light.range / 5f, light.spotAngle, 0f);
				VisibleLightCount++;
				break;
			}
			}
		}
	}
}
