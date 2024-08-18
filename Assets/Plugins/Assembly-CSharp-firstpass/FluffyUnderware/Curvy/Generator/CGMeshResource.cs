using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class CGMeshResource : DuplicateEditorMesh, IPoolable
	{
		private MeshRenderer mRenderer;

		private Collider mCollider;

		public MeshRenderer Renderer
		{
			get
			{
				if (mRenderer == null)
				{
					mRenderer = GetComponent<MeshRenderer>();
				}
				return mRenderer;
			}
		}

		public Collider Collider
		{
			get
			{
				if (mCollider == null)
				{
					mCollider = GetComponent<Collider>();
				}
				return mCollider;
			}
		}

		public Mesh Prepare()
		{
			return base.Filter.PrepareNewShared();
		}

		public bool ColliderMatches(CGColliderEnum type)
		{
			if (Collider == null && type == CGColliderEnum.None)
			{
				return true;
			}
			if (Collider is MeshCollider && type == CGColliderEnum.Mesh)
			{
				return true;
			}
			if (Collider is BoxCollider && type == CGColliderEnum.Box)
			{
				return true;
			}
			if (Collider is SphereCollider && type == CGColliderEnum.Sphere)
			{
				return true;
			}
			return false;
		}

		public void RemoveCollider()
		{
			if ((bool)Collider)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(mCollider);
				}
				else
				{
					Object.DestroyImmediate(mCollider);
				}
				mCollider = null;
			}
		}

		public bool UpdateCollider(CGColliderEnum mode, bool convex, PhysicMaterial material)
		{
			if (Collider == null)
			{
				switch (mode)
				{
				case CGColliderEnum.Mesh:
					if (canAddMeshCollider(base.Filter.sharedMesh.bounds))
					{
						MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
						meshCollider.convex = convex;
						mCollider = meshCollider;
					}
					break;
				case CGColliderEnum.Box:
					base.gameObject.AddComponent<BoxCollider>();
					break;
				case CGColliderEnum.Sphere:
					base.gameObject.AddComponent<SphereCollider>();
					break;
				}
			}
			if (Collider != null)
			{
				switch (mode)
				{
				case CGColliderEnum.Mesh:
				{
					MeshCollider meshCollider2 = Collider as MeshCollider;
					if (meshCollider2 != null)
					{
						meshCollider2.sharedMesh = null;
						if (!canAddMeshCollider(base.Filter.sharedMesh.bounds))
						{
							return false;
						}
						try
						{
							meshCollider2.sharedMesh = base.Filter.sharedMesh;
						}
						catch
						{
							return false;
						}
					}
					break;
				}
				case CGColliderEnum.Box:
				{
					BoxCollider boxCollider = Collider as BoxCollider;
					if (boxCollider != null)
					{
						boxCollider.center = base.Filter.sharedMesh.bounds.center;
						boxCollider.size = base.Filter.sharedMesh.bounds.size;
					}
					break;
				}
				case CGColliderEnum.Sphere:
				{
					SphereCollider sphereCollider = Collider as SphereCollider;
					if (sphereCollider != null)
					{
						sphereCollider.center = base.Filter.sharedMesh.bounds.center;
						sphereCollider.radius = base.Filter.sharedMesh.bounds.extents.magnitude;
					}
					break;
				}
				}
				Collider.material = material;
			}
			return true;
		}

		private bool canAddMeshCollider(Bounds b)
		{
			return b.extents.x > float.Epsilon && b.extents.y > float.Epsilon && b.extents.z > float.Epsilon;
		}

		protected override void Awake()
		{
			base.Awake();
		}

		public void OnBeforePush()
		{
		}

		public void OnAfterPop()
		{
		}
	}
}
