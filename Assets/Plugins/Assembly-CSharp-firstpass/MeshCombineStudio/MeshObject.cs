using System;
using UnityEngine;

namespace MeshCombineStudio
{
	[Serializable]
	public class MeshObject
	{
		public CachedGameObject cachedGO;

		public MeshCache meshCache;

		public int subMeshIndex;

		public Vector3 position;

		public Vector3 scale;

		public Quaternion rotation;

		public Vector4 lightmapScaleOffset;

		public bool intersectsSurface;

		public int startNewTriangleIndex;

		public int newTriangleCount;

		public bool skip;

		public MeshObject(CachedGameObject cachedGO, int subMeshIndex)
		{
			this.cachedGO = cachedGO;
			this.subMeshIndex = subMeshIndex;
			Transform t = cachedGO.t;
			position = t.position;
			rotation = t.rotation;
			scale = t.lossyScale;
			lightmapScaleOffset = cachedGO.mr.lightmapScaleOffset;
		}
	}
}
