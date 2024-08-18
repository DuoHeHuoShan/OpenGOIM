using UnityEngine;

namespace MeshCombineStudio
{
	public class DisabledMeshRenderer : MonoBehaviour
	{
		[HideInInspector]
		public MeshCombiner meshCombiner;

		public CachedGameObject cachedGO;
	}
}
