using UnityEngine;

namespace MeshCombineStudio
{
	[ExecuteInEditMode]
	public class GarbageCollectMesh : MonoBehaviour
	{
		public Mesh mesh;

		private void OnDestroy()
		{
			if (mesh != null)
			{
				Object.Destroy(mesh);
			}
		}
	}
}
