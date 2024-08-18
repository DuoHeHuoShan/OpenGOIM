using System;
using UnityEngine;

namespace MeshCombineStudio
{
	[Serializable]
	public class CachedGameObject
	{
		public GameObject go;

		public Transform t;

		public MeshRenderer mr;

		public MeshFilter mf;

		public Mesh mesh;

		public CachedGameObject(GameObject go, Transform t, MeshRenderer mr, MeshFilter mf, Mesh mesh)
		{
			this.go = go;
			this.t = t;
			this.mr = mr;
			this.mf = mf;
			this.mesh = mesh;
		}

		public CachedGameObject(CachedComponents cachedComponent)
		{
			go = cachedComponent.go;
			t = cachedComponent.t;
			mr = cachedComponent.mr;
			mf = cachedComponent.mf;
			mesh = cachedComponent.mf.sharedMesh;
		}
	}
}
