using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FogVolumePrimitive : MonoBehaviour
{
	public BoxCollider BoxColl;

	public SphereCollider SphereColl;

	public bool IsPersistent = true;

	public EFogVolumePrimitiveType Type;

	public bool IsSubtractive;

	public Material PrimitiveMaterial;

	private GameObject Primitive;

	private Renderer _Renderer;

	private readonly float MinScale = 0.0001f;

	public Transform GetTransform
	{
		get
		{
			return base.gameObject.transform;
		}
	}

	public Vector3 GetPrimitiveScale
	{
		get
		{
			return new Vector3(Mathf.Max(MinScale, base.transform.lossyScale.x), Mathf.Max(MinScale, base.transform.lossyScale.y), Mathf.Max(MinScale, base.transform.lossyScale.z));
		}
	}

	public Bounds Bounds
	{
		get
		{
			if (BoxColl != null)
			{
				return BoxColl.bounds;
			}
			if (SphereColl != null)
			{
				return SphereColl.bounds;
			}
			return new Bounds(base.gameObject.transform.position, base.gameObject.transform.lossyScale);
		}
	}

	public FogVolumePrimitive()
	{
		SphereColl = null;
		BoxColl = null;
	}

	public void AddColliderIfNeccessary(EFogVolumePrimitiveType _type)
	{
		Type = _type;
		switch (Type)
		{
		case EFogVolumePrimitiveType.Box:
			if (BoxColl == null)
			{
				BoxColl = base.gameObject.AddComponent<BoxCollider>();
			}
			break;
		case EFogVolumePrimitiveType.Sphere:
			if (SphereColl == null)
			{
				SphereColl = base.gameObject.AddComponent<SphereCollider>();
			}
			break;
		}
	}

	private void OnEnable()
	{
		Primitive = base.gameObject;
		_Renderer = Primitive.GetComponent<MeshRenderer>();
		if (!PrimitiveMaterial)
		{
			PrimitiveMaterial = (Material)Resources.Load("PrimitiveMaterial");
		}
		_Renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		_Renderer.lightProbeUsage = LightProbeUsage.Off;
		_Renderer.shadowCastingMode = ShadowCastingMode.Off;
		_Renderer.receiveShadows = false;
		GetComponent<MeshRenderer>().material = PrimitiveMaterial;
		BoxColl = GetComponent<BoxCollider>();
		SphereColl = GetComponent<SphereCollider>();
		if (BoxColl == null && SphereColl == null)
		{
			BoxColl = base.gameObject.AddComponent<BoxCollider>();
			Type = EFogVolumePrimitiveType.Box;
		}
		else if (BoxColl != null)
		{
			Type = EFogVolumePrimitiveType.Box;
		}
		else if (SphereColl != null)
		{
			Type = EFogVolumePrimitiveType.Sphere;
		}
		else
		{
			Type = EFogVolumePrimitiveType.None;
		}
	}
}
