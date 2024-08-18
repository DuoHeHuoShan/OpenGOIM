using UnityEngine;

namespace MeshCombineStudio
{
	public class DisabledLODGroup : MonoBehaviour
	{
		[HideInInspector]
		public MeshCombiner meshCombiner;

		public LODGroup lodGroup;
	}
}
