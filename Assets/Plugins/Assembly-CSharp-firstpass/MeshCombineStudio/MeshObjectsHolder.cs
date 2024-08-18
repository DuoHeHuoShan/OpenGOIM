using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio
{
	[Serializable]
	public class MeshObjectsHolder
	{
		public Material mat;

		public List<MeshObject> meshObjects = new List<MeshObject>();

		public ObjectOctree.LODParent lodParent;

		public List<CachedGameObject> newCachedGOs;

		public int lodLevel;

		public int lightmapIndex;

		public bool shadowCastingModeTwoSided;

		public bool hasChanged;

		public MeshObjectsHolder(CachedGameObject cachedGO, Material mat, int subMeshIndex, bool shadowCastingModeTwoSided, int lightmapIndex)
		{
			this.mat = mat;
			this.shadowCastingModeTwoSided = shadowCastingModeTwoSided;
			this.lightmapIndex = lightmapIndex;
			meshObjects.Add(new MeshObject(cachedGO, subMeshIndex));
		}
	}
}
