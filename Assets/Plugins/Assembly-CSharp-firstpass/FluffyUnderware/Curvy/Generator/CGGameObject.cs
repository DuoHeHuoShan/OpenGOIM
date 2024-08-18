using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo("#FFF59D")]
	public class CGGameObject : CGBounds
	{
		public GameObject Object;

		public Vector3 Translate;

		public Vector3 Rotate;

		public Vector3 Scale = Vector3.one;

		public Matrix4x4 Matrix
		{
			get
			{
				return Matrix4x4.TRS(Translate, Quaternion.Euler(Rotate), Scale);
			}
		}

		public CGGameObject()
		{
		}

		public CGGameObject(CGGameObjectProperties properties)
			: this(properties.Object, properties.Translation, properties.Rotation, properties.Scale)
		{
		}

		public CGGameObject(GameObject obj)
			: this(obj, Vector3.zero, Vector3.zero, Vector3.one)
		{
		}

		public CGGameObject(GameObject obj, Vector3 translate, Vector3 rotate, Vector3 scale)
		{
			Object = obj;
			Translate = translate;
			Rotate = rotate;
			Scale = scale;
			if ((bool)Object)
			{
				Name = Object.name;
			}
			RecalculateBounds();
		}

		public CGGameObject(CGGameObject source)
		{
			Object = source.Object;
			Name = source.Name;
			base.Bounds = source.Bounds;
			Translate = source.Translate;
			Rotate = source.Rotate;
			Scale = source.Scale;
		}

		public override T Clone<T>()
		{
			return new CGGameObject(this) as T;
		}

		public static CGGameObject Get(CGGameObject data, GameObject obj, Vector3 translate, Vector3 rotate, Vector3 scale)
		{
			if (data == null)
			{
				return new CGGameObject(obj);
			}
			data.Object = obj;
			data.Name = ((!(obj != null)) ? null : obj.name);
			data.Translate = translate;
			data.Rotate = rotate;
			data.Scale = scale;
			data.RecalculateBounds();
			return data;
		}

		public override void RecalculateBounds()
		{
			mBounds = default(Bounds);
			Bounds bounds = default(Bounds);
			if (Object == null)
			{
				return;
			}
			Renderer[] componentsInChildren = Object.GetComponentsInChildren<Renderer>(true);
			Collider[] componentsInChildren2 = Object.GetComponentsInChildren<Collider>(true);
			if (componentsInChildren.Length > 0)
			{
				bounds = componentsInChildren[0].bounds;
				for (int i = 1; i < componentsInChildren.Length; i++)
				{
					bounds.Encapsulate(componentsInChildren[i].bounds);
				}
				for (int j = 1; j < componentsInChildren2.Length; j++)
				{
					bounds.Encapsulate(componentsInChildren2[j].bounds);
				}
			}
			else if (componentsInChildren2.Length > 0)
			{
				bounds = componentsInChildren2[0].bounds;
				for (int k = 1; k < componentsInChildren2.Length; k++)
				{
					bounds.Encapsulate(componentsInChildren2[k].bounds);
				}
			}
			bounds.size = new Vector3(bounds.size.x * Scale.x, bounds.size.y * Scale.y, bounds.size.z * Scale.z);
			base.Bounds = bounds;
		}
	}
}
