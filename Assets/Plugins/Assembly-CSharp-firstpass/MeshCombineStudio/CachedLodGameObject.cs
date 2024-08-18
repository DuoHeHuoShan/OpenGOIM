using System;

namespace MeshCombineStudio
{
	[Serializable]
	public class CachedLodGameObject : CachedGameObject
	{
		public int lodCount;

		public int lodLevel;

		public CachedLodGameObject(CachedGameObject cachedGO, int lodCount, int lodLevel)
			: base(cachedGO.go, cachedGO.t, cachedGO.mr, cachedGO.mf, cachedGO.mesh)
		{
			this.lodCount = lodCount;
			this.lodLevel = lodLevel;
		}
	}
}
