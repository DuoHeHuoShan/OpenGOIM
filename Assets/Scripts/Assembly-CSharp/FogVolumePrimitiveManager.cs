using System.Collections.Generic;
using UnityEngine;

public class FogVolumePrimitiveManager : MonoBehaviour
{
	protected class PrimitiveData
	{
		public EFogVolumePrimitiveType PrimitiveType { get; set; }

		public FogVolumePrimitive Primitive { get; set; }

		public Transform Transform { get; set; }

		public Renderer Renderer { get; set; }

		public float SqDistance { get; set; }

		public float Distance2Camera { get; set; }

		public Bounds Bounds { get; set; }

		public PrimitiveData()
		{
			PrimitiveType = EFogVolumePrimitiveType.None;
			Primitive = null;
			Transform = null;
			Renderer = null;
			SqDistance = 0f;
			Distance2Camera = 0f;
			Bounds = default(Bounds);
		}

		public void Reset()
		{
			PrimitiveType = EFogVolumePrimitiveType.None;
			Primitive = null;
			Transform = null;
			Renderer = null;
			SqDistance = 0f;
			Distance2Camera = 0f;
		}
	}

	private FogVolume m_fogVolume;

	private FogVolumeData m_fogVolumeData;

	private Camera m_camera;

	private BoxCollider m_boxCollider;

	private Transform m_pointOfInterestTf;

	private Vector3 m_pointOfInterest = Vector3.zero;

	private List<PrimitiveData> m_primitives;

	private List<PrimitiveData> m_primitivesInFrustum;

	private int m_inFrustumCount;

	private Plane[] FrustumPlanes;

	private readonly Vector4[] m_primitivePos = new Vector4[20];

	private readonly Vector4[] m_primitiveScale = new Vector4[20];

	private readonly Matrix4x4[] m_primitiveTf = new Matrix4x4[20];

	private readonly Vector4[] m_primitiveData = new Vector4[20];

	private const int InvalidIndex = -1;

	private const int MaxVisiblePrimitives = 20;

	private const int MaxPrimitivesCount = 1000;

	public int CurrentPrimitiveCount { get; private set; }

	public int VisiblePrimitiveCount { get; private set; }

	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return m_pointOfInterestTf != null;
		}
	}

	public void FindPrimitivesInFogVolume()
	{
		CurrentPrimitiveCount = 0;
		VisiblePrimitiveCount = 0;
		m_primitives.Clear();
		m_primitivesInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			m_primitives.Add(new PrimitiveData());
			m_primitivesInFrustum.Add(new PrimitiveData());
		}
		if (m_boxCollider == null)
		{
			m_boxCollider = base.gameObject.GetComponent<BoxCollider>();
		}
		Bounds bounds = m_boxCollider.bounds;
		FogVolumePrimitive[] array = Object.FindObjectsOfType<FogVolumePrimitive>();
		foreach (FogVolumePrimitive fogVolumePrimitive in array)
		{
			if (bounds.Intersects(fogVolumePrimitive.Bounds))
			{
				if (fogVolumePrimitive.BoxColl != null)
				{
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Box;
				}
				else if (fogVolumePrimitive.SphereColl != null)
				{
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Sphere;
				}
				else
				{
					fogVolumePrimitive.BoxColl = fogVolumePrimitive.GetTransform.gameObject.AddComponent<BoxCollider>();
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Box;
				}
				if (fogVolumePrimitive.Type == EFogVolumePrimitiveType.Box)
				{
					AddPrimitiveBox(fogVolumePrimitive);
				}
				else if (fogVolumePrimitive.Type == EFogVolumePrimitiveType.Sphere)
				{
					AddPrimitiveSphere(fogVolumePrimitive);
				}
			}
		}
	}

	public bool AddPrimitiveBox(FogVolumePrimitive _box)
	{
		int num = _FindFirstFreePrimitive();
		if (num != -1)
		{
			PrimitiveData primitiveData = m_primitives[num];
			CurrentPrimitiveCount++;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Box;
			primitiveData.Transform = _box.transform;
			primitiveData.Renderer = _box.GetComponent<Renderer>();
			primitiveData.Primitive = _box;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _box.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	public bool AddPrimitiveSphere(FogVolumePrimitive _sphere)
	{
		int num = _FindFirstFreePrimitive();
		if (num != -1)
		{
			PrimitiveData primitiveData = m_primitives[num];
			CurrentPrimitiveCount++;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Sphere;
			primitiveData.Transform = _sphere.transform;
			primitiveData.Renderer = _sphere.GetComponent<Renderer>();
			primitiveData.Primitive = _sphere;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _sphere.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	public bool RemovePrimitive(Transform _primitiveToRemove)
	{
		int count = m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			PrimitiveData primitiveData = m_primitives[i];
			if (object.ReferenceEquals(m_primitives[i].Transform, _primitiveToRemove))
			{
				primitiveData.Reset();
				CurrentPrimitiveCount--;
				return true;
			}
		}
		return false;
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

	public void OnDrawGizmos()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		m_camera = ((!(m_fogVolumeData != null)) ? null : m_fogVolumeData.GameCamera);
		if (!(m_camera == null))
		{
			FrustumPlanes = _frustumPlanes;
			if (m_boxCollider == null)
			{
				m_boxCollider = m_fogVolume.GetComponent<BoxCollider>();
			}
			_UpdateBounds();
			_FindPrimitivesInFrustum();
			if (m_primitivesInFrustum.Count > 20)
			{
				_SortPrimitivesInFrustum();
			}
			_PrepareShaderArrays();
		}
	}

	public void SetVisibility(bool _enabled)
	{
		int count = m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_primitives[i].Renderer != null)
			{
				m_primitives[i].Renderer.enabled = _enabled;
			}
		}
	}

	public void Initialize()
	{
		m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		m_fogVolumeData = Object.FindObjectOfType<FogVolumeData>();
		m_camera = null;
		m_boxCollider = null;
		CurrentPrimitiveCount = 0;
		if (m_primitives == null)
		{
			m_primitives = new List<PrimitiveData>(1000);
			m_primitivesInFrustum = new List<PrimitiveData>();
			for (int i = 0; i < 1000; i++)
			{
				m_primitives.Add(new PrimitiveData());
				m_primitivesInFrustum.Add(new PrimitiveData());
			}
		}
	}

	public void Deinitialize()
	{
		VisiblePrimitiveCount = 0;
	}

	public Vector4[] GetPrimitivePositionArray()
	{
		return m_primitivePos;
	}

	public Vector4[] GetPrimitiveScaleArray()
	{
		return m_primitiveScale;
	}

	public Matrix4x4[] GetPrimitiveTransformArray()
	{
		return m_primitiveTf;
	}

	public Vector4[] GetPrimitiveDataArray()
	{
		return m_primitiveData;
	}

	private void _UpdateBounds()
	{
		int count = m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			PrimitiveData primitiveData = m_primitives[i];
			if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.None)
			{
				continue;
			}
			if (primitiveData.Primitive == null)
			{
				RemovePrimitive(primitiveData.Transform);
			}
			else if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.Box)
			{
				if (primitiveData.Primitive.BoxColl == null)
				{
					Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
					primitiveData.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Box);
				}
				primitiveData.Bounds = primitiveData.Primitive.BoxColl.bounds;
			}
			else if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.Sphere)
			{
				if (primitiveData.Primitive.SphereColl == null)
				{
					Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
					primitiveData.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Sphere);
				}
				primitiveData.Bounds = primitiveData.Primitive.SphereColl.bounds;
			}
		}
	}

	private int _FindFirstFreePrimitive()
	{
		if (CurrentPrimitiveCount < 1000)
		{
			int count = m_primitives.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_primitives[i].PrimitiveType == EFogVolumePrimitiveType.None)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void _FindPrimitivesInFrustum()
	{
		m_inFrustumCount = 0;
		Vector3 position = m_camera.gameObject.transform.position;
		int count = m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			PrimitiveData primitiveData = m_primitives[i];
			if (primitiveData.Transform == null)
			{
				primitiveData.PrimitiveType = EFogVolumePrimitiveType.None;
			}
			if (primitiveData.PrimitiveType != EFogVolumePrimitiveType.None)
			{
				if (primitiveData.Primitive.IsPersistent)
				{
					Vector3 position2 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position2 - m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position2 - position).magnitude;
					m_primitivesInFrustum[m_inFrustumCount++] = primitiveData;
				}
				else if (GeometryUtility.TestPlanesAABB(FrustumPlanes, m_primitives[i].Bounds))
				{
					Vector3 position3 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position3 - m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position3 - position).magnitude;
					m_primitivesInFrustum[m_inFrustumCount++] = primitiveData;
				}
			}
		}
	}

	private void _SortPrimitivesInFrustum()
	{
		bool flag = false;
		do
		{
			flag = true;
			for (int i = 0; i < m_inFrustumCount - 1; i++)
			{
				if (m_primitivesInFrustum[i].SqDistance > m_primitivesInFrustum[i + 1].SqDistance)
				{
					PrimitiveData value = m_primitivesInFrustum[i];
					m_primitivesInFrustum[i] = m_primitivesInFrustum[i + 1];
					m_primitivesInFrustum[i + 1] = value;
					flag = false;
				}
			}
		}
		while (!flag);
	}

	private void _PrepareShaderArrays()
	{
		VisiblePrimitiveCount = 0;
		Quaternion rotation = m_fogVolume.gameObject.transform.rotation;
		for (int i = 0; i < 20 && i < m_inFrustumCount; i++)
		{
			PrimitiveData primitiveData = m_primitivesInFrustum[i];
			Vector3 position = primitiveData.Transform.position;
			m_primitivePos[i] = base.gameObject.transform.InverseTransformPoint(position);
			m_primitiveTf[i].SetTRS(position, Quaternion.Inverse(primitiveData.Transform.rotation) * rotation, Vector3.one);
			m_primitiveScale[i] = primitiveData.Primitive.GetPrimitiveScale;
			m_primitiveData[i] = new Vector4((primitiveData.PrimitiveType != 0) ? 1.5f : 0.5f, (!primitiveData.Primitive.IsSubtractive) ? 0.5f : 1.5f, 0f, 0f);
			VisiblePrimitiveCount++;
		}
	}
}
